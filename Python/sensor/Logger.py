import sys  
import time
import StringIO 
import logging
from logging.handlers import TimedRotatingFileHandler
from lib.Plugin import Plugin
from lib.threads import new as newThread

class Logger(Plugin):
    def __init__(self, envir):
        super(Logger, self).__init__(envir)
        logger = logging.getLogger()
        hdlr = TimedRotatingFileHandler("log/sensor.log", "midnight", 1, 10)
        formatter = logging.Formatter('\n%(levelname)-7s | %(asctime)-23s | === %(message)s ===')
        hdlr.setFormatter(formatter)
        logger.addHandler(hdlr)
        logger.setLevel(logging.NOTSET)
        logger.info("begin")

        formatter = logging.Formatter('%(levelname)-7s | %(asctime)-23s | %(message)s')
        hdlr.setFormatter(formatter)
        self._logger = logger

        handlers = {
            "start": self.info("start"),
            "post": self.info("post"),
            "register": self.info("register"),
            "start post": self.info("start post"),
            "call register": self.info("call register"),
            "set period": self.info("set period"),
            "cancel category": self.info("cancel category"),
            "display": self.info("display"),
            "init info": self.info("init"),
            "exit": self.exit,
            "warning": self.warning("warning"),
            "error": self.error("error"),
        }
        for event, handler in handlers.iteritems():
            self.listen(event, handler)
        self.state = "running"
        self.trace_IO()

    @newThread
    def trace_IO(self):
        buf = StringIO.StringIO()
        tmp = sys.stderr
        sys.stderr = buf
        pos = 0
        while self.state == "running":
            try:
                buf.seek(pos)
                msg = buf.read()
            except:
                pass
            else:
                if not msg == "":
                    self.error("escaped error")(msg, )
                    pos = pos + len(msg)
            finally:
                time.sleep(1)
        msg = buf.read()
        if not msg == "":
            self.error("escaped error")(msg, )
        sys.stderr = tmp

    def exit(self):
        self.info("exit")()
        self.state = "stop"

    def info(self, option):
        def _info(*args, **kwargs):
            self._logger.info("%-14s , %s , %s",option, args, kwargs)
        return _info

    def warning(self, option):
        def _warning(*args, **kwargs):
            self._logger.warning("%-14s , %s , %s",option, args, kwargs)
        return _warning

    def error(self, option):
        def _error(*args, **kwargs):
            self._logger.error("%-14s , %s , %s",option, args, kwargs)
        return _error