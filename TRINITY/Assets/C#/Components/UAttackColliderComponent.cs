using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAttackColliderComponent : MonoBehaviour
{
    //public static System.Action OnPlayerHit;
    //public static System.Action<float> OnGroundHit;

    private IEnemyController Controller;

    [Tooltip("If true, particles will deal damage only once during the lifetime of the particle system.")]
    public bool DealDamageOnlyOnce = true;

    private bool hasDealtDamage = false;

    private void Start()
    {
        IEnemyController enemyController;
        if ((enemyController = transform.root.GetComponentInChildren<IEnemyController>()) != null)
        {
            Controller = enemyController;
            return;
        }
        //var controllerTransform = transform.root.Find("Controller");
        //if (controllerTransform != null && controllerTransform.TryGetComponent<IEnemyController>(out enemyController))
        //{
        //    Debug.Log("B");
        //    Controller = enemyController;
        //    //Debug.Log("IEnemyController found on the 'Controller' child object.");
        //}

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Controller.bDead)
        {
            Debug.Log("Attack Collider : Enemy dead.");
            return;
        }

        //if(collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        //{
        //    OnGroundHit?.Invoke(collision.GetContact(0).impulse.magnitude);
        //    return;
        //}


        //Debug.Log("Hit player");
        ATrinityController player = collision.gameObject.GetComponent<ATrinityController>();

        if (player == null)
        {
            Debug.Log("Attack Collider: Not a Player.");
            return;
        }

        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        Vector3 direction = (collision.transform.position - Controller.transform.position).normalized;
        direction.y = 0; // Ignore Y-axis for horizontal knockback
        if (direction.z < 0.1f) direction.z = 0.1f;
        // Add upward force for an arc effect
        Vector3 upwardForce = Vector3.up * 0f; // Adjust multiplier as needed

        // Combine horizontal and upward forces
        Vector3 knockbackForce = (direction + upwardForce).normalized * Controller.AttackForce;
        rb.AddForce(knockbackForce, ForceMode.Impulse);
        //Debug.Log($"Knockback Applied! Direction: {direction}, Force: {knockbackForce}");


        FHitInfo hitInfo = new FHitInfo(Controller.gameObject, this.gameObject, collision, Controller.GetCurrentAttackDamage());
        player.ApplyHit(hitInfo);
        //OnPlayerHit?.Invoke();

    }

    private void OnParticleCollision(GameObject other)//only use for Ice spray
    {
        //Debug.Log($"Particles Hit player ");
        if (DealDamageOnlyOnce && hasDealtDamage)
        {
            return;
        }

        ATrinityController player = other.GetComponent<ATrinityController>();

        if (player == null)
        {
            return;
        }

        FHitInfo hitInfo = new FHitInfo(Controller.gameObject, this.gameObject, null, Controller.GetParticleAttack());  
        player.ApplyHit(hitInfo);
        hasDealtDamage = true;
        Debug.Log($"Particles Hit player " + Controller.GetParticleAttack());
    }

    public void SetController(IEnemyController enemyController)
    {
        Controller = enemyController;
    }
}
