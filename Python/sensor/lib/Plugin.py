import Envir

# exceptions
class RepeatListenError(Exception):
    def __init__(self, event):
        Exception.__init__(self, "repeat listen event: "+repr(event))
        self.event = event

class CancelError(Exception):
    def __init__(self, event):
        Exception.__init__(self, "event: "+repr(event)+" has canceled or not registered")
        self.event = event

# classes
class Plugin(object):

    def __init__(self, envir):
        super(Plugin, self).__init__()
        self.__env = envir
        self.__reg = {}

    def listen(self, event, handler):
        if self.__reg.has_key(event):
            raise RepeatListenError(event)
        self.__reg[event] = handler
        self.__env.register(event, handler)
        

    def cancel(self, event=None):
        if event is None:   #cancel all handlers in this plugin
            for evt,handler in self.__reg.items():
                self.__env.cancel(evt, handler)
            self.__reg.clear()
        else:
            if not self.__reg.has_key(event):
                raise CancelError(event)
            handler = self.__reg[event]
            self.__env.cancel(event, handler)
            del self.__reg[event]

    def trigger(self, event, args=(), kwargs={}, blocking=False, timeout=None, err_handler=None):
        if err_handler is None:
            err_handler = self.error
        try:
            self.__env.trigger(event, args, kwargs, blocking, timeout, err_handler)
        except Exception, e:
            apply(err_handler, (e, ))

    def error(self, e):
        """Handle the exception raised by trigger.

        May be overridden.

        """
        raise e


if __name__ == '__main__':
    import time
    class test(Plugin):
        def __init__(self, env):
            super(test, self).__init__(env)
            self.listen("t1", self.x)

        def x(self,a,b):
            time.sleep(3)
            print a,b

        def run(self):
            self.trigger("t1", args=("1",), kwargs={'b':3}, blocking=True, timeout=4)


    class test2(Plugin):
        def __init__(self, env):
            super(test2, self).__init__(env)
            self.listen("t1", self.x)

        def x(self,a,b):
            print "****"
            self.cancel()


    e=Envir.Envir()
    y=test2(e)
    x=test(e)
    x.run()
    time.sleep(1)



