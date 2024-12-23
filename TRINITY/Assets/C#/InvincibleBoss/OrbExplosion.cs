using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbExplosion : MonoBehaviour
{
    private AInvincibleBossController Controller;

    [Tooltip("If true, particles will deal damage only once during the lifetime of the particle system.")]
    public bool DealDamageOnlyOnce = true;

    private bool hasDealtDamage = false;

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log($"Particles Hit player " );
        if (DealDamageOnlyOnce && hasDealtDamage)
        {
            return;
        }

        ATrinityController player = other.GetComponent<ATrinityController>();

        if (player == null)
        {
            return;
        }

        FHitInfo hitInfo = new FHitInfo(Controller.gameObject, this.gameObject, null, Controller.OrbExplosionDMG);
        player.ApplyHit(hitInfo);
        hasDealtDamage = true;
        Debug.Log($"Particles Hit player " + Controller.OrbExplosionDMG);
    }
    public void SetController(AInvincibleBossController enemyController)
    {
        Controller = enemyController;
    }
}
