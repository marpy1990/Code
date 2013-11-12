import threading

INFI = -1

STOPPED = 1
RUNNING = 2
RELEASED = 3

class Timer(object):
    def __init__(self, period, function, args=(), kwargs={}):
        super(Timer, self).__init__()
        if period <= 0:
            self._period = INFI
        else:
            self._period = period
        self.function = function
        self.args = args
        self.kwargs = kwargs
        self._state = STOPPED
        self.wakeup = None

    @property
    def period(self):
        return self._period

    @property
    def state(self):
        return self._state

    def start(self):
        if self._state is STOPPED:
            self._state = RUNNING
            thread = threading.Thread(target = self._start)
            thread.setDaemon(True)
            thread.start()

    def set_period(self, period):
        self._period = period
        if self._state is RUNNING:
            if not self.wakeup is None:
                self.wakeup.cancel()
            if self._period > 0:
                self.wakeup = threading.Timer(self._period, self._wakeup)
                self.wakeup.setDaemon(True)
                self.wakeup.start()
    
    def release(self, period):
        self.set_period(INFI)
        self._state = RELEASED

    def _start(self):
        while True:
            self.block = threading.Semaphore(0)
            self.set_period(self._period)
            self.block.acquire()
            thread = threading.Thread(target = self.function, args = self.args, kwargs = self.kwargs)
            thread.setDaemon(True)
            thread.start()

    def _wakeup(self):
        self.block.release()

if __name__ == '__main__':
    import time
        
    def ppp(a):
        print a
        
    timer = Timer(1.0,ppp,("123",))
    timer.start()
    time.sleep(10)


    