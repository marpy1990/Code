import subprocess

from lib.Envir import Envir
from lib.Plugin import Plugin
from lib.parsers import ShellConfigParser

class Shell(Plugin):
    def __init__(self, envir):
        super(Shell, self).__init__(envir)
        handlers = {
            "display": self.display, 
        }
        for event, handler in handlers.iteritems():
            self.listen(event, handler)
        config = ShellConfigParser("config/shell.xml")
        self._proc_cmd = config.process

    def display(self):
        proc = subprocess.Popen(self._proc_cmd, shell = True)
        proc.wait()
        self.trigger("exit", timeout = 3.0)

    def error(self, e):
        self.trigger("error", args = (e, ), err_handler = self.serious_error)

    def serious_error(self, e):
        pass

if __name__ == '__main__':
    shell = Shell(Envir())
    shell.display()