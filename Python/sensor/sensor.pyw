"""
This module is just an entry to run sensor.

"""
import lib.Envir
import Sensor

envir = lib.Envir.Envir()
sensor = Sensor.Sensor(envir)
sensor.run()