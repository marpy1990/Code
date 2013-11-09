# -*- coding: UTF-8 -*-
"""模拟socket通信逻辑的线程间通信模块

继承自Block模型。
实现了同一进程的不同线程间的通信机制。

version 1.0.1: send不再使用同步机制

version 1.0.2: 修正了close后的一个bug

Basic usage::

    >>> #thread1:
    >>> th1 = VirtualSocket("name1")
    >>> th1.listen()
    >>> while True:
    >>>     vc = th1.accept()
    >>>     recv_text = vc.recv()
    >>>     vc.close()
    >>> 
    >>> #thread2:
    >>> th2 = VirtualSocket()
    >>> th2.bind("name2")
    >>> th2.connect("name1")
    >>> th2.send(recv_text)
"""
__docformat__ = "restructuredtext en"

__all__ = [
    "VirtualSocket"
]

__author__ = "marpy"
__version__ = "1.0.2"
__date__ = "$Date: 2013-10-19$"

# standard library modules
import Queue

# exceptions
class VirtualSocketNameError(Exception):
    """VirtualSocket名字已经被绑定过"""
    def __init__(self, name):
        Exception.__init__(self, "名称 "+name+" 已经被绑定过")

class CannotFindVirtualSocketError(Exception):
    """名称表中找不到对应VirtualSocket"""
    def __init__(self, name):
        Exception.__init__(self, "表中找不到名为 "+name+" 的VirtualSocket")

# classes
class VirtualSocket(object):
    """模拟socket通信逻辑的线程间通信模块"""

    socket_name_table = { }    #虚拟socket名称表，VirtualSocket名称在同进程中是唯一的

    def __init__(self, name = None):
        """
        构造函数
        name: 绑定名字，用于标识本socket，可以多次绑定
        """
        self.__accept_queue = None
        self.__send_queue = None
        self.__recv_queue = None
        self.name = "Unknown"
        if not name == None:
            self.bind(name)

    def connect(self, name):
        """与名为name的模块建立连接"""
        target = VirtualSocket.find(name)
        self.__send_queue = Queue.Queue(maxsize = 0)
        self.__recv_queue = Queue.Queue(maxsize = 0)
        target.__accept_queue.put(self)

    def listen(self, *args, **kwargs):
        """开始监听所有可能的连接"""
        self.__accept_queue = Queue.Queue(maxsize = 0)

    def bind(self, name):
        """
        绑定一个name用于标识线程，该名称在线程表中唯一，可以绑定多个名字
        name: 绑定的名字，用于标识本socket
        """
        if name in VirtualSocket.socket_name_table:
            raise VirtualSocketNameError(name)
        VirtualSocket.socket_name_table[name] = self
        self.name = name

    def accept(self):
        """获得连入的VirtualSocket，需要先执行listen"""
        _s = self.__accept_queue.get()
        s = VirtualSocket()
        s.name = _s.name
        s.__send_queue, s.__recv_queue = _s.__recv_queue, _s.__send_queue
        return s, s.name

    def send(self, text):
        """建立连接后，向VirtualSocket写入值"""
        self.__send_queue.put(text)
        
    def recv(self, *args, **kwargs):
        """建立连接后，从VirtualSocket读出值"""
        text = self.__recv_queue.get()
        return text

    def close(self):
        """关闭连接"""
        #self.__send_queue = None
        self.__recv_queue = None
        self.__accept_queue = None

    @staticmethod
    def find(name):
        """查找名为name的VirtualSocket"""
        if not name in VirtualSocket.socket_name_table:
            raise CannotFindVirtualSocketError(name)
        return VirtualSocket.socket_name_table[name]

if __name__ == "__main__":
    from ThreadManagement import *
    from time import *
    t = ThreadManagement("t")
    s1 = VirtualSocket()
    s1.bind("s1")
    s2 = VirtualSocket()
    s2.bind("s2")
    s3 = VirtualSocket()
    s3.bind("s3")
    s1.listen()
    @new_thread
    def test1(cls):
        try:
            while True:
                a,name = s1.accept()
                text = a.recv()
                print "test1 receive "+text+" from "+name
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
        s3.connect("s1")
        s3.send("msg2")
        text3 = s3.recv()
        print "test3 receive "+text3
        s3.close()


    test2(t)
    test3(t)
    test1(t)
    sleep(3)
    s1.close()
    sleep(5)