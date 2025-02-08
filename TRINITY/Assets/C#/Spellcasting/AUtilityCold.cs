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

    public AudioClip[] IceCrystal;
    
    [Header("VFX")]
    public GameObject SurroundingIce;

    private float FrozenTimer = 0f;

    public bool bActive = false;
    
    public override void Initialize()
    {
        ColdUtilitySource = GetComponent<AudioSource>();
        bActive = false;
        FrozenTimer = 0f;
    }

    public override void CastStart()
    {
        bActive = true;
        FrozenTimer = 0f;
        GameObject vfxSpike = Instantiate(SpellPrefab, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);
        GameObject vfxSurroundingIce = Instantiate(SurroundingIce, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);

        vfxSpike.transform.parent = ATrinityGameManager.GetPlayerController().transform;

        int rng = Random.Range(0,IceCrystal.Length);
        ColdUtilitySource.PlayOneShot(IceCrystal[rng]);

        Destroy(vfxSpike, 6);
        Destroy(vfxSurroundingIce, 6);

        ATrinityGameManager.GetPlayerController().HealthComponent.bInvulnerable = true;

        ATrinityGameManager.GetBrain().SetStunnedState(FrozenTime, true);
    }

    public override void CastUpdate()
    {
        if (bActive)
        {
            FrozenTimer += Time.deltaTime;
            ATrinityGameManager.GetPlayerController().HealthComponent
                .Modify((HealAmount / FrozenTime) * Time.deltaTime);

            if (FrozenTimer > FrozenTime)
            {
                //Release();
                ATrinityGameManager.GetPlayerController().HealthComponent.bInvulnerable = false;
                bActive = false;
                FrozenTimer = 0f;   
            }
        }
    }

    public override void CastEnd()
    {
    }
}
