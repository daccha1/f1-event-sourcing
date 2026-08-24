from datetime import datetime
import json
import random

from Contracts.mq_contracts.race_messages import EventWrapper
from Data.dbSchemas import Race
from Events.RaceEvents.race_events import CreateRace, FinishRace, StopRace
from Models.models import RacingDriver
from Services.RabbitMQ import messageQueueService as rmq
from Events.DriverEvents import driver_events
from Events.DriverEvents.driver_events import DriverStartedRace, DriverFinishedRace, DriverOvertook, DriverCrashed, DriverPitted, DriverDisqualified
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


    # DriverEvents:  Disqualified, Pitted, Crashed | CRASHED missing in event handling

    raceGuid = uuid.uuid4()

    # publishing: RaceStart
    raceCreatedEvt = CreateRace(
        raceId=raceGuid,
        country=race.country,
        laps = race.laps,
        gp = race.grandPrix
    )
    json_raceCreatedEvt = raceCreatedEvt.model_dump_json()
    wrapper_raceCreatedEvt = EventWrapper(
        EventType="RaceCreated",
        Payload= json_raceCreatedEvt,
        OcurredAt=datetime.now(),
    )
    publishMsg(wrapper_raceCreatedEvt)


    for d in drivers:
        driverGuid = uuid.uuid4()
        d.correlation = driverGuid
        # publish: DriverStartedRace
        startedRaceEvt = DriverStartedRace(driverId=driverGuid, raceId=raceGuid, name=d.name, team=d.team)
        startedRaceJson = startedRaceEvt.model_dump_json()
        eventWrapper = EventWrapper(EventType="DriverStartedRace", Payload=startedRaceJson, OcurredAt=datetime.now())
        publishMsg(eventWrapper)

    # MAIN RACE SIMULATING LOGIC
    for lap in range(1, race.laps + 1):
        # Iterira se preko kopije jer `drivers` menja duzinu unutar petlje.
        for driver in list(drivers):
            if resolve_crash(driver, rng):
                driver.crashed_on_lap = lap
                crashed.append(driver)
                drivers.remove(driver)
                driverCrashedEvt = DriverCrashed(
                    driverId=driver.correlation,
                    occurredAt=datetime.now()
                )
                json_driverCrashedEvt = driverCrashedEvt.model_dump_json()
                wrapper_driverCrashedEvt = EventWrapper(
                    EventType="DriverCrashed",
                    OcurredAt=driverCrashedEvt.occurredAt,
                    Payload=json_driverCrashedEvt
                )
                publishMsg(wrapper_driverCrashedEvt)
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
                # publish: DriverOvertook
                driverOvertookEvt = DriverOvertook(
                    driverFront=attacker.correlation,
                    driverBehind=defender.correlation
                )
                json_driverOvertookEvt = driverOvertookEvt.model_dump_json()
                wrapper_driverOvertookEvt = EventWrapper(
                    EventType="DriverOvertook",
                    OcurredAt=datetime.now(),
                    Payload=json_driverOvertookEvt
                )
                publishMsg(wrapper_driverOvertookEvt)



    # publish: RaceFinished
    raceFinishedEvt = FinishRace(
        raceId=raceGuid,
    )
    json_raceFinishedEvt = raceFinishedEvt.model_dump_json()
    wrapper_raceFinishedEvt = EventWrapper(
        EventType="RaceFinished",
        Payload=json_raceFinishedEvt,
        OcurredAt=datetime.now(),
    )
    publishMsg(wrapper_raceFinishedEvt)



    #rmq.publishMsg(msg.model_dump_json()) # FIX PUBLSHING MSG

    for position, driver in enumerate(drivers, start=1):
        driverFinishedRaceEvt = DriverFinishedRace(
            raceId=raceGuid,
            driverId = driver.correlation,
            position=position
        )
        json_driverFinishedRaceEvt = driverFinishedRaceEvt.model_dump_json()
        wrapper_driverFinishedRaceEvt = EventWrapper(
            EventType="DriverFinishedRace", #driver finished race
            OcurredAt=datetime.now(),
            Payload=json_driverFinishedRaceEvt
        )
        publishMsg(wrapper_driverFinishedRaceEvt)

        driver.finished_position = position

    return drivers



