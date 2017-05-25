using System;
using UnityEngine;

public class FSM{
	private string currState;
	private string prevState;
	private DateTime startTime;

	public FSM(){
		currState = "";
		prevState = "";
		startTime = DateTime.Now;
	}

	public FSM(string newState){
		currState = newState;
		prevState = "";
		startTime = DateTime.Now;
	}

	public void updateFSM(string newState){
		prevState = currState;
		currState = newState;
		startTime = DateTime.Now;
        Debug.Log("System current state is: " + newState);
	}

	public string CurrState{
		get{return currState;}
	}

	public string PrevState{
		get{return prevState;}
	}

	public DateTime StartTime{
		get{return startTime;}
	}
}