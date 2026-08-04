import Models
from Models.models import  RaceRequest, DriverRequest
import random

def overtake(d1: DriverRequest, d2: DriverRequest):
    d1_coeff = d1.racing_coeff / 5
    d2_coeff = d2.racing_coeff / 5
    res = random.choices(['No', 'Yes'], weights=[d1_coeff, d2_coeff])[0]
    print(d1.name + ' ' + res)
    return res;

def pit(d: DriverRequest, last_index: int):
    d.pits = d.pits + 1

def resolve_crash(driver):
    res = random.choices(['Yes', 'No'], weights=[driver.crash_coeff, (1-driver.crash_coeff)])[0]
    return res;

async def raceStart(race: RaceRequest, drivers: list[DriverRequest], crashed: list[DriverRequest]):
    for lap in range(race.laps):
        print(race.name + ' ' + str(lap))
        for driver in drivers:
            crash_res = resolve_crash(driver)
            if crash_res == "Yes":
                print(driver.name + ' has crashed.')
                crashed.append(driver)
                drivers.remove(driver)
                continue
            driver.crash_coeff = driver.crash_coeff + 0.00008
            driver.pit_coeff = driver.pit_coeff + 0.04
            driverIdx = drivers.index(driver)
            if driverIdx != len(drivers) - 1:
                res = overtake(driver, drivers[drivers.index(driver) + 1])
                if res == 'Yes':
                    drivers[driverIdx + 1].crash_coeff = driver.crash_coeff  + 0.00008
                    drivers[driverIdx], drivers[driverIdx + 1] = drivers[driverIdx + 1], drivers[driverIdx]

    return drivers



