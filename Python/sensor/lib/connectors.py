import socket
import time
import threading
import subprocess
import traceback

from threads import new as newThread, STA as STAThread, STAPass
from parsers import SocketConfigParser

class FailConnectError(Exception):
    def __init__(self, cls):
        Exception.__init__(self, repr(cls)+" fail to connect")

class MessageHandlerError(Exception):
    def __init__(self, message, handler, log):
        Exception.__init__(self, "error for handling the message: "+repr(message)+" handler: "+repr(handler)+"\nDetail:\n"+log)
        self.message = message
        self.handler = handler
        self.log = log
        
class Connector(object):
    @property
    def listener(self):
        return self._listener
    @listener.setter
    def listener(self, func):
        self._listener = func

    @property
    def error(self):
        return self._error
    @error.setter
    def error(self, func):
        self._error = func
    

    def send(self, message):
        pass

    def close(self):
        pass
    

class SocketClient(Connector):
    __delay = 3.0
    __bufsize = 1024
    
    def __init__(self, addr, cache_size = 0):
        super(SocketClient, self).__init__()
        self._sock = None
        self._addr = addr
        self._cache = []
        self._cache_cond = threading.Condition()
        self._cache_size = 0
        self._cache_max_size = cache_size
        self._listener = None
        self._error = None
        self.running = True
        self._start()

    @newThread
    def _start(self):
        self._connect()
        self._listen()

    @STAPass
    def _connect(self):
        while self.running:
            try:
                self._sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                self._sock.connect(self._addr)
            except Exception, e:
                if self._error:
                    newThread(apply)(self._error, (FailConnectError(self), ))
                time.sleep(SocketClient.__delay)
            else:
                self._cache_flush()
                break

    def _resume(self):
        if not self.running:
            break
        if self._error:
            newThread(apply)(self._error, (FailConnectError(self), ))
        self._connect()
        time.sleep(SocketClient.__delay)

    def _listen(self):
        while self.running:
            try:
                message = self._sock.recv(SocketClient.__bufsize)
            except Exception, e:
                self._resume()
                continue
            else:
                if message == "":
                    self._resume()
                else:
                    if self._listener is not None:
                        self._apply(message)
    
    @newThread
    def _apply(self, message):
        try:
            apply(self._listener, (message, ))
        except Exception, e:
            if self._error:
                apply(self._error, (MessageHandlerError(message, self._listener, traceback.format_exc()), ))

    @newThread
    def _cache_append(self, text):
        with self._cache_cond:
            self._cache.append(text)
            self._cache_size = self._cache_size + len(text)
            while self._cache_size > self._cache_max_size:
                length = len(self._cache[0])
                del self._cache[0]
                self._cache_size = self._cache_size - length

    @newThread
    def _cache_flush(self):
        with self._cache_cond:
            cache = self._cache
            self._cache = []
            self._cache_size = 0
        
        for msg in cache:   #this must out the "with" range for avoiding dead block
            self.send(msg)

    def send(self, message):
        if self.running:
            try:
                self._sock.send(message)
                time.sleep(0.1)
            except:
                self._cache_append(message)
                self._resume()

    def close(self):
        self.running = False
        self._sock.close()

        
class Pipe(Connector):
    __delay = 3.0

    def __init__(self, proc_cmd):
        super(Pipe, self).__init__()
        self._listener = None
        self._error =None
        self.running = True
        self._proc = None
        self._proc_cmd = proc_cmd
        self._start()

    @newThread
    def _start(self):
        self._connect()
        self._listen()

    @STAPass
    def _connect(self):
        if self._proc:
            self._proc.terminate()
        self._proc = subprocess.Popen(self._proc_cmd, stdin = subprocess.PIPE, stdout=subprocess.PIPE, shell = True)
        time.sleep(1.0)

    def _resume(self):
        if not self.running:
            break
        if self._error:
            newThread(apply)(self._error, (FailConnectError(self), ))
        self._connect()
        time.sleep(Pipe.__delay)

    def _listen(self):
        while self.running:
            try:
                message = self._proc.stdout.readline()
            except Exception, e:
                self._resume()
                continue
            else:
                if message == '' or self._proc.poll() !=None:
                    self._resume()
                else:
                    if self._listener is not None:
                        self._apply(message)

    @newThread
    def _apply(self, message):
        try:
            apply(self._listener, (message, ))
        except Exception, e:
            if self._error:
                apply(self._error, (MessageHandlerError(message, self._listener, traceback.format_exc()), ))

    def send(self, message):
        n_try = 10
        while self.running and n_try:
            try:
                self._proc.stdin.write(message+"\n")
                self._proc.stdout.flush()
                break
            except:
                self._resume()
                n_try = n_try-1

    def close(self):
        self.running = False
        if self._proc:
            self._proc.terminate()
        