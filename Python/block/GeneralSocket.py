# -*- coding: UTF-8 -*-
"""模拟socket通信逻辑的线程间通信模块

继承自Block模型。
实现了同一进程的不同线程间的通信机制。

version 1.0.1: 添加了 get_bind_block 和 get_bind_address 两个方法，分别返回绑定的模块名和地址名

version 1.0.2: 添加了在debug模式下的日志记录

Basic usage::

    >>> #thread1:
    >>> th1 = GeneralSocket("name1")
    >>> th1.listen()
    >>> while True:
    >>>     vc = th1.accept()
    >>>     recv_text = vc.recv()
    >>>     vc.close()
    >>> 
    >>> #thread2:
    >>> th2 = GeneralSocket()
    >>> th2.bind("name2")
    >>> th2.connect("name1")
    >>> th2.send(recv_text)
"""
__docformat__ = "restructuredtext en"

__all__ = [
    "GeneralSocket"
]

__author__ = "marpy"
__version__ = "1.0.2"
__date__ = "$Date: 2013-10-19$"

# standard library modules
import Queue
import threading
import socket

# user library modules
import VirtualSocket
import Debug

# exceptions
class SocketBindError(Exception):
    """端口绑定错误"""
    def __init__(self, address):
        Exception.__init__(self, "端口 "+repr(address)+" 无法绑定或已经被绑定过")

# classes
class GeneralSocket(object):
    """广义socket通信模块"""
    __LOCAL = 1     #连接模式：本地内存
    __REMOTE = 2    #连接模式：网络端口

    __MAX_LISTEN = 65535    #最大监听数
    __MAX_BUFF = 65535      #最大接收缓存

    def __init__(self, block_name = None, address = None):
        """
        构造函数，确定监听的模块端口和网络端口
        block_name: 绑定的模块端口，可缺省，缺省时不会监听模块端口
        address: 绑定的网络地址，形式为(ip, port)，可缺省，缺省时不会监听网络端口
        """
        self.__socket = None    #用于send和recv的socket
        self.__listen_type = 0
        self.__connect_type = 0
        self.__local_socket = VirtualSocket.VirtualSocket()
        self.__remote_socket = socket.socket()
        self.__accept_queue = None
        self.__address_bind = None
        self.__block_bind = None
        if not block_name == None:
            self.bind(block_name)
        if not address == None:
            self.bind(address)

    def bind(self, block_name = None, address = None):
        """
        绑定网络端口
        block_name: 绑定的模块端口，可缺省，缺省时不会监听模块端口
        address: 绑定的网络地址，形式为(ip, port)，可缺省，缺省时不会监听网络端口
        """
        try:
            if not block_name == None:
                self.__listen_type = self.__listen_type|GeneralSocket.__LOCAL
                self.__local_socket.bind(block_name)
                self.__block_bind = block_name
        except:
            raise SocketBindError(block_name)
        
        try:
            if not address ==None:
                self.__listen_type = self.__listen_type|GeneralSocket.__REMOTE
                self.__remote_socket.bind(address)
                self.__address_bind = address
        except:
            raise SocketBindError(address)
        
    def get_bind_block(self):
        """返回绑定的模块名"""
        return self.__block_bind

    def get_bind_address(self):
        """返回绑定的地址"""
        return self.__address_bind

    @Debug.writelog
    def connect(self, block_name = None, address = None):
        """
        与名为block_name的模块或地址为address的网络端口建立连接
        当block_name 和 address 都存在时，以address为主
        """
        if not address == None:
            self.__remote_socket.connect(address)
            self.__connect_type = GeneralSocket.__LOCAL
            self.__socket = self.__remote_socket
        elif not block_name == None:
            self.__local_socket.connect(block_name)
            self.__connect_type = GeneralSocket.__REMOTE
            self.__socket = self.__local_socket
        else:
            pass

    @Debug.writelog
    def listen(self, *args, **kwargs):
        """开始监听所有可能的连接"""
        self.__accept_queue = Queue.Queue(maxsize = 0)
        if self.__listen_type&GeneralSocket.__LOCAL == GeneralSocket.__LOCAL:
            self.__local_socket.listen()
            thread_local = threading.Thread(target = self.__local_accept)
            thread_local.setDaemon(True)
            thread_local.start()
        
        if self.__listen_type&GeneralSocket.__REMOTE == GeneralSocket.__REMOTE:
            self.__remote_socket.listen(GeneralSocket.__MAX_LISTEN)
            thread_remote = threading.Thread(target = self.__remote_accept)
            thread_remote.setDaemon(True)
            thread_remote.start()

    @Debug.writelog
    def accept(self):
        """获得连入的GeneralSocket，需要先执行listen"""
        _sock, name = self.__accept_queue.get()
        sock = GeneralSocket()
        sock.__socket = _sock
        return sock, name

    def __local_accept(self):
        """获得VirtualSocket，需要先执行listen"""
        try:
            while True:
                sock, name = self.__local_socket.accept()
                self.__accept_queue.put((sock, name))
        except:
            pass

    def __remote_accept(self):
        """获得socket，需要先执行listen"""
        try:
            while True:
                sock, addr = self.__remote_socket.accept()
                self.__accept_queue.put((sock, addr))
        except:
            pass

    @Debug.writelog
    def send(self, text):
        """建立连接后，向GeneralSocket写入值，支持连续写入"""
        self.__socket.send(text)
        return self
    
    @Debug.writelog    
    def recv(self, *args, **kwargs):
        """建立连接后，从GeneralSocket读出值"""
        text = self.__socket.recv(GeneralSocket.__MAX_BUFF)
        return text

    @Debug.writelog
    def close(self):
        """关闭连接"""
        if not self.__socket == None:
            self.__socket.close()
        else:
            self.__local_socket.close()
            self.__remote_socket.close()
            self.__accept_queue = None

if __name__ == "__main__":
    from ThreadManagement import *
    from time import *
    t = ThreadManagement("t")
    s1 = GeneralSocket()
    s1.bind("s1",("",1324))
    s2 = GeneralSocket()
    s2.bind("s2")
    s3 = GeneralSocket()
    s3.bind("s3",("",1151))
    s1.listen()
    @new_thread
    def test1(cls):
        try:
            while True:
                a,name = s1.accept()
                text = a.recv()
                print "test1 receive "+text+" from "+repr(name)
                sleep(2)
                a.send("check1")
                a.close()
        except:
            print "out"

    @new_thread
    def test2(cls):
        s2.connect("s1")
        s2.send("msg1")
        text2 = s2.recv()
        print "test2 receive "+text2
        s2.close()

    @new_thread
    def test3(cls):
        s3.connect(address = ("localhost",1324))
        s3.send("msg2")
        text3 = s3.recv()
        print "test3 receive "+text3
        s3.close()


    test2(t)
    test3(t)
    test1(t)
    sleep(3)
    s1.close()