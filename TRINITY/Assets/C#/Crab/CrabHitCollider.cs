using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabHitCollider : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "PLayer")
        {
            Debug.Log("Hit player");
            UHealthComponent health = collision.gameObject.GetComponent<UHealthComponent>();
            if (health != null)
            {
                health.Modify(-10);
            }
        }
    }
}
