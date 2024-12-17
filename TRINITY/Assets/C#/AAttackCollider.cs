using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAttackCollider : MonoBehaviour
{
    private IEnemyController Controller;

    [Tooltip("If true, particles will deal damage only once during the lifetime of the particle system.")]
    public bool DealDamageOnlyOnce = true;

    private bool hasDealtDamage = false;

    private void Start()
    {
        Controller = transform.root.Find("Controller").GetComponent<IEnemyController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") && Controller.bDead)
        {
            Debug.Log("Crab Collision: Crab is dead or did not collide with player.");
            return;
        }
        
        //Debug.Log("Hit player");
        ATrinityController player = collision.gameObject.GetComponent<ATrinityController>();

        if (player == null)
        {
            Debug.Log("Crab Collision: Player controller is null on player tagged object.");
            return;
        }
        
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        Vector3 direction = collision.impulse.normalized;

        //direction.y = Mathf.Max(direction.y, 0.2f); // Add a slight vertical lift if needed
        //direction.Normalize();
        //Debug.Log(direction);

        // Apply knockback force
        
        Vector3 knockbackForce = new Vector3(direction.x, direction.y, direction.z) * Controller.AttackForce;
        
        
        
        rb.AddForce(knockbackForce, ForceMode.Impulse);
        
        FHitInfo hitInfo = new FHitInfo(Controller.gameObject, collision, Controller.GetCurrentAttackDamage());
        player.ApplyHit(hitInfo);
            
        
    }

    private void OnParticleCollision(GameObject other)
    {
        if (DealDamageOnlyOnce && hasDealtDamage)
        {
            return;
        }

        ATrinityController player = other.GetComponent<ATrinityController>();

        if (player == null)
        {
            return;
        }
        
        FHitInfo hitInfo = new FHitInfo(Controller.gameObject, null, Controller.GetParticleAttack());
        hasDealtDamage = true;
        //Debug.Log("Particles Hit player");
    }

    public void GetCrabController(ACrabController controller)
    {
        Controller = controller;
    }
}
