import threading
import functools

def new(func):
    """The function decorated is a new thread."""
    @functools.wraps(func)
    def _new(*args, **kwargs):
        thread = threading.Thread(target = func, args = args, kwargs = kwargs)
        thread.setDaemon(True)
        thread.start()
    return _new


def STA(func):
    """The function decorated is using STA(Single Thread Apartment) module.
    When the function has been occupied, other calling threads will block until it has finished.

    """
    STA.__cond = threading.Condition()
    @functools.wraps(func)
    def _STA(*args, **kwargs):
        with STA.__cond:
            apply(func, args, kwargs)
    return _STA


def STAPass(func):
    """The function decorated is using STA(Single Thread Apartment) module.
    When the function has been occupied,  other calling threads will pass to apply the function.

    """
    STAPass.__occupied = False
    STAPass.__cond = threading.Condition()
    @functools.wraps(func)
    def _STAPass(*args, **kwargs):
        with STAPass.__cond:
            if STAPass.__occupied:
                return
            else:
                STAPass.__occupied = True
        try:
            apply(func, args, kwargs)
        finally:
            STAPass.__occupied = False
    return _STAPass



