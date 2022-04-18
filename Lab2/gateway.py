import paho.mqtt.client as mqttclient
import time
import json


BROKER_ADDRESS = "mqttserver.tk"
PORT = 1883
USER_NAME = "bkiot"
PASS = "12345678"


def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")


def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    temp_data = {'value': True}
    try:
        jsonobj = json.loads(message.payload)
        if jsonobj['method'] == "setValue":
            temp_data['value'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
    except:
        pass


def connected(client, usedata, flags, rc):
    if rc == 0:
        print("Connected successfully!!")
        client.subscribe("v1/devices/me/rpc/request/+")
    else:
        print("Connection is failed")


client = mqttclient.Client("Gateway_Thingsboard")
client.username_pw_set(USER_NAME, PASS)

client.connect(BROKER_ADDRESS, 1883)
client.loop_start()


temp = 30
humi = 50

while True:
    collect_data = { 'name' : ['temperature', 'humidity'],
                     'value' : [temp, humi]
                    }      
    client.publish('/bkiot/1914691/status',
                        json.dumps(collect_data), 1)

    led_data = {'device':'LED', 'status':'OFF'}
    pump_data = {'device':'PUMP', 'status':'OFF'}
    # client.publish('/bkiot/1914691/led', json.dumps(led_data), 1)
    # client.publish('/bkiot/1914691/pump', json.dumps(pump_data), 1)
    temp += 5
    humi += 5
    time.sleep(10)