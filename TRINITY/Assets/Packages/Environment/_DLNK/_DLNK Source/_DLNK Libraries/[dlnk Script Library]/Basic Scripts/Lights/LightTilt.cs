using UnityEngine;
using System.Collections;

public class LightTilt : MonoBehaviour {


	// DLNK WORKSHOP WWW.DAELONIK.COM

	public float Tempo = 1f;

	// Light Values
	public bool EnableIntensity = true;
	public float RandomIntensity = 0.1f;
	public float Fluctuation = 0.2f;

	private float OriginalInt;
	private float TiltInt;
	private float Tmp;


	// Position Values

	public bool EnableMovement;
	public float MovFluct = 0.01f;
	public float PositionRandom = 0.1f;
	public float RangeFluct = 0.1f;

	private Vector3 OriginalPos = new Vector3();
	private float OriginalRan;
	private float TiltMovx;
	private float TiltMovy;
	private float TiltMovz;


	// Color Values

	public bool EnableColorChange;
	public Color TiltColor = Color.white;

	private Color OriginalColor;


	// Taking initial values
	void Start () {

		OriginalInt = (this.GetComponent<Light>().intensity);
		OriginalPos = (this.GetComponent<Light>().transform.position);
		OriginalColor = (this.GetComponent<Light>().color);
		OriginalRan = (this.GetComponent<Light>().range);
		//Debug.Log (OriginalPos);
	
	}



	// Light Tilt
	void Update () {

		Tmp = (Tmp + ((Time.deltaTime)*Tempo));
		//Debug.Log (Tmp);

			// LIGHT INTENSITY
		if (EnableIntensity) {
			TiltInt = ((OriginalInt + ((Mathf.Sin (Tmp)) * Fluctuation))+ (Random.Range(0,RandomIntensity)));
			this.GetComponent<Light>().intensity = TiltInt;
			//Debug.Log (TiltInt);
	
				}


			// LIGHT POSITION
		if (EnableMovement) {
			TiltMovx = (((Mathf.Cos (Mathf.Sin(Tmp))) * MovFluct))+ (Random.Range(-PositionRandom,PositionRandom));
			TiltMovy = (((Mathf.Sin (Mathf.Cos(Tmp))) * MovFluct))+ (Random.Range(-PositionRandom,PositionRandom));
			TiltMovz = (((Mathf.Sin (Mathf.Sin(Tmp))) * MovFluct))+ (Random.Range(-PositionRandom,PositionRandom));
			this.GetComponent<Light>().transform.position = new Vector3(OriginalPos.x + TiltMovx,OriginalPos.y + TiltMovy,OriginalPos.z + TiltMovz);
			this.GetComponent<Light>().range= (OriginalRan + ((Mathf.Sin (Tmp)) * RangeFluct));
		//	Debug.Log();

				}


			// LIGHT COLOR
		if (EnableColorChange) {
			this.GetComponent<Light>().color = Color.Lerp(OriginalColor,TiltColor, ( Mathf.PingPong (Time.time, (Tempo)) / Tempo));
				}

		}
}
