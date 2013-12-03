try:
    import xml.etree.cElementTree as ElementTree
except ImportError:
    import xml.etree.ElementTree as ElementTree

class XmlFileParser(object):
    def __init__(self, config_file):
        super(XmlFileParser, self).__init__()
        self._root = ElementTree.parse(config_file).getroot()

    def getclass(self, module_name, class_name):
        module = __import__(module_name, fromlist=[""])
        return getattr(module, class_name)


class PluginFactoryParser(XmlFileParser):
    def __init__(self, config_file):
        super(PluginFactoryParser, self).__init__(config_file)
        self._plugins = []
        for node in self._root.findall("plugin"):
            module_name = node.find("module").text
            class_name = node.find("class").text
            self._plugins.append(self.getclass(module_name, class_name))

    @property
    def plugins(self):
        return self._plugins


class PluginConfigParser(XmlFileParser):
    def __init__(self, config_file):
        super(PluginConfigParser, self).__init__(config_file)
        adapter_node = self._root.find("adapter")
        connector_node = self._root.find("connector")

        module_name = adapter_node.find("module").text
        class_name = adapter_node.find("class").text
        self._adapter = self.getclass(module_name, class_name)

        module_name = connector_node.find("module").text
        class_name = connector_node.find("class").text
        self._connector = self.getclass(module_name, class_name)

    @property
    def adapter(self):
        return self._adapter

    @property
    def connector(self):
        return self._connector


class PortalConfigParser(PluginConfigParser):
    pass


class ProbeConfigParser(PluginConfigParser):
    def __init__(self, config_file):
        super(ProbeConfigParser, self).__init__(config_file)
        self._categorys = {}
        node = self._root.find("categorys")
        for node_category in node.findall("category"):
            category = node_category.text
            period = float(node_category.attrib.get("period", "0"))
            self._categorys[category] = period

    @property
    def categorys(self):
        return self._categorys


class SocketConfigParser(XmlFileParser):
    def __init__(self, config_file):
        super(SocketConfigParser, self).__init__(config_file)
        ip = self._root.find("connector/socket/ip").text
        port = int(self._root.find("connector/socket/port").text)
        node_cache = self._root.find("connector/socket/cache")
        self._addr = (ip, port)
        if node_cache.text is None or int(node_cache.text) <= 0:
            self._cache = 0
        else:
            self._cache = int(node_cache.text)
        unit = node_cache.attrib.get("unit", "KB")
        if unit == "KB":
            self._cache = self._cache*1024
        elif unit == "MB":
            self._cache = self._cache*1024*1024

    @property
    def addr(self):
        return self._addr

    @property
    def cache(self):
        return self._cache


class PipeConfigParser(XmlFileParser):
    def __init__(self, config_file):
        super(PipeConfigParser, self).__init__(config_file)
        self._process = self._root.find("connector/pipe/process").text

    @property
    def process(self):
        return self._process

class ShellConfigParser(XmlFileParser):
    def __init__(self, config_file):
        super(ShellConfigParser, self).__init__(config_file)
        self._process = self._root.find("process").text

    @property
    def process(self):
        return self._process
    


    

    