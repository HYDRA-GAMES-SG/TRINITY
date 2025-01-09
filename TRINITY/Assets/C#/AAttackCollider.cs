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
        // Get the root object of the current GameObject
        var rootTransform = transform.root;

        // Check if the root itself has IEnemyController
        if (rootTransform.TryGetComponent<IEnemyController>(out var enemyController))
        {
            Controller = enemyController;
            Debug.Log("IEnemyController found on the root object.");
            return;
        }

        // Check if the root has a child named "Controller" with IEnemyController
        var controllerTransform = rootTransform.Find("Controller");
        if (controllerTransform != null && controllerTransform.TryGetComponent<IEnemyController>(out enemyController))
        {
            Controller = enemyController;
            Debug.Log("IEnemyController found on the 'Controller' child object.");
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
        Vector3 direction = collision.impulse.normalized;

        //direction.y = Mathf.Max(direction.y, 0.2f); // Add a slight vertical lift if needed
        //direction.Normalize();
        //Debug.Log(direction);

        // Apply knockback force

        Vector3 knockbackForce = new Vector3(direction.x, direction.y, direction.z) * Controller.AttackForce;



        rb.AddForce(knockbackForce, ForceMode.Impulse);

        FHitInfo hitInfo = new FHitInfo(Controller.gameObject, this.gameObject, collision, Controller.GetCurrentAttackDamage());
        player.ApplyHit(hitInfo);


    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log($"Particles Hit player ");
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
