import os

import pika
from pika import exchange_type
from pika.exchange_type import ExchangeType
import json

from Contracts.mq_contracts.race_messages import EventWrapper

rabbitmq_host = os.getenv("RABBITMQ_HOST", "localhost")
rabbitmq_port = int(os.getenv("RABBITMQ_PORT", "5672"))
rabbitmq_username = os.getenv("RABBITMQ_USERNAME", "guest")
rabbitmq_password = os.getenv("RABBITMQ_PASSWORD", "guest")

connection = pika.BlockingConnection(pika.ConnectionParameters(
    host=rabbitmq_host,
    port=rabbitmq_port,
    credentials=pika.PlainCredentials(rabbitmq_username, rabbitmq_password),
    connection_attempts=10,
    retry_delay=3,
))
channel = connection.channel()

race_exchange = "simulatorExchange"
race_queue = "raceevents"
race_routing_key = "raceroute"

channel.exchange_declare(exchange=race_exchange, exchange_type=ExchangeType.direct)


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

        #EventWrapper
def publishMsg(msg:EventWrapper, exchange=race_exchange, routing_key=race_routing_key):
    msgJson = msg.model_dump_json()
    msgBytes = msgJson.encode('utf-8')
    basic_properties = pika.BasicProperties()
    channel.basic_publish(
        exchange=exchange,
        routing_key=routing_key,
        body=msgBytes
    )
