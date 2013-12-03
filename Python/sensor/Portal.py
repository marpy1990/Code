from lib.PortalPlugin import PushModePortal
from lib.connectors import SocketClient
from lib.parsers import SocketConfigParser

class SocketToServer(SocketClient):
    def __init__(self):
        config = SocketConfigParser("config/portal.xml")
        addr = config.addr
        cache_size = config.cache
        super(SocketToServer, self).__init__(addr, cache_size)

class Portal(PushModePortal):
    def __init__(self, envir):
        super(Portal, self).__init__(envir, "config/portal.xml")


if __name__ == '__main__':
    from lib.Envir import Envir
    import time
    envir = Envir()
    portal = Portal(envir)
    time.sleep(20)