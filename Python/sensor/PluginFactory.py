import traceback
from lib.parsers import PluginFactoryParser

class PluginCreateError(Exception):
    def __init__(self, plugin, e, log):
        Exception.__init__(self, "error for creating the plugin: "+repr(plugin)+"\nDetail:\n"+log)
        self.plugin = plugin
        self.e = e
        self.log = log

        
class PluginFactory(object):
    def __init__(self, envir, config_file):
        super(PluginFactory, self).__init__()
        self.__envir = envir
        self.__plugins = PluginFactoryParser(config_file).plugins

    def iter(self):
        for plugin in self.__plugins:
            try:
                yield plugin(self.__envir)
            except Exception, e:
                yield PluginCreateError(plugin, e, traceback.format_exc())


if __name__ == '__main__':
    from lib.Envir import Envir
    plugins = PluginFactory(Envir(), "config/plugins.xml")
    for plugin in plugins.iter():
        print plugin.__class__.__name__