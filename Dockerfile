FROM mcr.microsoft.com/dotnet/sdk:10.0 AS eventstore-build
WORKDIR /src

COPY ["src/event-store/eventstore/eventstore.csproj", "src/event-store/eventstore/"]
RUN dotnet restore "src/event-store/eventstore/eventstore.csproj"

COPY src/event-store/eventstore/ src/event-store/eventstore/
WORKDIR /src/src/event-store/eventstore
RUN dotnet publish "eventstore.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS eventstore
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=eventstore-build /app/publish ./
ENTRYPOINT ["dotnet", "eventstore.dll"]

FROM python:3.13-slim AS simulator
WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

COPY src/simulator/requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY src/simulator/ ./
EXPOSE 8000

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]