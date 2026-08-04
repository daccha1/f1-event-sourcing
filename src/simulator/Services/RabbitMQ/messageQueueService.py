import pika
from pika import exchange_type
from pika.exchange_type import ExchangeType
import json
from Contracts.mq_contracts.race_messages import start_evt, EventType, RaceMessage

connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost'))
channel = connection.channel()

race_exchange = "race"
race_queue = "race-events"
race_routing_key = "race-route"

channel.exchange_declare(exchange=race_exchange, exchange_type=ExchangeType.direct)


channel.queue_declare(
    queue=race_queue,
    durable=True,
    passive=False,
)

channel.queue_bind(
    queue=race_queue,
    exchange=race_exchange,
    routing_key=race_routing_key
)


def publishMsg(msg, exchange=race_exchange, routing_key=race_routing_key):
    msgJson = json.dumps(msg)
    msgBytes = msgJson.encode('utf-8')
    basic_properties = pika.BasicProperties()
    channel.basic_publish(
        exchange=exchange,
        routing_key=routing_key,
        body=msgBytes
    )
