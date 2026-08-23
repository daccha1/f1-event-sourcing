import uuid
from uuid import UUID

from pydantic import BaseModel, Field

# These events get serialized in JSON format

# StartedRace
# FinishedRace
# DriverOvertook / #DriverOvertaken (implicit)
# Disqualified
# Pitted

class DriverStartedRace(BaseModel):
    driverId : UUID
    raceId: UUID

class DriverFinishedRace(BaseModel):
    driverId: UUID
    raceId: UUID
    position: int

class DriverOvertook(BaseModel):
    driverFront: UUID
    driverBehind: UUID

class DriverDisqualified(BaseModel):
    driverId: UUID
    reason: str

class DriverPitted(BaseModel):
    driverId: UUID
    tyreType: str = Field(..., min_length=1, max_length=1)


