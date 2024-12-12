using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACrabAttackCollider : MonoBehaviour
{
    public ACrabController CrabContoller;

    [Tooltip("If true, particles will deal damage only once during the lifetime of the particle system.")]
    public bool DealDamageOnlyOnce = true;

    private bool hasDealtDamage = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !CrabContoller.bCrabDie)
        {
            //Debug.Log("Hit player");
            ATrinityController health = collision.gameObject.GetComponent<ATrinityController>();
            if (health != null)
            {
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                Vector3 direction = -collision.GetContact(0).normal;

                //direction.y = Mathf.Max(direction.y, 0.2f); // Add a slight vertical lift if needed
                //direction.Normalize();
                //Debug.Log(direction);

                // Apply knockback force
                Vector3 knockbackForce = direction * 150;
                rb.AddForce(knockbackForce, ForceMode.Impulse); 

                health.ApplyDamage(CrabContoller.GetCurrentAttackDamage());
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (DealDamageOnlyOnce && hasDealtDamage)
        {
            return;
        }

        ATrinityController health = other.GetComponent<ATrinityController>();
        if (health != null)
        {
            health.ApplyDamage(CrabContoller.GetParticleAttack());
            hasDealtDamage = true;
            //Debug.Log("Particles Hit player");
        }
    }

    public void GetCrabController(ACrabController controller)
    {
        CrabContoller = controller;
    }
}
