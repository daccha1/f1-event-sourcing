from datetime import datetime
import json
import random

from Contracts.mq_contracts.race_messages import EventType, EventWrapper
from Data.dbSchemas import Race
from Models.models import RacingDriver
from Services.RabbitMQ import messageQueueService as rmq
from Events.DriverEvents import driver_events
from Events.DriverEvents.driver_events import StartedRace, FinishedRace, DriverOvertook, Pitted, Disqualified
import uuid
from Services.RabbitMQ.messageQueueService import publishMsg

CRASH_RISK_PER_LAP = 0.00008
CRASH_RISK_PER_OVERTAKE = 0.00008
MAX_CRASH_PROBABILITY = 0.95


def resolve_crash(driver: RacingDriver, rng: random.Random) -> bool:
    return rng.random() < min(driver.crash_coeff, MAX_CRASH_PROBABILITY)


def wins_duel(attacker: RacingDriver, defender: RacingDriver, rng: random.Random) -> bool:
    total = attacker.racing_coeff + defender.racing_coeff
    if total <= 0:
        return False
    return rng.random() < attacker.racing_coeff / total


def pit(d: RacingDriver, last_index: int):
    d.pits = d.pits + 1


def raceStart(
    race: Race,
    drivers: list[RacingDriver],
    crashed: list[RacingDriver],
    seed: int | None = None,) -> list[RacingDriver]:
    rng = random.Random(seed) # definise randomness i paralelizam (dve trke pokrenute istovremeno bez rng imaju isti output na kraju)

    # INSERT MSG PUBLISHING

    # napravi startedRace event
    # serijalizuj event
    # publishuj

    raceGuid = uuid.uuid4()

    for d in drivers:
        driverGuid = uuid.uuid4()
        startedRaceEvt = StartedRace(driverId=driverGuid, raceId=raceGuid)
        startedRaceJson = startedRaceEvt.model_dump_json()
        eventWrapper = EventWrapper(EventType="StartedRace", Payload=startedRaceJson, OcurredAt=datetime.now())
        publishMsg(eventWrapper)


    for lap in range(1, race.laps + 1):
        # Iterira se preko kopije jer `drivers` menja duzinu unutar petlje.
        for driver in list(drivers):
            if resolve_crash(driver, rng):
                driver.crashed_on_lap = lap
                crashed.append(driver)
                drivers.remove(driver)
                continue
            driver.crash_coeff += CRASH_RISK_PER_LAP
            driver.pit_coeff += 0.04

        if not drivers:
            break

        # Odzada napred; `moved` ogranicava vozaca na jedan duel i jedan pomak po lapu.
        moved: set[int] = set()
        for i in range(len(drivers) - 1, 0, -1):
            attacker, defender = drivers[i], drivers[i - 1]
            if attacker.driver_id in moved:
                continue
            if wins_duel(attacker, defender, rng):
                attacker.crash_coeff += CRASH_RISK_PER_OVERTAKE
                drivers[i - 1], drivers[i] = attacker, defender
                moved.add(attacker.driver_id)


        #rmq.publishMsg(msg.model_dump_json()) # FIX PUBLSHING MSG

    for position, driver in enumerate(drivers, start=1):
        driver.finished_position = position

    return drivers



