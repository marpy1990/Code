from xml.etree import ElementTree
import socket
import time
import threading

BUFSIZE = 65535
TIMEWAIT = 3.0

class IConnection(object):
    
    def __init__(self, config):
        super(IConnection, self).__init__()
        self._send_block = threading.Semaphore(1)
        self.type_table = {
            "socketclient": _SocketClient,
            "socketserver": None,   #may be written latter
            "activemq": None        #may be written latter
        }
        contype = config.attrib["type"]
        self.connection = self.type_table[contype](config)


    def send(self, message):
        #self._send_block.acquire()
        self.connection.send(message)
        time.sleep(0.01)
        #self._send_block.release()

    def recv(self):
        return self.connection.recv()

    def close(self):
        return self.connection.close()

        
class _SocketClient(object):

    def __init__(self, config):
        super(_SocketClient, self).__init__()
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        ip = config.find("ip").text
        port = int(config.find("port").text)
        while True:
            try:
                self.sock.connect((ip, port))
                break
            except:
                time.sleep(TIMEWAIT)

    def send(self, message):
        self.sock.send(message)

    def recv(self):
        return self.sock.recv(BUFSIZE)

    def close(self):
        self.sock.close()

if __name__ == '__main__':
    """
    config = ElementTree.Element(tag = "root")
    ip = ElementTree.Element(tag = "ip")
    ip.text = "localhost"
    post = ElementTree.Element(tag = "port")
    post.text = "9000"
    config.append(ip)
    config.append(post)
    sock = _SocketClient(config)
    #print "1"
    #sock.send("a")
    #sock.close()
    """
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect(("localhost",9000))
    sock.send("aaaaaaa")
    sock.send("bbbbbbb")
    sock.send("c")
    help(time.sleep)
