using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace Lab2
{
    public class Lab2Manager : MonoBehaviour
    {
        public GameObject result;
        public CanvasGroup loginUI;
        public GameObject loginObject;

        public CanvasGroup dbUI;
        public GameObject dbOject;

        int MAX_TEMP = 100;
        int MAX_HUMID = 100;
        public Text temp;
        public Text humid;
        public Text temp_gauge;
        public Text humid_gauge;
        public Image temp_cir;
        public Image humid_cir;

        int ledControl = 1;
        int pumpControl = 1;
        public GameObject ledToggle;
        public GameObject pumpToggle;

        public void showErrorWhenConnectFailed(string msg)
        {
            result.GetComponent<Text>().text = msg;
        }

        public void hideLogin()
        {
            loginUI.alpha = 0;
            loginObject.SetActive(false);
            dbOject.SetActive(true);
            dbUI.alpha = 1;
        }

        public void Update_Sensor(Sensor_Data sensorData)
        {
            int cnt = 0;
            
            foreach (string x in sensorData.Name)
            {
                switch (x)
                {
                    case "temperature":
                        temp.text = sensorData.Value[cnt].ToString() + "°C";
                        temp_gauge.text = sensorData.Value[cnt].ToString() + "°C";
                        temp_cir.fillAmount = (float)Int32.Parse(sensorData.Value[cnt].ToString()) / (float)MAX_TEMP;
                        break;
                    case "humidity":
                        humid.text = sensorData.Value[cnt] + "%";
                        humid_gauge.text = sensorData.Value[cnt] + "%";
                        humid_cir.fillAmount = (float)Int32.Parse(sensorData.Value[cnt].ToString()) / (float)MAX_HUMID;
                        break;
                    default:
                        break;
                }
                cnt += 1;
            }
        }

        public void Update_Control(Control_Data controlData)
        {
            if (controlData.device == "LED")
            {
                if ((controlData.status == "ON" && ledControl == 0) || (controlData.status == "OFF" && ledControl == 1))
                    ToggleLedBtn();
            }
            else if (controlData.device == "PUMP")
            {
                if ((controlData.status == "ON" && pumpControl == 0) || (controlData.status == "OFF" && pumpControl == 1))
                    TogglePumpBtn();
            }
        }

        public void ToggleLedBtn()
        {
            ledToggle.transform.DOLocalMoveX(-ledToggle.transform.localPosition.x, 0.2f);
            ledControl = (ledControl + 1 ) % 2;
            Control_Data forLed = new Control_Data();
            forLed.device = "LED";
            forLed.status = (ledControl == 1) ? "ON" : "OFF";
            GetComponent<Lab2MQTT>().PublishLed(forLed);
        }

        public void TogglePumpBtn()
        {
            pumpToggle.transform.DOLocalMoveX(-pumpToggle.transform.localPosition.x, 0.2f);
            pumpControl = (pumpControl + 1) % 2;
            Control_Data forPump = new Control_Data();
            forPump.device = "PUMP";
            forPump.status = (pumpControl == 1) ? "ON" : "OFF";
            GetComponent<Lab2MQTT>().PublishPump(forPump);
        }

        void Start()
        {
            loginObject.SetActive(true);
            loginUI.alpha = 1;
            dbUI.alpha = 0;
            dbOject.SetActive(false);
        }

        void Update()
        {

        }
    }

}
