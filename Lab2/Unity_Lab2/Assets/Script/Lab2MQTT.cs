using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace Lab2
{
    public class Sensor_Data
    {
        [JsonProperty("name")]
        public List<string> Name { get; set; }
        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }

    public class Control_Data
    {
        public string device { get; set; }
        public string status { get; set; }
    }
    
    public class Lab2MQTT : M2MqttUnityClient
    {
        public List<string> topics = new List<string>();
        private List<string> eventMessages = new List<string>();
        public Text input1;
        public Text input2;
        public Text input3;

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }

        public void PublishLed(Control_Data led)
        {
            string msg = JsonConvert.SerializeObject(led);
            client.Publish(topics[1], System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("Publish LED!");
        }

        public void PublishPump(Control_Data pump)
        {
            string msg = JsonConvert.SerializeObject(pump);
            client.Publish(topics[2], System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("Publish Pump!");
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            Debug.Log("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            GetComponent<Lab2Manager>().hideLogin();
            SubscribeTopics();
        }

        protected override void SubscribeTopics()
        {

            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

                }
            }
        }

        protected override void UnsubscribeTopics()
        {
            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Unsubscribe(new string[] { topic });
                }
            }

        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            GetComponent<Lab2Manager>().showErrorWhenConnectFailed(errorMessage);
            base.Disconnect();
        }

        protected override void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            Debug.Log("CONNECTION LOST!");
        }

        public void myStart()
        {
            base.brokerAddress = input1.text;
            base.mqttUserName = input2.text;
            base.mqttPassword = input3.text;
            base.Connect();
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            //StoreMessage(msg);
            if (topic == topics[0])
                ProcessMessageSensor(msg);

            if (topic == topics[1] || topic == topics[2])
                ProcessMessageControl(msg);

        }

        private void ProcessMessageSensor(string msg)
        {
            Sensor_Data sensorData = JsonConvert.DeserializeObject<Sensor_Data>(msg);
            GetComponent<Lab2Manager>().Update_Sensor(sensorData);
        }

        private void ProcessMessageControl(string msg)
        {
            Control_Data controlData = JsonConvert.DeserializeObject<Control_Data>(msg);
            GetComponent<Lab2Manager>().Update_Control(controlData);
        }
        
        private void OnDestroy()
        {
            Disconnect();
        }
    }

}