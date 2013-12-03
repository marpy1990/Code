from Plugin import Plugin
from parsers import PortalConfigParser
from threads import STA as STAThread

class BasePortal(Plugin):
    def __init__(self, envir):
        super(BasePortal, self).__init__(envir)
        handlers = {
            "start": self.setup,
            "post": self.post,
            "register": self.register,
            "exit": self.exit,
        }
        for event, handler in handlers.iteritems():
            self.listen(event, handler)

    def setup(self):
        pass

    def post(self, samples):
        pass

    def register(self, categorys):
        pass

    def exit(self):
        pass
        

class PushModePortal(BasePortal):
    def __init__(self, envir, config_file):
        super(PushModePortal, self).__init__(envir)
        config = PortalConfigParser(config_file)
        adapter = config.adapter
        self._conn = adapter(config.connector())
        self._conn.listener = self.on_message
        self._conn.error = self.error

    def setup(self):
        self._conn.setup()
        self.trigger("call register", blocking = True, timeout = 3.0)
        self.trigger("start post", timeout = 3.0)

    @STAThread
    def on_message(self, option, param):
        if option == "set_periods":
            for category, period in param.iteritems():
                self.trigger("set period", args = (category, period), blocking = True, timeout = 3.0)
        elif option == "cancel_categorys":
            for category in param:
                self.trigger("cancel category", args = (category, ), blocking = True, timeout = 3.0)

    def post(self, samples):
        self._conn.post(samples)

    def register(self, categorys):
        self._conn.register(categorys)

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
