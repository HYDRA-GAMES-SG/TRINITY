using UnityEngine;
using System.Collections;

public class RainGUI : MonoBehaviour {

	float currentTime = 1;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnGUI () {
		GUI.Label(new Rect(15,30, 120, 35), "Time");
		currentTime = GUI.HorizontalSlider(new Rect(50, 37, 120, 15), currentTime, 0.02f, 1);
		Time.timeScale = currentTime;
	}
}
