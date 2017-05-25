using System;
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Collections.Generic;
using System.IO;

public class VideoController : MonoBehaviour
{

	// Use this for initialization
	public Texture background;
	public MovieTexture[] Video;

    private bool[] brushEnable = { true, false, true, false, true, false };
    private int[] stimulusOrder = {0,1,2,3,4,5};
	private string brushCommand;
    private string brushStatus;
    private float restInterval;
    private bool playReady;
    private bool startSent = false;
    private int stimulusID;
    private List<EventInfo> eventList = new List<EventInfo>();
    private string jsonDataPath;
    private string txtDataPath;
    private string participantName;

    public static SerialPort brushSerial;
    public static SerialPort eegSerial;
	public FSM stimulusFSM = new FSM("WaitForStart");

    void Start ()
    {
        brushSerial = new SerialPort("COM4", 9600);
        brushSerial.Open();
        brushSerial.ReadTimeout = 10;
        restInterval = 15.0f;

        eegSerial = new SerialPort("COM6", 9600);
        eegSerial.Open();
        eegSerial.ReadTimeout = 10;
        restInterval = 15.0f;

        stimulusID = 0;

        // Shuffle the order of the first four stimuli
        int[] order = { 0, 1, 2, 3 };
        Shuffle(order);
        for(int i=0;i<4;i++)
        {
            Debug.Log(order[i]);
            stimulusOrder[i] = order[i];
        }

        //jsonDataPath = Application.dataPath + "\\Data\\" + GetFileName("jsonData",".json");
        //txtDataPath = Application.dataPath + "\\Data\\" + GetFileName("txtData",".txt");
        jsonDataPath = "C:\\Users\\biand\\Documents\\Affective_touch_project\\unity_read_from_arduino\\Assets\\Data\\" + GetFileName("jsonData", ".json");
        txtDataPath = "C:\\Users\\biand\\Documents\\Affective_touch_project\\unity_read_from_arduino\\Assets\\Data\\" + GetFileName("txtData", ".txt");
        AddToEventList("session_start", "N/A", false);
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(brushSerial.IsOpen)
        {
            StartCoroutine(ReadFromArduino());
        }
        if(brushStatus == "t")
        {
            playReady = true;
            startSent = false;
            brushStatus = "";
        }
        if(Input.GetKeyUp("s"))
        {
            WriteToSerial("s", brushSerial);
        }
        if(Input.GetKeyUp("e"))
        {
            WriteToSerial("e", brushSerial);
        }
        if(Input.GetKeyUp("r"))
        {
            playReady = true;
        }
	}

    // Display stimuli one by one
	void OnGUI()
    {
        if(stimulusFSM.CurrState == "WaitForStart")
        {
            if(playReady)
            {
                stimulusFSM.updateFSM("PlayStimulus");
                WriteToSerial("s", eegSerial);
                playReady = false;
                AddToEventList("start_stimulus", Video[stimulusOrder[stimulusID]].name, brushEnable[stimulusOrder[stimulusID]]);
            }
        }

        // PlayStimulus State
        else if (stimulusFSM.CurrState == "PlayStimulus")
        {
            Video[stimulusOrder[stimulusID]].Play();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background, ScaleMode.StretchToFill, false, 0.0f);
            GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), Video[stimulusOrder[stimulusID]], 
                ScaleMode.ScaleToFit, false, 0.0f);
			if(DateTime.Now.Subtract (stimulusFSM.StartTime).TotalSeconds > Video[stimulusOrder[stimulusID]].duration)
            {

                if(brushEnable[stimulusOrder[stimulusID]] == true)
                {
                    WriteToSerial("e", brushSerial);
                }
				stimulusFSM.updateFSM ("Rest");
                stimulusID++;
                AddToEventList("end_stimulus", "N/A", false);
                WriteToSerial("e", eegSerial);
            }
		}

        // Rest State
        else if (stimulusFSM.CurrState == "Rest")
        {
            if(stimulusID == Video.Length)
            {
                stimulusFSM.updateFSM("End");
                AddToEventList("session_end", "N/A", false);
                using (StreamWriter sw = new StreamWriter(txtDataPath, true))
                {
                    string tail = Environment.NewLine + "Participant name: " + participantName + Environment.NewLine + 
                        "Participate date: " + DateTime.Now.Date;
                    sw.WriteLine(tail);

                    string order = Environment.NewLine + "Stimuli order is: ";
                    foreach(int i in stimulusOrder)
                    {
                        order += i.ToString() + " ";
                    }
                    sw.WriteLine(order);
                }
                WriteToJSONFile();
            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background, ScaleMode.StretchToFill, false, 0.0f);

                if (DateTime.Now.Subtract(stimulusFSM.StartTime).TotalSeconds > restInterval)
                {
                    if (playReady)
                    {
                        stimulusFSM.updateFSM("PlayStimulus");
                        playReady = false;
                        AddToEventList("start_stimulus", Video[stimulusOrder[stimulusID]].name, brushEnable[stimulusOrder[stimulusID]]);
                    }
                    else if ((brushEnable[stimulusOrder[stimulusID]]==true) && (startSent==false))
                    {
                        WriteToSerial("s", brushSerial);
                        startSent = true;
                    }
                    else if((brushEnable[stimulusOrder[stimulusID]] == false) && (startSent == false))
                    {
                        playReady = true;
                    }
                }
            }
		}

        else if(stimulusFSM.CurrState == "End")
        {
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), background, ScaleMode.StretchToFill, false, 0.0f);
			GUI.Box(new Rect (10, 10, 100, 20), "Thanks!");
		}
	}

    // Write string value to serial port to commnicate with Arduino
	private void WriteToSerial(string command, SerialPort port)
    {
		Debug.Log (command);
		if (!port.IsOpen)
			port.Open ();
        port.Write(command);
    }

    private void AddToEventList(string msg, string name, bool brush)
    {
        EventInfo info = new EventInfo(GetTimeStamp(), msg, name, brush);
        eventList.Add(info);
        using (StreamWriter sw = new StreamWriter(txtDataPath, true))
        {
            sw.WriteLine(info.SaveToOneLineString());
        }
    }

    private void WriteToJSONFile()
    {
        string json = "[";
        foreach(EventInfo e in eventList)
        {
            json += JsonUtility.ToJson(e) + "," + Environment.NewLine;
        }
        json = json.Substring(0,json.Length-1-Environment.NewLine.Length) + "]";
        File.WriteAllText(jsonDataPath, json);
    }

    private string GetTimeStamp()
    {
        string s = "";
        DateTime moment = DateTime.Now;

        s += moment.Hour + ":";
        s += moment.Minute + ":";
        s += moment.Second + ":";
        s += moment.Millisecond;

        return s;
    }

    private string GetFileName(string name,string format)
    {
        string t = "";
        DateTime moment = DateTime.Now;
        t += "-" + moment.Hour;
        t += "-" + moment.Minute;
        return name + t + format;
    }

    // Start the experiment, callback function for Start Button
    public void StartTask()
    {
        if(brushEnable[stimulusOrder[0]] == true)
        {
            WriteToSerial("s", brushSerial);
        }
        else
        {
            playReady = true;
        }
    }

    public void EnterParticipantName(string name)
    {
        participantName = name;
    }

    // Read from Arduino, runs every frame
    public IEnumerator ReadFromArduino()
    {
        try
        {
            brushStatus = brushSerial.ReadLine();
            Debug.Log(brushStatus);
        }
        catch (TimeoutException)
        {
            //Debug.Log(te.ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        yield return null;
    }

    void OnApplicationQuit()
    {
        WriteToSerial("e", brushSerial);
        if (brushSerial != null)
        {
            if (brushSerial.IsOpen)
            {
                print("Closing brush serial port");
                brushSerial.Close();
            }

            brushSerial = null;
        }

        if (eegSerial != null)
        {
            if (eegSerial.IsOpen)
            {
                print("Closing eeg serial port");
                eegSerial.Close();
            }

            eegSerial = null;
        }
    }

    //void OnApplicationPause()
    //{
    //    WriteToSerial("e");
    //}

    // Automatically get the serial port name
    private string GetPortName()
    {
        string[] portNames;

        portNames = SerialPort.GetPortNames();
        if (portNames.Length > 0)
        {
            return portNames[portNames.Length - 1];
        }
        else
            Debug.Log("No serial port found.");
        return "";
    }

    /// <summary>
    /// Shuffle the array.
    /// </summary>
    /// <typeparam name="T">Array element type.</typeparam>
    /// <param name="array">Array to shuffle.</param>
    private void Shuffle<T>(T[] array)
    {
        System.Random _random = new System.Random();
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random() in Java.
            int r = i + (int)(_random.NextDouble() * (n - i));
            T t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }

}
