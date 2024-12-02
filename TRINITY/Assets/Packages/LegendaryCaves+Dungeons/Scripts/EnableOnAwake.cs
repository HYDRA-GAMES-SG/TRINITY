using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LegendaryCavesAndDungeons
{
    public class EnableOnAwake : MonoBehaviour
    {
        public GameObject EnableObject;

        // Update is called once per frame
        void Awake()
        {
            EnableObject.SetActive( false );
            StartCoroutine( ExampleCoroutine() );
        }

        private IEnumerator ExampleCoroutine()
        {
            yield return new WaitForSeconds( 2 );
            EnableObject.SetActive( true );
        }
    }
}

