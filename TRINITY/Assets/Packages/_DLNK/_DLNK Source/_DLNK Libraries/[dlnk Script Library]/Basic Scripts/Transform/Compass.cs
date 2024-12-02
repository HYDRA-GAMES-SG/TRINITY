using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class Compass : MonoBehaviour {

	public GameObject Target;
	public Vector3 RotationIntensity = new Vector3(1,1,1);
	public bool continuous = true;


	void Start () {

		if (!continuous)
			{
				gameObject.transform.LookAt (Target.transform.position);
				gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x*RotationIntensity.x, gameObject.transform.eulerAngles.y*RotationIntensity.y, gameObject.transform.eulerAngles.z*RotationIntensity.z);
			}
	}


	void Update () {

		if (continuous) 
		{
			gameObject.transform.LookAt (Target.transform.position);
			gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x*RotationIntensity.x, gameObject.transform.eulerAngles.y*RotationIntensity.y, gameObject.transform.eulerAngles.z*RotationIntensity.z);
		}
	}
}
