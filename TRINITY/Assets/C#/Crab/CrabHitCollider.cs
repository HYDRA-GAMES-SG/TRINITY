using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabHitCollider : MonoBehaviour
{
    private Vector3 GroundContactPoint;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player");
            UHealthComponent health = collision.gameObject.GetComponent<UHealthComponent>();
            if (health != null)
            {
                health.Modify(-10);
            }
        }
        if (collision.gameObject.name == "Plane")
        {
            GroundContactPoint = collision.contacts[0].point;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GroundContactPoint, 0.2f);
    }
}
