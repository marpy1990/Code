# -*- coding: UTF-8 -*-
"""提供用于Debug的一些函数，仅在debug模式下起作用

    version 1.0.1: 增加了一些日志选项
"""
__docformat__ = "restructuredtext en"

__all__ = [
    "writelog"
]

__author__ = "marpy"
__version__ = "1.0.1"
__date__ = "$Date: 2013-10-22$"

# standard library modules
import logging
import functools

# functions
def initlog():
    logger = logging.getLogger()
    hdlr = logging.FileHandler("debug.log")
    
    formatter = logging.Formatter('\n%(asctime)s ===%(message)s===')
    hdlr.setFormatter(formatter)
    logger.addHandler(hdlr)
    logger.setLevel(logging.NOTSET)
    logger.debug("start")

    formatter = logging.Formatter('%(asctime)s %(threadName)s %(thread)d %(message)s')
    hdlr.setFormatter(formatter)
    logger.addHandler(hdlr)
    logger.setLevel(logging.NOTSET)
    return logger

# globals
debug_log = None
if __debug__:
    debug_log = initlog()

# functions
def writelog(func):
    """装饰器，对被装饰的函数在调用时自动写入日志"""
    @functools.wraps(func)
    def _writelog(*args, **kwargs):
        message = func.__name__+"("
        
        flag = False
        for value in args:
            message = message+repr(value)+", "
            flag = True
        for key, value in kwargs.items():
            message = message+key+" = "+repr(value)+", "
            flag = True
        if flag:
            message = message[:-2]
        message = message+")"

        debug_log.debug(message+" enter")
        ret = func(*args, **kwargs)
        debug_log.debug(message+ " leave")
        return ret
    if __debug__:
        return _writelog
    else:
        return func

if __name__ == "__main__":
    class T:
        @writelog
        def test(self, x, y):
            print x+y
    class V:
        def ttt(self, a):
            return 2
    a = T()
    b = V()
    b.ttt(a.test(1, y=2))


