import uuid
from uuid import UUID

from pydantic import BaseModel, Field

# These events get serialized in JSON format

# StartedRace
# FinishedRace
# DriverOvertook / #DriverOvertaken (implicit)
# Disqualified
# Pitted

class StartedRace(BaseModel):
    driverId : UUID
    raceId: UUID

class FinishedRace(BaseModel):
    driverId: UUID
    raceId: UUID
    position: int

class DriverOvertook(BaseModel):
    driverFront: UUID
    driverBehind: UUID

class Disqualified(BaseModel):
    driverId: UUID
    reason: str

class Pitted(BaseModel):
    driverId: UUID
    tyreType: str = Field(..., min_length=1, max_length=1)


