# -*- coding: UTF-8 -*-
"""拥有线程管理功能的模块（Block）模型

继承自Block模型。
在本架构中，线程是模块功能执行的最小单位，所有线程的建立和释放必须在类的线程表上注册和注销。
提供线程表管理所有线程。
提供装饰器auto_register自动注册注销线程

version 1.0.1: 修改了时间的表示方式，现在为 yyyy/mm/dd hh:mm:ss

version 1.0.2: 采用datetime库获取当前时间，现在时间戳精确到微妙，为YYYY-MM-DD HH-MM-SS.SSSSSS

Basic usage::

    >>> threadpool = ThreadManagement(name)
    >>> threadpool.thread_table
"""
__docformat__ = "restructuredtext en"

__all__ = [
    "ThreadManagement",
    "auto_register",
    "new_thread"
]

__author__ = "marpy"
__version__ = "1.0.2"
__date__ = "$Date: 2013-10-14$"

# standard library modules
import threading
import functools
import datetime

# user library modules
import Block

# exceptions
class ThreadRegisterError(Exception):
    """线程注册错误"""
    def __init__(self, thread):
        Exception.__init__(self, "线程"+repr(thread)+"已经被注册过")

class CannotFindInThreadTableError(Exception):
    """线程不在对应线程表中"""
    def __init__(self, cls, thread):
        Exception.__init__(self, "模块"+cls.name+" 的线程表 "+repr(cls.thread_table)+" 中不存在"+repr(thread)+"的线程")

# classes
class ThreadManagement(Block.Block):
    """提供线程管理的Block模块"""
    
    def __init__(self, name):
        """
        构造函数
        name: 模块名
        """
        super(ThreadManagement, self).__init__(name)
        self.thread_table = { }       #内置线程表

    def register_thread(self):
        """注册当前线程至线程表"""
        thread = threading.currentThread()
        if thread in self.thread_table:
            raise ThreadRegisterError(thread)
        date = datetime.datetime.now().isoformat(' ')
        self.thread_table[thread]={"create_date": date}

    def cancel_thread(self):
        """从线程表中注销当前线程"""
        thread = threading.currentThread()
        if not thread in self.thread_table:
            raise CannotFindInThreadTableError(self, thread)
        del self.thread_table[thread]

    def current_thread_info(self):
        """从线程表中读取当前线程信息"""
        thread = threading.currentThread()
        if not thread in self.thread_table:
            raise CannotFindInThreadTableError(self, thread)
        return self.thread_table[thread]

# functions
def auto_register(func):
    """装饰器，对被装饰的线程自动注册并在其结束时注销"""
    @functools.wraps(func)
    def _auto_register(*args, **kwargs):
        thread = threading.currentThread()
        thread.setName(func.__name__)
        cls = args[0]
        cls.register_thread()
        ret=func(*args,**kwargs)
        cls.cancel_thread()
        return ret
    return _auto_register

def new_thread(func):
    """装饰器，调用被装饰的函数时开启新线程并自动注册，并置该线程随主线程结束而结束"""
    @functools.wraps(func)
    def _new_thread(*args, **kwargs):
        run = auto_register(func)
        thread = threading.Thread(target = run, args = args, kwargs = kwargs)
        thread.setDaemon(True)
        thread.start()
    return _new_thread

if __name__ == "__main__":
        import sys
        import time
        m=ThreadManagement("1123")
        print m.thread_table
        @new_thread
        def test(cls):
            print cls.thread_table
            time.sleep(2)
        test(m)
        time.sleep(1)
        print m.thread_table
        time.sleep(1)
        print m.thread_table
        time.sleep(1)
        print m.thread_table
        #m.current_thread_info()