from lib.ProbePlugin import PushModeProbe
from lib.connectors import Pipe
from lib.parsers import PipeConfigParser

class PipeToSensor(Pipe):
    def __init__(self):
        config = PipeConfigParser("config/sysprobe.xml")
        proc_cmd = config.process
        super(PipeToSensor, self).__init__(proc_cmd)

class SystemProbe(PushModeProbe):
    def __init__(self, envir):
        super(SystemProbe, self).__init__(envir, "config/sysprobe.xml")