using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class VideoStimulus : MonoBehaviour {

    public SerialPort serial;
    public MovieTexture[] Video;

    private string brushCommand;
    private string brushStatus;
    private bool playReady;
	// Use this for initialization
	void Start ()
    {
        serial = new SerialPort(GetPortName(), 9600);
        serial.ReadTimeout = 1;
        serial.Open();
        serial.DiscardInBuffer();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (serial.IsOpen)
        {
            StartCoroutine(ReadFromSerial());
        }

        if (brushStatus == "t")
        {
            playReady = true;
        }
    }

    void OnGUI()
    {
        if(playReady == true)
        {
            Video[0].Play();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Video[0], 
                ScaleMode.StretchToFill, false, 0.0f);
        }
    }

    public void StartDemo()
    {
        Debug.Log(brushCommand);
        if (serial.IsOpen)
        {
            serial.Write(brushCommand);
        }
    }

    public void BrushCommand(string command)
    {
        brushCommand = command;
    }

    public IEnumerator ReadFromSerial()
    {
        try
        {
            brushStatus = serial.ReadLine();
            Debug.Log(brushStatus);
        }
        catch (System.TimeoutException)
        {
            //Debug.Log(te.ToString());
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        yield return null;
    }

    void OnApplicationQuit()
    {
        if (serial != null)
        {
            if (serial.IsOpen)
            {
                print("Closing serial port");
                serial.Close();
            }

            serial = null;
        }
    }

    private string GetPortName()
    {
        string[] portNames;

        portNames = System.IO.Ports.SerialPort.GetPortNames();
        if (portNames.Length > 0)
        {
            return portNames[portNames.Length - 1];
        }
        else
            Debug.Log("No serial port found.");
            return "";
    }
}
