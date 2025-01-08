using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class AUtilityFire : ASpell
{
    public AudioSource FireUtilitySource;

    public APrimaryFire FireballSpell;
    public Transform SpawnPos;
    private ETrinityAction Cleanse;

    public float HealAmount;
    private float OriginalCooldown;
    public float LoweredCooldown;
    public float AuraTimer;
    public float AuraTime;
    public bool bAura;

    [Header("VFX")]
    public GameObject AuraVFX;
    public GameObject ExplosionVFX;
    public override void Initialize()
    {
        Cleanse = ETrinityAction.ETA_None;
        OriginalCooldown = FireballSpell.Cooldown;
        FireUtilitySource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        FlameAura();     
    }

    public override void CastUpdate()
    {
        AuraTimer -= Time.deltaTime;
        if (AuraTimer < 0)
        {
            FireballSpell.Cooldown = OriginalCooldown;
            bAura = false;
            print("CleanseFlame cast ended");
        }
    }

    public override void CastEnd()
    {
        print("Spell ended");
    }
    public void FlameAura() 
    {
        //Spawn VFX
        GameObject explosionVFX = Instantiate(ExplosionVFX, SpawnPos.position, Quaternion.identity);
        GameObject auraVFX = Instantiate (AuraVFX, SpawnPos.position,Quaternion.identity);
        auraVFX.transform.parent = Controller.transform;
        Destroy(auraVFX, AuraTime);

        //Get Trinity Brain on player and change action to cleanse
        BrainReference.ModifyState(Cleanse);
        bAura = true;

        //Apply heal, set aura timer where Primary attacks are faster, if aura is on and used with other primary, apply ignite as well
        UHealthComponent playerHealth = Controller.gameObject.GetComponent<UHealthComponent>();
        playerHealth.Modify(HealAmount);
        FireballSpell.Cooldown = LoweredCooldown;
        AuraTimer = AuraTime;
    }
}
