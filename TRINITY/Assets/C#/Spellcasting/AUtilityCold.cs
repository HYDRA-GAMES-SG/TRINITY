using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUtilityCold : ASpell
{
    public AudioSource ColdUtilitySource;
    public ATrinitySpells TrinitySpells;
    public Transform SpawnPos;

    public float HealAmount;
    public float FrozenTime;
    public float LiftForce;
    public int StacksApplied;
    public EAilmentType AilmentType;
   
    Rigidbody TrinityRB;

    [Header("VFX")]
    public GameObject SurroundingIce;
    public GameObject IceSpike;
    public override void Initialize()
    {
        ColdUtilitySource = GetComponent<AudioSource>();
        TrinityRB = Controller.RB;      
    }

    public override void CastStart()
    {
        //Frozen pulse that launches you into the air & chills enemies AOE
        IceHill();
    }

    public override void CastUpdate()
    {
        
    }

    public override void CastEnd()
    {
        print("Spell ended");
    }

    public void IceHill() 
    {
        GameObject vfxSpike = Instantiate(IceSpike, SpawnPos.position, Quaternion.identity);
        GameObject vfxSurroundingIce = Instantiate(SurroundingIce, SpawnPos.position, Quaternion.identity);

        vfxSpike.transform.parent = Controller.transform;

        UHealthComponent playerHealth = Controller.gameObject.GetComponent<UHealthComponent>();
        playerHealth.Modify(HealAmount);

        BrainReference.SetStunnedState(FrozenTime, BrainReference.bIsStunned);
    }
}
