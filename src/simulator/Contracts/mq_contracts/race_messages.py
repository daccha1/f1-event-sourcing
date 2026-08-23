import uuid
from datetime import datetime
from enum import Enum

from pydantic import BaseModel, Field

class EventWrapper(BaseModel):
    CorrelationId : str = Field(default_factory=lambda: str(uuid.uuid4()))
    Payload : str
    EventType : str
    OcurredAt : datetime


