from xml.etree import ElementTree
import functools
import threading
import time

from IConnection import IConnection as Connection
from IProbe import IProbe as Probe
from Timer import Timer

def thread(func):
    """The function decorated is a thread"""
    @functools.wraps(func)
    def _thread(*args, **kwargs):
        t = threading.Thread(target = func, args = args, kwargs = kwargs)
        t.setDaemon(True)
        t.start()
    return _thread

class Sensor(object):

    def __init__(self):
        super(Sensor, self).__init__()
        self.category2probe = {}
        self.category2timer = {}
        self.probes = []
        self.timers = []
        self.config = ElementTree.parse("config.xml").getroot()
        self.connection = None
        self._block = threading.Semaphore(0)
        self._error_block = threading.Semaphore(1)
        #self._post_block = threading.Semaphore(1)
        self.option_table = {
            "set_periods": self.set_periods,
            "cancel_categorys": self.cancel_categorys
        }

    def start(self):
        self.connect()
        self.setup_notify()
        self.add_probes()
        self.handle()
        self.start_post()

        self._block.acquire()

    def release(self):
        self._block.release()

    def setup_notify(self):
        root = ElementTree.Element(tag = "message")
        setup_elem = ElementTree.Element(tag = "setup")
        root.append(setup_elem)
        message = ElementTree.tostring(root)
        self.connection.send(message)

    def categorys_notify(self, categorys):
        reg_node = ElementTree.Element(tag = "register")
        for category in categorys:
            category_elem = ElementTree.Element(tag = "category")
            category_elem.text = category
            reg_node.append(category_elem)
        root = ElementTree.Element(tag = "message")
        root.append(reg_node)
        message = ElementTree.tostring(root)
        self.connection.send(message)

    def start_post(self):
        for timer in self.timers:
            timer.start()

    def add_probes(self):
        lst_probepath = self.config.findall("probes/path")
        for path_elem in lst_probepath:
            self._add_probe(path_elem.text)

    def _add_probe(self, path):
        probe = Probe(path)
        self.probes.append(probe)
        reg_node = ElementTree.Element(tag = "register")
        for category in probe.categorys:
            self.category2probe[category] = probe
            timer = Timer(probe.periods[category], self._post, (category,))
            self.category2timer[category] = timer
            self.timers.append(timer)
        self.categorys_notify(probe.categorys)

    def _post(self, category):
        try:
            #self._post_block.acquire()
            #print "--",category,"--"
            probe = self.category2probe[category]
            #print "--",probe.name,"--"
            data_node = probe.getdata(category)
            #print "--",data_node,"--"
            data_node.tag = "post"
            root = ElementTree.Element(tag = "message")
            root.append(data_node)
            message = ElementTree.tostring(root)
            #print "--",message,"--"
        except:
            return
        finally:
            pass
            #self._post_block.release()

        try:
            self.connection.send(message)
        except:
            self.connect_error()

    def connect(self):
        lst_config = self.config.findall("connect/to")
        for config in lst_config:
            if config.attrib["name"] == "server":
                self.connection = Connection(config.find("connection"))
                break

    def connect_error(self):
        self._error_block.acquire()
        self.connect()
        self._error_block = threading.Semaphore(1)

    @thread
    def handle(self):
        while True:
            try:
                message = self.connection.recv()
                if message is None:
                    break
                handle_message(message)
            except:
                break
        self.connect_error()

    @thread
    def handle_message(self, message):
        try:    
            root = ElementTree.fromstring(message)
            lst_option = root.getchildren()
            for option in lst_option:
                self.option_table[option.tag](option)
        except:
            self.message_error()

    def message_error(self):
        """
        Cannot parse the message.
        May be overridden.
        """
        pass

    def set_periods(self, option):
        lst_pair = option.findall("pair")
        for pair in lst_pair:
            category = pair.find("category").text
            period = float(pair.find("period").text)
            self._set_period(category, period)
    
    def cancel_categorys(self, option):
        lst_category = option.findall("category")
        for category_elem in lst_category:
            category = category_elem.text
            self._cancel_category(category)

    def _set_period(self, category, period):
        timer = self.category2timer[category]
        timer.set_period(period)

    def _cancel_category(self, category):
        timer = self.category2timer[category]
        timer.set_period(Timer.INFI)
        probe = self.category2probe[category]
        stopprobe_flag = True
        for category in probe.categorys:
            period = self.category2timer[category]
            if not period == Timer.INFI:
                stopprobe_flag = False
                break
        if stopprobe_flag:
            probe.stopprobe()

if __name__ == '__main__':
    sensor = Sensor()
    sensor.start()





