from Plugin import Plugin
from parsers import ProbeConfigParser
from threads import STA as STAThread

class BaseProbe(Plugin):
    def __init__(self, envir):
        super(BaseProbe, self).__init__(envir)
        handlers = {
            "start post": self.start,
            "call register": self.register,
            "set period": self.set_period,
            "cancel category": self.cancel_category,
            "exit": self.exit,
        }
        for event, handler in handlers.iteritems():
            self.listen(event, handler)

    def start(self):
        pass

    def register(self):
        pass

    def set_period(self, category, period):
        pass

    def cancel_category(self, category):
        pass

    def exit(self):
        pass


class PushModeProbe(BaseProbe):
    def __init__(self, envir, config_file):
        super(PushModeProbe, self).__init__(envir)
        config = ProbeConfigParser(config_file)
        adapter = config.adapter
        self._categorys = config.categorys
        self._conn = adapter(config.connector())
        self._conn.listener = self.on_message
        self._conn.error = self.error
        self.state = "ready"

    def start(self):
        self.state = "running"
        for category, period in self._categorys.iteritems():
            if period > 0:
                self._conn.set_period(category, period)

    @STAThread
    def on_message(self, samples):
        if self.state == "running":
            self.trigger("post", args = (samples, ), timeout = 3.0)

    def register(self):
        self.trigger("register", args = (self._categorys.keys(), ), timeout = 3.0)

    def set_period(self, category, period):
        self._conn.set_period(category, period)

    def cancel_category(self, category):
        self._conn.cancel_category(category)

    def exit(self):
        self.cancel()
        self._conn.close()

    def error(self, e):
        self.trigger("warning", args = (e, ), timeout = 3.0, err_handler = self.serious_error)

    def serious_error(self, e):
        """It's a serious error because the error handler failed.
        
        May be overridden.

        """
        pass
