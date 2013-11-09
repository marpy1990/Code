# -*- coding: UTF-8 -*-
"""用于管理一定资源并实现某些功能的一种模型

模块是占有资源的最小单位，对模块资源的访问只能采用申请应答机制。
在这种架构体系下，一种资源只能被一个模块唯一占有，全局资源是不被允许的。

Basic usage::

    >>> block = Block(name)
"""
__docformat__ = "restructuredtext en"

__all__ = [
    "Block"
]

__author__ = "marpy"
__version__ = "1.0.0"
__date__ = "$Date: 2013-10-13$"

# exceptions
class BlockNameError(Exception):
    """模块名字已经被注册过"""
    def __init__(self, name):
        Exception.__init__(self, "模块名 "+name+" 已经被注册过")

class CannotFindBlockError(Exception):
    """模块表中找不到对应模块"""
    def __init__(self, name):
        Exception.__init__(self, "模块表中找不到名为 "+name+" 的模块")

# classes
class Block(object):
    """占有资源的最小单位"""

    block_name_table = { }    #模块名称表，模块名称在同进程中是唯一的

    def __init__(self, name):
        """
        构造函数
        name: 模块名
        """
        if name in Block.block_name_table:
            raise BlockNameError(name)
        Block.block_name_table[name] = self
        self.name = name      #实例化后的名字

    @staticmethod
    def find(name):
        """查找名为name的模块"""
        if not name in Block.block_name_table:
            raise CannotFindBlockError(name)
        return Block.block_name_table[name]

if __name__ == "__main__":
    b1 = Block("test1")
    b2 = Block("test2")
    #b3 = Block("test1")
    print Block.block_name_table
    #help(Block)
    print Block.find("test1")