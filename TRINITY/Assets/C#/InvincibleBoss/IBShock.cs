using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class IBShock : MonoBehaviour
{
    [SerializeField] LayerMask CollideMask;
    [SerializeField] CapsuleCollider Collider;
    [SerializeField] Rigidbody RB;
    AInvincibleBossController IBController;
    float Damage;

    void Start()
    {
        Collider = GetComponent<CapsuleCollider>();
        RB = GetComponent<Rigidbody>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if ((CollideMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            Debug.Log(collision.gameObject.name);
            Collider.enabled = false;
            ATrinityController player = collision.gameObject.GetComponent<ATrinityController>();
            if (player != null)
            {
                FHitInfo hitInfo = new FHitInfo(IBController.gameObject, this.gameObject, collision, Damage);
                player.ApplyHit(hitInfo);
                //Debug.Log($"Particles Hit player " + IBController.GetCurrentAttackDamage());

            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if ((CollideMask.value & (1 << other.gameObject.layer)) != 0)
        {
            RB.velocity = Vector3.zero;
            //Debug.Log(other.gameObject.name);
            Collider.enabled = false;
            ATrinityController player = other.GetComponent<ATrinityController>();
            if (player != null)
            {
                FHitInfo hitInfo = new FHitInfo(IBController.gameObject, this.gameObject, null, Damage);
                player.ApplyHit(hitInfo);
                //Debug.Log($"Particles Hit player " + IBController.GetCurrentAttackDamage());
            }
            Destroy(gameObject, 0.55f);
        }
    }

    public void GetController(AInvincibleBossController controller)
    {
        IBController = controller;
        Damage = controller.GetCurrentAttackDamage();
    }
}
