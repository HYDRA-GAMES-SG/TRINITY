using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUtilityCold : ASpell
{
    public AudioSource ColdUtilitySource;
    public ATrinitySpells TrinitySpells;

    public float HealAmount;
    public float FrozenTime;
    public float LiftForce;
    public int StacksApplied;
    public EAilmentType AilmentType;
    
    [Header("VFX")]
    public GameObject SurroundingIce;
    public GameObject IceSpike;
    public override void Initialize()
    {
        ColdUtilitySource = GetComponent<AudioSource>();
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
        GameObject vfxSpike = Instantiate(IceSpike, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);
        GameObject vfxSurroundingIce = Instantiate(SurroundingIce, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);

        vfxSpike.transform.parent = ATrinityGameManager.GetPlayerController().transform;

        ATrinityGameManager.GetPlayerController().HealthComponent.Modify(HealAmount);

        ATrinityGameManager.GetBrain().SetStunnedState(FrozenTime, true);
    }
}
