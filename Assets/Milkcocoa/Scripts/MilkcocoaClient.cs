#region License
/*
 * Milkcocoa.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014-2016 m2wasabi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
#if SSL
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#endif
using UnityEngine;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Milkcocoa
{
    public class MilkcocoaClient : MonoBehaviour
    {
        private MqttClient mqttClient;
        public string appId ;
        public string dataStorePath = "unity";
        public bool debugMessages = false;
        private string clientId;

        bool connection = false;
        private int seq = 1;

        private Dictionary<string, List<Action<MilkcocoaEvent>>> handlers;


        public void Awake()
        {
            if (debugMessages) Debug.Log("Awake Milkcocoa.");
            connection = true;
            handlers = new Dictionary<string, List<Action<MilkcocoaEvent>>>();
#if SSL
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnRemoteCertificateValidationCallback);
            X509Certificate caCert = X509Certificate.CreateFromCertFile("Assets/Milkcocoa/cert/ca.der");
            mqttClient = new MqttClient(appId + ".mlkcca.com", MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT, true, caCert, null,MqttSslProtocols.TLSv1_0 );
#else
            mqttClient = new MqttClient(appId + ".mlkcca.com");
#endif
            clientId = Guid.NewGuid().ToString();
            mqttClient.MqttMsgPublishReceived += OnReceiveMqttMessage;
            mqttClient.ConnectionClosed += OnDisconnectedMqtt;
            mqttClient.Connect(clientId, "sdammy", appId);

            mqttClient.Subscribe(new string[] { appId + "/" + dataStorePath + "/send" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            mqttClient.Subscribe(new string[] { appId + "/" + dataStorePath + "/push" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

        }
#if SSL
        private bool OnRemoteCertificateValidationCallback(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            return true;  // 「SSL証明書の使用は問題なし」と示す
        }
#endif
        private void OnReceiveMqttMessage(object sender, MqttMsgPublishEventArgs e)
        {
            string[] topics = e.Topic.Split(new char[] {'/'});
            string _event = topics[2];
            switch (_event)
            {
                case "push":
                    break;
                case "send":
                    break;
            }
            if (debugMessages) Debug.Log(_event + " Received from Milkcocoa: " + System.Text.Encoding.UTF8.GetString(e.Message));
            if (!handlers.ContainsKey(_event)) { return; }
            foreach (Action<MilkcocoaEvent> handler in this.handlers[_event])
            {
                try
                {
                    handler(new MilkcocoaEvent(_event,new JSONObject(Encoding.UTF8.GetString(e.Message))));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
            }

        }
        private void OnDisconnectedMqtt(object sender, EventArgs e)
        {
            if (debugMessages) Debug.Log("Disconnected from Milkcocoa.");
            if (connection)
            {
                if (debugMessages) Debug.Log("Reconnect Milkcocoa.");
                mqttClient = new MqttClient(appId + ".mlkcca.com");
                mqttClient.MqttMsgPublishReceived += OnReceiveMqttMessage;
                mqttClient.ConnectionClosed += OnDisconnectedMqtt;
                mqttClient.Connect(clientId, "sdammy", appId);
                mqttClient.Subscribe(new string[] { appId + "/" + dataStorePath + "/send" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                mqttClient.Subscribe(new string[] { appId + "/" + dataStorePath + "/push" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            }
        }

        /*
        //一時停止or再開時
        private void OnApplicationPause(bool pauseStatus)
        {

            //一時停止
            if (pauseStatus)
            {
                if (debugMessages) Debug.Log("Pause Milkcocoa.");
                connection = false;

            }
            //再開時
            else
            {
                if (debugMessages) Debug.Log("Rstart Milkcocoa.");
                connection = true;
                mqttClient = new MqttClient(appId + ".mlkcca.com");
                mqttClient.MqttMsgPublishReceived += OnReceiveMqttMessage;
                mqttClient.ConnectionClosed += OnDisconnectedMqtt;
                mqttClient.Connect(clientId, "sdammy", appId);
                mqttClient.Subscribe(new string[] { appId + "/" + dataStorePath + "/send" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                mqttClient.Subscribe(new string[] { appId + "/" + dataStorePath + "/push" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            }

        }
        */

        //終了処理
        private void OnApplicationQuit()
        {
            if (debugMessages) Debug.Log("Shutdown Milkcocoa.");
            connection = false;
            if (mqttClient.IsConnected)
            {
                if (debugMessages) Debug.Log("Disconnect Milkcocoa.");
                mqttClient.Disconnect();
            }
        }
        public void Send(JSONObject data)
        {
            if (debugMessages) Debug.Log("sending...");
            string s = seq.ToString();
            mqttClient.Publish(appId + "/" + dataStorePath + "/send", Encoding.UTF8.GetBytes("{\"path\":\"" + dataStorePath + "\",\"params\":" + data  + ",\"r\":" + s + "}"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            if (debugMessages) Debug.Log("sent to Milkcocoa");
            seq++;

        }
        public void Push(JSONObject data)
        {
            if (debugMessages) Debug.Log("pushing...");
            string s = seq.ToString();
            mqttClient.Publish(appId + "/" + dataStorePath + "/push", Encoding.UTF8.GetBytes("{\"path\":\"" + dataStorePath + "\",\"params\":" + data + ",\"r\":" + s + "}"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            if (debugMessages) Debug.Log("push to Milkcocoa");
            seq++;

        }

        public void On(string ev, Action<MilkcocoaEvent> callback)
        {
            if (!handlers.ContainsKey(ev))
            {
                handlers[ev] = new List<Action<MilkcocoaEvent>>();
            }
            handlers[ev].Add(callback);
        }
        public void Off(string ev, Action<MilkcocoaEvent> callback)
        {
            if (!handlers.ContainsKey(ev))
            {
                return;
            }

            List<Action<MilkcocoaEvent>> l = handlers[ev];
            if (!l.Contains(callback))
            {
                return;
            }

            l.Remove(callback);
            if (l.Count == 0)
            {
                handlers.Remove(ev);
            }
        }
        public void OnSend(Action<MilkcocoaEvent> callback)
        {
            On("send", callback);
        }
        public void OnPush(Action<MilkcocoaEvent> callback)
        {
            On("push", callback);
        }
    }
}
