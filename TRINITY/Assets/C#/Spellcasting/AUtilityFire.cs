using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class AUtilityFire : ASpell
{
    public AudioSource FireUtilitySource;

    public APrimaryFire FireballSpell;
    public ATrinityController TrinityController;
    public ATrinityBrain TrinityBrain;
    public ATrinitySpells TrinitySpells;
    public Transform SpawnPos;
    private ETrinityAction Cleanse;

    public float ManaCost;
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

    }

    public override void CastEnd()
    {

    }

    public void Update()
    {
       AuraTimer -= Time.deltaTime;
        if (AuraTimer < 0) 
        {
            FireballSpell.Cooldown = OriginalCooldown;
            bAura = false;
        }
    }
    public void FlameAura() 
    {
        UManaComponent playerMana = TrinitySpells.GetComponent<UManaComponent>();
        if (playerMana.Current < ManaCost || bAura) 
        {
            return;
        }
        //Deduct Mana
        playerMana.Modify(-ManaCost);
        print(playerMana.Current);

        //Spawn VFX
        GameObject explosionVFX = Instantiate(ExplosionVFX, SpawnPos.position, Quaternion.identity);
        GameObject auraVFX = Instantiate (AuraVFX, SpawnPos.position,Quaternion.identity);
        auraVFX.transform.parent = TrinityController.transform;
        Destroy(auraVFX, AuraTime);

        //Get Trinity Brain on player and change action to cleanse
        TrinityBrain.ModifyState(Cleanse);
        bAura = true;

        //Apply heal, set aura timer where Primary attacks are faster, if aura is on and used with other primary, apply ignite as well
        UHealthComponent playerHealth = TrinityController.gameObject.GetComponent<UHealthComponent>();
        playerHealth.Modify(HealAmount);
        FireballSpell.Cooldown = LoweredCooldown;
        AuraTimer = AuraTime;
    }
}
