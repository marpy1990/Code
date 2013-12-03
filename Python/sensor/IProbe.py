from xml.etree import ElementTree
import subprocess
import functools
import threading

from IConnection import IConnection as Connection 

INFI = -1

STOPPED = 1
RUNNING = 2
ERROR = 3

def thread(func):
    """The function decorated is a thread"""
    @functools.wraps(func)
    def _thread(*args, **kwargs):
        t = threading.Thread(target = func, args = args, kwargs = kwargs)
        t.setDaemon(True)
        t.start()
    return _thread

def security(func):
    """The function decorated is secure"""
    @functools.wraps(func)
    def _security(*args, **kwargs):
        try:
            ret = func(*args, **kwargs)
        except:
            ret = None
            args[0]._state = ERROR
        finally:
            return ret
    return _security

class IProbe(object):

    def __init__(self, path):
        super(IProbe, self).__init__()
        self.config = None
        self.connection = None
        self._name = None
        self._state = STOPPED
        self._state_block = threading.Semaphore(1)
        self._probeproc = None
        self._categorys = []
        self._category2period = {}
        self._parse(path)

    @property
    def name(self):
        return self._name
    
    @property
    def categorys(self):
        return self._categorys

    @property
    def periods(self):
        return self._category2period

    @property
    def state(self):
        return self._state

    #@thread
    @security
    def runprobe(self):
        cmd = self.config.find("start_command").text

        self._state = RUNNING
        self._probeproc = subprocess.Popen(cmd, shell = True)

    @security
    def connect(self):
        lst_config = self.config.findall("connect/to")
        for config in lst_config:
            if config.attrib["name"] == "self":
                self.connection = Connection(config.find("connection"))
                return
        raise Exception()
    
    @security
    def getdata(self, category):
        self._state_block.acquire()
        if self._state is STOPPED:
            self.runprobe()
            self.connect()
        self._state_block.release()
        msg = self._getreq2str(category)
        self.connection.send(msg)
        datatext = self.connection.recv()
        xml = self._datatext2xml(datatext)
        return xml

    @security
    def stopprobe(self):
        if self._state is RUNNING:
            self.connection.close()
            self._probeproc.kill(self._probepid)
            self._state = STOPPED

    @security
    def _parse(self, path):
        self.config = ElementTree.parse(path).getroot()
        self._name = self.config.find("name").text
        self._get_categorys()

    def _shutdown2str(self):
        root = ElementTree.Element(tag = "message")
        request = ElementTree.Element(tag = "shutdown")
        root.append(request)
        return ElementTree.tostring(root)

    def _datatext2xml(self, datatext):
        root = ElementTree.fromstring(datatext)
        dataxml = root.find("performance_data")
        return dataxml

    def _getreq2str(self, category):
        root = ElementTree.Element(tag = "message")
        request = ElementTree.Element(tag = "get")
        category_elem = ElementTree.Element(tag = "category")
        category_elem.text = category
        request.append(category_elem)
        root.append(request)
        return ElementTree.tostring(root)

    def _get_categorys(self):
        lst_category = self.config.findall("categorys/category")
        for category_elem in lst_category:
            name = category_elem.find("name").text
            pertext = category_elem.find("period").text
            if pertext is None or pertext == "INFI":
                period = INFI
            else:
                period = float(pertext)
                if period<=0:
                    period = INFI
            self._categorys.append(name)
            self._category2period[name] = period

if __name__ == '__main__':
    import time
    p = IProbe("testprobe1.xml")
    for i in range(0,5):
        xml = p.getdata("category1")
        print ElementTree.tostring(xml)
    time.sleep(3)
    p.stopprobe()
    for i in range(0,3):
        xml = p.getdata("category2")
        print ElementTree.tostring(xml)
    p.stopprobe()
