import os
import threading

import pika
from pika.exchange_type import ExchangeType

from Contracts.mq_contracts.race_messages import EventWrapper

rabbitmq_host = os.getenv("RABBITMQ_HOST", "localhost")
rabbitmq_port = int(os.getenv("RABBITMQ_PORT", "5672"))
rabbitmq_username = os.getenv("RABBITMQ_USERNAME", "guest")
rabbitmq_password = os.getenv("RABBITMQ_PASSWORD", "guest")

race_exchange = "simulatorExchange"
race_queue = "raceevents"
race_routing_key = "raceroute"

# Publishing happens from request handlers, so the shared channel needs a lock:
# a pika channel is not safe to use from more than one thread at a time.
_lock = threading.Lock()
_connection = None
_channel = None


def _connect():
    """Open a connection and declare the topology the Event Store consumes from."""
    connection = pika.BlockingConnection(pika.ConnectionParameters(
        host=rabbitmq_host,
        port=rabbitmq_port,
        credentials=pika.PlainCredentials(rabbitmq_username, rabbitmq_password),
        connection_attempts=10,
        retry_delay=3,
        # A publish-only client sits idle between races and never runs pika's event loop,
        # so it cannot answer heartbeats; without this the broker drops the connection.
        heartbeat=0,
    ))

    channel = connection.channel()

    channel.exchange_declare(
        exchange=race_exchange,
        exchange_type=ExchangeType.direct
    )

    channel.queue_declare(
        queue=race_queue,
        durable=True,
        passive=False,
        exclusive=False
    )

    channel.queue_bind(
        queue=race_queue,
        exchange=race_exchange,
        routing_key=race_routing_key
    )

    return connection, channel


def _discard_connection():
    global _connection, _channel
    try:
        if _connection is not None and _connection.is_open:
            _connection.close()
    except Exception:
        pass
    _connection, _channel = None, None


def _ensure_channel():
    global _connection, _channel
    if _channel is not None and _channel.is_open:
        return _channel
    _connection, _channel = _connect()
    return _channel


def publishMsg(msg: EventWrapper, exchange=race_exchange, routing_key=race_routing_key):
    msgBytes = msg.model_dump_json().encode('utf-8')

    # The queue is durable, so mark the message persistent as well: otherwise a broker
    # restart drops everything the Event Store has not consumed yet.
    properties = pika.BasicProperties(delivery_mode=2, content_type="application/json")

    with _lock:
        for attempt in (1, 2):
            try:
                _ensure_channel().basic_publish(
                    exchange=exchange,
                    routing_key=routing_key,
                    body=msgBytes,
                    properties=properties
                )
                return
            except (pika.exceptions.AMQPError, OSError):
                # The connection went away between races. Rebuild it once and retry.
                _discard_connection()
                if attempt == 2:
                    raise
