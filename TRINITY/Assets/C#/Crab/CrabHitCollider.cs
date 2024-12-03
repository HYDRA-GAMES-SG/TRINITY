using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabHitCollider : MonoBehaviour
{
    ACrabController CrabContoller;

    private void Start()
    {
        CrabContoller = GetComponentInParent<ACrabController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player");
            ATrinityController health = collision.gameObject.GetComponent<ATrinityController>();
            if (health != null)
            {
                //health.ApplyDamage(CrabContoller.GetCurrentAttackDamage());
            }
        }
    }

}
