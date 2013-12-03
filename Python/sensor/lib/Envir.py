import traceback
import threading

# exceptions
class NotListenEventError(Exception):
    def __init__(self, event, handler):
        Exception.__init__(self, "handler: "+repr(handler)+" not listening event: "+repr(event))
        self.event = event
        self.handler = handler

class EventNotInEnvError(Exception):
    def __init__(self, event):
        Exception.__init__(self, "event: "+repr(event)+" not in environment")
        self.event = event

class NoResponseError(Exception):
    def __init__(self, event):
        Exception.__init__(self, "triggered event: "+repr(event)+" has no listener")
        self.event = event

class TimeoutError(Exception):
    def __init__(self):
        Exception.__init__(self, "timeout error")
        
class HandlingError(Exception):
    def __init__(self, event, handler, e, log):
        Exception.__init__(self, "error for handling the event: "+repr(event)+" handler: "+repr(handler)+"\nDetail:\n"+log)
        self.event = event
        self.handler = handler
        self.e = e
        self.log = log

# classes
class Envir(object):
    def __init__(self):
        super(Envir, self).__init__()
        self.__env = {}

    def handlers(self, event):
        if not self.__env.has_key(event):
            raise EventNotInEnvError(event)
        return self.__env[event]
    
    def register(self, event, handler):
        if self.__env.has_key(event):
            self.__env[event].append(handler)
        else:
            self.__env[event] = [handler, ]

    def cancel(self, event, handler):
        lst_handler = self.handlers(event)
        if not handler in lst_handler:
            raise NotListenEventError(event, handler)
        lst_handler.remove(handler)

    def trigger(self, event, args, kwargs, blocking, timeout, err_handler):
        handlers = []
        for handler in self.handlers(event):
            handlers.append(handler)    #get a deepcopy for handler list
        if len(handlers) == 0:
            raise NoResponseError(event)
        
        n_thread = [len(handlers), ]
        block = threading.Event()
        for handler in handlers:
            th_args = (event,handler,args,kwargs,err_handler,timeout,block,n_thread)
            thread = threading.Thread(target = self.__apply, args = th_args)
            thread.setDaemon(True)
            thread.start()

        if blocking:
            block.wait()

    def __apply(self, event, handler, args, kwargs, err_handler, timeout, block, n_thread):
        try:
            alarm = None
            if timeout > 0:
                alarm = threading.Timer(timeout, self.__wakeup, args=(block,event,handler,err_handler))
                alarm.start()
            apply(handler, args, kwargs)
        except Exception, e:
            self.__error(event, handler, e, traceback.format_exc(), err_handler)
        finally:
            if not alarm is None:
                alarm.cancel()
            n_thread[0] = n_thread[0]-1
            if n_thread[0] == 0:
                block.set()
    
    def __wakeup(self, block, event, handler, err_handler):
        try:
            raise TimeoutError()
        except TimeoutError, e:
            self.__error(event, handler, e, traceback.format_exc(), err_handler)
            block.set()

    def __error(self, event, handler, e, log, err_handler):
        error = HandlingError(event, handler, e, log)
        apply(err_handler, (error, ))