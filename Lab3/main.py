print("IoT Gateway")
import paho.mqtt.client as mqttclient
import time
import json
import serial.tools.list_ports

BROKER_ADDRESS = "demo.thingsboard.io"
PORT = 1883
mess = ""

#TODO: Add your token and your comport
#Please check the comport in the device manager
THINGS_BOARD_ACCESS_TOKEN = "c5wFYgBcXgCc1vLOcx6v"
bbc_port = "COM9"
if len(bbc_port) > 0:
    ser = serial.Serial(port=bbc_port, baudrate=115200)

def processData(data):
    data = data.replace("!", "")
    data = data.replace("#", "")
    splitData = data.split(":")
    print(splitData)
    #TODO: Add your source code to publish data to the server
    temp_data = {'temperature' : int(splitData[2])}
    light_data = {'light' : int(splitData[2])}
    if splitData[1] == 'TEMP':
        client.publish('v1/devices/me/telemetry', json.dumps(temp_data), 1)
    elif splitData[1] == 'LIGHT':
        client.publish('v1/devices/me/telemetry', json.dumps(light_data), 1)

def readSerial():
    bytesToRead = ser.inWaiting()
    if (bytesToRead > 0):
        global mess
        mess = mess + ser.read(bytesToRead).decode("UTF-8")
        while ("#" in mess) and ("!" in mess):
            start = mess.find("!")
            end = mess.find("#")
            processData(mess[start:end + 1])
            if (end == len(mess)):
                mess = ""
            else:
                mess = mess[end+1:]


def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")

def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    led_data = {'led': True}
    fan_data = {'fan': True}
    cmd = -1
    #TODO: Update the cmd to control 2 devices
    try:
        jsonobj = json.loads(message.payload)
        if jsonobj['method'] == "setValueLed":
            if jsonobj['params'] == True:
                cmd = 0
            if jsonobj['params'] == False:
                cmd = 1
            led_data['led'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(led_data), 1)
        if jsonobj['method'] == "setValueFan":
            if jsonobj['params'] == True:
                cmd = 2
            if jsonobj['params'] == False:
                cmd = 3
            fan_data['fan'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(fan_data), 1)
    except:
        pass

    if len(bbc_port) > 0:
        ser.write((str(cmd) + "#").encode())

def connected(client, usedata, flags, rc):
    if rc == 0:
        print("Thingsboard connected successfully!!")
        client.subscribe("v1/devices/me/rpc/request/+")
    else:
        print("Connection is failed")


client = mqttclient.Client("Gateway_Thingsboard")
client.username_pw_set(THINGS_BOARD_ACCESS_TOKEN)

client.on_connect = connected
client.connect(BROKER_ADDRESS, 1883)
client.loop_start()

client.on_subscribe = subscribed
client.on_message = recv_message


while True:

    if len(bbc_port) >  0:
        readSerial()

    time.sleep(1)