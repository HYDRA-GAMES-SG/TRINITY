using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAttackColliderComponent : MonoBehaviour
{
    private IEnemyController Controller;

    [Tooltip("If true, particles will deal damage only once during the lifetime of the particle system.")]
    public bool DealDamageOnlyOnce = true;

    private bool hasDealtDamage = false;

    private void Start()
    {
        // Get the root object of the current GameObject
        var rootTransform = transform.root;

        // Check if the root itself has IEnemyController
        if (rootTransform.TryGetComponent<IEnemyController>(out var enemyController))
        {
            Controller = enemyController;
            //Debug.Log("IEnemyController found on the root object.");
            return;
        }

        // Check if the root has a child named "Controller" with IEnemyController
        var controllerTransform = rootTransform.Find("Controller");
        if (controllerTransform != null && controllerTransform.TryGetComponent<IEnemyController>(out enemyController))
        {
            Controller = enemyController;
            //Debug.Log("IEnemyController found on the 'Controller' child object.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") && Controller.bDead)
        {
            Debug.Log("Attack Collider : Enemy dead or did not collide with player.");
            return;
        }

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


    }

    private void OnParticleCollision(GameObject other)
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
        //Debug.Log($"Particles Hit player " + Controller.GetParticleAttack());
    }

    public void SetController(IEnemyController enemyController)
    {
        Controller = enemyController;
    }
}
