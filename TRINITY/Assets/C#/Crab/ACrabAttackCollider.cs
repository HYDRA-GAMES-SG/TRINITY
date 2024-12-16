using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACrabAttackCollider : MonoBehaviour
{
    public ACrabController CrabController;

    [Tooltip("If true, particles will deal damage only once during the lifetime of the particle system.")]
    public bool DealDamageOnlyOnce = true;

    private bool hasDealtDamage = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !CrabController.bCrabDie)
        {
            //Debug.Log("Hit player");
            ATrinityController player = collision.gameObject.GetComponent<ATrinityController>();
            if (player != null)
            {
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                Vector3 direction = -collision.GetContact(0).normal;

                //direction.y = Mathf.Max(direction.y, 0.2f); // Add a slight vertical lift if needed
                //direction.Normalize();
                //Debug.Log(direction);

                // Apply knockback force
                Vector3 knockbackForce = direction * 150;
                rb.AddForce(knockbackForce, ForceMode.Impulse); 
                
                FHitInformation hitInfo = new FHitInformation(CrabController.gameObject, collision, CrabController.GetCurrentAttackDamage());
                player.ApplyHit(hitInfo);
                
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (DealDamageOnlyOnce && hasDealtDamage)
        {
            return;
        }

        ATrinityController player = other.GetComponent<ATrinityController>();
        if (player != null)
        {
            FHitInformation hitInfo = new FHitInformation(CrabController.gameObject, null, CrabController.GetParticleAttack());
            hasDealtDamage = true;
            //Debug.Log("Particles Hit player");
        }
    }

    public void GetCrabController(ACrabController controller)
    {
        CrabController = controller;
    }
}
