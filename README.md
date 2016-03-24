Milkcocoa client for Unity
==========================

[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://github.com/m2wasabi/milkcocoa-client-unity/blob/master/LICENSE)

## About

This project is MOST easy way for realtime network comminucation in [Unity](http://unity3d.com).

There's nothing to setup network server for multiplayers!

Using [Milkcocoa](http://mlkcca.com) as a BaaS(Backend as a Service).

## Dependencies

* Unity 4.6 , 5.3 (Demo project can run only 5.x)
* [JSONObject](https://github.com/mtschoen/JSONObject) 
* [M2Mqtt](https://github.com/ppatierno/m2mqtt) v4.3.0.0
* [Milkcocoa](http://mlkcca.com)

## Usage

+ Sign up Milkcocoa service  
    Sign up and create Application in [Milkcocoa](http://mlkcca.com)
+ Create Milkcocoa prefab in sceme  
    Milkcocoa prefab (Assets/Milkcocoa/Prefabs/Milkcocoa) drop into hierarchy window
+ Configure in unity  
    Edit Milkcocoa prefab in hierarchy.  
    
    |param | description |
    |:---|:---|
    |App Id | Milkcocoa application id|
    |Data Store Path | data namespace in Milkcocoa|

+ Write code

## Code examples

**1. Find object and attach event on send**

```c#
using UnityEngine;
using System;
using Milkcocoa;
public class example1 : MonoBehaviour {
    MilkcocoaClient milkcocoa;

    void Start () {
        milkcocoa = FindObjectOfType<MilkcocoaClient>();
        milkcocoa.OnSend(milkcocoaEventHandler);
    }

    public void milkcocoaEventHandler(MilkcocoaEvent e)
    {
        // example print
        Debug.Log(e.GetValues());

        // example search
        if (e.data.GetField("params").HasField("chat"))
        {
            string message = e.data.GetField("params").GetField("chat").GetField("message").str;
            string username = e.data.GetField("params").GetField("chat").GetField("name").str;
        }
    }
}
```

**2. Send event**
```c#
using UnityEngine;
using System;
using Milkcocoa;
public class example2 : MonoBehaviour {
    MilkcocoaClient milkcocoa;

    void Start () {
        milkcocoa = FindObjectOfType<MilkcocoaClient>();
    }

    private void sendMessage(string str)
    {
        JSONObject jsonobj = new JSONObject(delegate(JSONObject values)
        {
            values.AddField("chat", delegate(JSONObject chat)
            {
                chat.AddField("name", "John");
                chat.AddField("message", Uri.EscapeDataString(str));
            });
        });
        milkcocoa.Send(jsonobj);
    }
}
```


## Reference

### Class: MilkcocoaClient

##### properties

* public string MilkcocoaClient.appId
* public string MilkcocoaClient.dataStorePath
* public bool MilkcocoaClient.debugMessages
* string MilkcocoaClient.clientId

##### methods

* void MilkcocoaClient.Send(JSONObject)
* void MilkcocoaClient.Push(JSONObject)

* void MilkcocoaClient.OnSend(Action&lt;MilkcocoaEvent&gt;)
* void MilkcocoaClient.OnPush(Action&lt;MilkcocoaEvent&gt;)


### Class: MilkcocoaEvent


##### properties

* string MilkcocoaEvent.name
* JSONObject MilkcocoaEvent.data

##### methods

* JSONObject MilkcocoaEvent.GetValues()

