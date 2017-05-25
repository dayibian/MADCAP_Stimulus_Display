using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class arduino_test : MonoBehaviour {

	public SerialPort serial = new SerialPort("COM9",9600);
	// Use this for initialization
	void Start () {
		try{
			serial.Open ();
			serial.ReadTimeout = 1;
		}
		catch{
			Debug.Log("something wrong when open the serial port");
		}
		if (serial.IsOpen) {
			Debug.Log("Serial is open");
		}
		Thread.Sleep (3000);
	}
	
	// Update is called once per frame
	void Update () {
		try{
			if (serial.BytesToRead != 0) {
				int indata = serial.ReadByte();
				Debug.Log(indata);
				}
		}
		catch{
			Debug.Log("Something went wrong.");
		}
	}
}
