using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DelayedColliderActivator : MonoBehaviour
{
    [SerializeField] private Collider targetCollider;
    [SerializeField] private float delayTime;
    [SerializeField] private float damage;
    [SerializeField] private float devide;
    [SerializeField] private float attackForce;
    IEnemyController EnemyController;
    private float timer = 0f;
    private bool isTimerRunning = false;
    private bool hasDamaged = false;
    public AudioSource audioS;
    public AudioClip SoundCLip;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
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
        if (eb != null)
        {
            EnemyController = eb.Controller;
            damage = eb.Damage;
        }

    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;

            if (timer >= delayTime)
            {
                targetCollider.enabled = true;
                MediumCameraShake(0.2f);
                isTimerRunning = false;
                if (audioS != null)
                {
                    audioS.PlayOneShot(SoundCLip);
                }
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
        if (!other.gameObject.CompareTag("Player") || hasDamaged)
        {
            return;
        }
        ATrinityController player = other.gameObject.GetComponent<ATrinityController>();

        if (player == null)
        {
            return;
        }
        hasDamaged = true;
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        Vector3 direction = (other.transform.position - transform.position).normalized;

        direction.y += attackForce * 0.1f;
        direction = direction.normalized;

        Vector3 knockbackForce = direction * attackForce;
        rb.AddForce(knockbackForce, ForceMode.Impulse);

        Debug.Log("ElectricBoom hit " + gameObject.name + ": " + damage / devide);
        FHitInfo hitInfo = new FHitInfo(EnemyController.gameObject, this.gameObject, null, damage / devide);
        player.ApplyHit(hitInfo);

    }

    public void MediumCameraShake(float duration = .5f)
    {
        //Debug.Log("IsShack"+this.gameObject.name);
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCameraFrom(0.6f, duration, transform);
    }
    public void GetControllerDamage(IEnemyController controller, float dmg)
    {
        EnemyController = controller;
        damage = dmg;
    }
}
