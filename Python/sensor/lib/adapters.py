try:
    import xml.etree.cElementTree as ElementTree
except ImportError:
    import xml.etree.ElementTree as ElementTree
import json

class PortalConnectAdapter(object):
    def __init__(self, connector):
        super(PortalConnectAdapter, self).__init__()
        self._conn = connector
        self._listener = None

    @property
    def listener(self):
        return self._listener
    @listener.setter
    def listener(self, func):
        self._listener = func
        self._conn.listener = self._on_message

    @property
    def error(self):
        return self._conn.error
    @error.setter
    def error(self, func):
        self._conn.error = func
    
    def _xsample(self, samples):
        for sample in samples:
            node_sample = ElementTree.Element("sample")
            for key, value in sample.iteritems():
                node_pair = ElementTree.SubElement(node_sample, "pair")
                node_key = ElementTree.SubElement(node_pair, "key")
                node_key.text = key
                node_value = ElementTree.SubElement(node_pair, "value")
                node_value.text = repr(value)
            yield node_sample

    def _xcategory(self, categorys):
        for category in categorys:
            node_category = ElementTree.Element("category")
            node_category.text = category
            yield node_category

    def _on_message(self, message):
        root = ElementTree.fromstring(message)
        for node_option in list(root):
            option = node_option.tag
            if option == "set_periods":
                self._set_periods(node_option)
            elif option == "cancel_categorys":
                self._cancel_categorys(node_option)

    def _set_periods(self, node):
        periods = {}
        for node_pair in node.findall("pair"):
            category = node_pair.find("category").text
            period = float(node_pair.find("period").text)
            periods[category] = period
        apply(self._listener, ("set_periods", periods))

    def _cancel_categorys(self, node):
        categorys = []
        for node_category in node.findall("category"):
            category = node_category.text
            categorys.append(category)
        apply(self._listener, ("cancel_categorys", categorys))
            
    def setup(self, samples = None):
        root = ElementTree.Element("message")
        node = ElementTree.SubElement(root, "setup")
        if samples is not None:
            for elem in self._xsample(samples):
                node.append(elem)
        self._conn.send(ElementTree.tostring(root))

    def post(self, samples):
        root = ElementTree.Element("message")
        node = ElementTree.SubElement(root, "post")
        for elem in self._xsample(samples):
            node.append(elem)
        self._conn.send(ElementTree.tostring(root))

    def register(self, categorys):
        root = ElementTree.Element("message")
        node = ElementTree.SubElement(root, "register")
        for elem in self._xcategory(categorys):
            node.append(elem)
        self._conn.send(ElementTree.tostring(root))

    def close(self):
        root = ElementTree.Element("message")
        node = ElementTree.SubElement(root, "shutdown")
        self._conn.send(ElementTree.tostring(root))
        self._conn.close()


class ProbeConnectAdapter(object):
    def __init__(self, connector):
        super(ProbeConnectAdapter, self).__init__()
        self._conn = connector
        self._listener = None

    @property
    def listener(self):
        return self._listener
    @listener.setter
    def listener(self, func):
        self._listener = func
        self._conn.listener = self._on_message

    @property
    def error(self):
        return self._conn.error
    @error.setter
    def error(self, func):
        self._conn.error = func
    
    def _on_message(self, message):
        obj = json.loads(message)
        if type(obj) is dict:
            samples = [obj, ]
        elif type(obj) is list:
            samples = obj
        apply(self._listener, (samples, ))
    
    def set_period(self, category, period):
        cmd = {category: period}
        self._conn.send(json.dumps(cmd))

    def cancel_category(self, category):
        cmd = {category: 0}
        self._conn.send(json.dumps(cmd))

    def close(self):
        self._conn.close()


if __name__ == '__main__':
    adp = PortalConnectAdapter(None)
    root = ElementTree.Element("message")
    node = ElementTree.SubElement(root, "setup")
    for elem in adp._xsample([{"machine":"sdf-pc", "category":"cpu", "_Total":"12.3"}, {"machine":"sdf-pc", "category":"cpu", "0":"24.5"}]):
        node.append(elem)
    print ElementTree.tostring(root)

    root = ElementTree.Element("message")
    node = ElementTree.SubElement(root, "register")
    for elem in adp._xcategory(["aaa","bbb","ccc","ddd"]):
        node.append(elem)
    print ElementTree.tostring(root)

