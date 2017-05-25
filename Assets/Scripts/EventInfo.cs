using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class EventInfo
{
    public string timeStamp;
    public string message;
    public string stimulusName;
    public bool brushEnabled;

    public EventInfo(string t,string msg,string name,bool brush=false)
    {
        this.timeStamp = t;
        this.message = msg;
        this.stimulusName = name;
        this.brushEnabled = brush;
    }

    public string SaveToOneLineString()
    {
        return this.timeStamp + ' ' + this.message + ' ' + this.stimulusName + ' ' + this.brushEnabled.ToString();
    }

    public string SaveToJSONString()
    {
        return JsonUtility.ToJson(this);
    }

    public static EventInfo CreateFromJSON(string json)
    {
        return JsonUtility.FromJson<EventInfo>(json);
    }
}
