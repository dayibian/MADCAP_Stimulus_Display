using UnityEngine;
using System.Collections;
using System.IO.Ports;
using UnityEngine.Events;

public class VehicleController : MonoBehaviour {

    // Use this for initialization
    public static SerialPort serial;
    public float speed;

    private string brush_status;
    private AudioSource siren;
    private bool playReady;
	
    void Start()
    {
        serial = new SerialPort(GetPortName(), 9600);
        serial.ReadTimeout = 1;
        serial.Open();
        serial.DiscardInBuffer();
        siren = GetComponent<AudioSource>();
    }
	// Update is called once per frame
	void Update ()
    {
        if(serial.IsOpen)
        {
            StartCoroutine(ReadFromSerial());
        }

        if (brush_status == "touched")
        {
            playReady = true;
        }

        if(playReady == true)
        {
            transform.Translate(new Vector3(0, 0, speed) * Time.deltaTime);
            if (!siren.isPlaying)
            {
                siren.Play();
            }
        }

	}

    public void BrushCommand(string command)
    {
        Debug.Log(command);
        if(serial.IsOpen)
        {
            serial.Write(command);
        }
    }

    public IEnumerator ReadFromSerial()
    {
        try
        {
            brush_status = serial.ReadLine();
            Debug.Log(brush_status);
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
