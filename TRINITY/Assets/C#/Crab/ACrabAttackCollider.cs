using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACrabAttackCollider : MonoBehaviour
{
    ACrabController CrabContoller;

    private void Start()
    {
        CrabContoller = GetComponentInParent<ACrabController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player");
            ATrinityController health = collision.gameObject.GetComponent<ATrinityController>();
            if (health != null)
            {
                health.ApplyDamage(CrabContoller.GetCurrentAttackDamage());
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit player");
            ATrinityController health = other.gameObject.GetComponent<ATrinityController>();
            if (health != null)
            {
                health.ApplyDamage(CrabContoller.GetCurrentAttackDamage());
            }
        }
    }

}
