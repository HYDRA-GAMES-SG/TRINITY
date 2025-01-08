using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Shock : MonoBehaviour
{
    [SerializeField] LayerMask CollideMask;
    [SerializeField] CapsuleCollider Collider;

    AInvincibleBossController IBController;
    float Damage;

    void Start()
    {
        Collider = GetComponent<CapsuleCollider>();
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
                Debug.Log($"Particles Hit player " + IBController.GetCurrentAttackDamage());

            }
        }
    }
    
    public void GetController(AInvincibleBossController controller)
    {
        IBController = controller;
        Damage = controller.GetCurrentAttackDamage();
    }
}
