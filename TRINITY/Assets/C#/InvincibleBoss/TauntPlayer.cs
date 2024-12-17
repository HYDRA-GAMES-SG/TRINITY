using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauntPlayer : MonoBehaviour
{


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

        }
    }
}
