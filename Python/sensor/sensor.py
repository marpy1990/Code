import time
import threading

from lib.Envir import Envir
from lib.Plugin import Plugin
from PluginFactory import PluginFactory

class Sensor(Plugin):
    def __init__(self, envir):
        super(Sensor, self).__init__(envir)
        self._envir = envir
        self._block = threading.Semaphore(0)
        plugins = PluginFactory(envir, "config/plugins.xml")

        for plugin in plugins.iter():
            if isinstance(plugin, Exception):
                self.trigger("error", args = (plugin, ), timeout = 3.0)
            else:
                self.trigger("init info", args = (repr(plugin)+" initialized", ), timeout = 3.0)

        self.listen("exit", self.exit)

    def run(self):
        self.trigger("start", timeout = 3.0)
        self.trigger("display", blocking = True, err_handler = self.close)
        self._block.acquire()

    def close(self, e):
        self.exit()

    def exit(self):
        time.sleep(0.5)
        self._block.release()

    def error(self, e):
        self.trigger("warning", args = (e, ), timeout = 3.0)

if __name__ == '__main__':
    envir = Envir()
    sensor = Sensor(envir)
    sensor.run()

        





