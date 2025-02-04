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

    private float FrozenTimer = 0f;
    public override void Initialize()
    {
        ColdUtilitySource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        FrozenTimer = 0f;
        GameObject vfxSpike = Instantiate(SpellPrefab, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);
        GameObject vfxSurroundingIce = Instantiate(SurroundingIce, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);

        vfxSpike.transform.parent = ATrinityGameManager.GetPlayerController().transform;

        ATrinityGameManager.GetPlayerController().HealthComponent.Modify(HealAmount);

        ATrinityGameManager.GetBrain().SetStunnedState(FrozenTime, true);
    }

    public override void CastUpdate()
    {
        FrozenTimer += Time.deltaTime;
        if (FrozenTimer > FrozenTime)
        {
            Release();
        }
    }

    public override void CastEnd()
    {
        
    }
}
