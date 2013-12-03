import threading

INFI = -1

class Timer(object):
    def __init__(self, duetime = INFI, period = INFI, function = None, args=(), kwargs={}):
        super(Timer, self).__init__()
        self._duetime = duetime
        self._period = period
        self._function = function
        self._args = args
        self._kwargs = kwargs
        self.__sem = threading.Semaphore(0)
        self._rouser = None
        self._start()

    @property
    def period(self):
        return self._period

    def change(self, duetime, period):
        self._duetime = duetime
        self._period = period
        if not self._rouser is None:
            self._rouser.cancel()
        self._wakeup(duetime)
    
    def stop(self):
        self.change(INFI, INFI)
    
    def _start(self):
            thread = threading.Thread(target = self._run)
            thread.setDaemon(True)
            thread.start()

    def _run(self):
        self._wakeup(self._duetime)
        while True:
            self.__sem.acquire()
            thread = threading.Thread(target = self._function, args = self._args, kwargs = self._kwargs)
            thread.setDaemon(True)
            thread.start()
            self._wakeup(self._period)

    def _wakeup(self, timeout):
        if timeout is INFI:
            self._rouser = None
        else:
            self._rouser = threading.Timer(timeout, self._notify)
            self._rouser.setDaemon(True)
            self._rouser.start()

    def _notify(self):
        self.__sem.release()


if __name__ == '__main__':
    import time

    def ppp():
        print time.time()
    
    ppp() 
    timer = Timer(0,1,ppp)
    time.sleep(3)
    timer.change(2,INFI)
    time.sleep(5)
    