using System.Collections;
using UnityEngine;
namespace PilotoStudio
{
	public class Rotator : MonoBehaviour
	{
		public float x = 0f;
		public float y = 0f;
		public float z = 0f;

		public bool useGlobal;



		private void Update()
		{
			if (!useGlobal)
			{
				this.transform.Rotate(new Vector3(x, y, z) * Time.deltaTime);

			}
			else
			{
				this.transform.Rotate(new Vector3(x, y, z) * Time.deltaTime, Space.Self);
			}


		}

	}
}