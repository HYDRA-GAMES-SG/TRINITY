using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUtilityFire : ASpell
{
    public AudioSource FireUtilitySource;

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
        OriginalCooldown = ATrinityGameManager.GetSpells().PrimaryFire.Cooldown;
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
            ATrinityGameManager.GetSpells().PrimaryFire.Cooldown = OriginalCooldown;
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
        GameObject explosionVFX = Instantiate(ExplosionVFX, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);
        GameObject auraVFX = Instantiate (AuraVFX, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);
        auraVFX.transform.parent = ATrinityGameManager.GetPlayerController().transform;
        Destroy(auraVFX, AuraTime);

        //Get Trinity Brain on player and change action to cleanse
        ATrinityGameManager.GetBrain().ModifyState(Cleanse);
        bAura = true;

        //Apply heal, set aura timer where Primary attacks are faster, if aura is on and used with other primary, apply ignite as well
        ATrinityGameManager.GetPlayerController().HealthComponent.Modify(HealAmount);
        ATrinityGameManager.GetSpells().PrimaryFire.Cooldown = LoweredCooldown;
        AuraTimer = AuraTime;
    }
}
