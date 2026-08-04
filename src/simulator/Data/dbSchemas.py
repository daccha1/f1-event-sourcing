from Data.database import Base
from sqlalchemy.orm import Mapped, mapped_column
from math import floor

class Race(Base):
    __tablename__ = "races"

    id: Mapped[int] = mapped_column(primary_key=True)
    name: Mapped[str] = mapped_column(nullable=False)
    laps: Mapped[int] = mapped_column(nullable=False)
    length: Mapped[float] = mapped_column(nullable=False)
    pit_number: Mapped[int] = mapped_column(default=2)
    pit_after: Mapped[int] = mapped_column(default=10)

    def pit_setup(self):
        self.pit_number = floor(self.laps / 20)
        self.pit_after = 20


class Driver(Base):
    __tablename__ = "drivers"

    id: Mapped[int] = mapped_column(primary_key=True)
    name: Mapped[str] = mapped_column(nullable=False)
    team: Mapped[str] = mapped_column(nullable=False)

    racing_coeff: Mapped[float] = mapped_column(nullable=False)
    pit_coeff: Mapped[float] = mapped_column(default=0.05)
    crash_coeff: Mapped[float] = mapped_column(default=0.0001)

    pits: Mapped[int] = mapped_column(default=0)

    def setCrash(self):
        self.crash_coeff = self.pit_coeff / 2

    def setPit(self):
        self.pit_coeff = self.pit_coeff * 2