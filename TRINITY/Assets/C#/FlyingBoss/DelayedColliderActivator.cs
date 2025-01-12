using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DelayedColliderActivator : MonoBehaviour
{
    [SerializeField] private Collider targetCollider;
    [SerializeField] private float delayTime = 2f;
    [SerializeField] private float damage;
    [SerializeField] private float devide = 1;
    [SerializeField] private float attackForce = 1;
    IEnemyController EnemyController;
    private float timer = 0f;
    private bool isTimerRunning = false;
    private bool hasDamaged = false;

    private void Start()
    {
        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider>();
        }

        if (targetCollider != null)
        {
            targetCollider.enabled = false;
            isTimerRunning = true;
        }
        else
        {
            Debug.LogError("No collider assigned or found on this GameObject.");
        }

        ElectricBoom eb = GetComponentInParent<ElectricBoom>();
        EnemyController = eb.Controller;
        damage = eb.Damage;
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;

            if (timer >= delayTime)
            {
                targetCollider.enabled = true;
                isTimerRunning = false;
            }
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (!collision.gameObject.CompareTag("Player"))
    //    {
    //        return;
    //    }
    //    Debug.Log("Hit player");
    //    ATrinityController player = collision.gameObject.GetComponent<ATrinityController>();

    //    if (player == null)
    //    {
    //        return;
    //    }
    //    Debug.Log("Hit player1");

    //    Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
    //    Vector3 direction = collision.impulse.normalized;

    //    Vector3 knockbackForce = new Vector3(direction.x, direction.y, direction.z) * attackForce;

    //    rb.AddForce(knockbackForce, ForceMode.Impulse);
    //    FHitInfo hitInfo = new FHitInfo(EnemyController.gameObject, this.gameObject, collision, damage / devide);
    //    player.ApplyHit(hitInfo);
    //}
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }
        if (!hasDamaged) { }
        ATrinityController player = other.gameObject.GetComponent<ATrinityController>();

        if (player == null)
        {
            return;
        }
        
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        Vector3 direction = (other.transform.position - transform.position).normalized;

        direction.y += attackForce * 0.1f; 
        direction = direction.normalized;

        Vector3 knockbackForce = direction * attackForce;
        rb.AddForce(knockbackForce, ForceMode.Impulse);

        FHitInfo hitInfo = new FHitInfo(EnemyController.gameObject, this.gameObject, null, damage / devide);
        player.ApplyHit(hitInfo);

    }
}
