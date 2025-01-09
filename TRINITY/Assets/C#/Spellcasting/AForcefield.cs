using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AForcefield : ASpell
{
    public float DamageAbsorbedPerMana = 1f;
    private GameObject ForcefieldEffect;
    
    public override void Initialize()
    {
        if (ForcefieldEffect == null)
        {
            ForcefieldEffect = Instantiate(SpellPrefab, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);
            ForcefieldEffect.transform.parent = ATrinityGameManager.GetPlayerController().transform;
            ForcefieldEffect.SetActive(false);
        }
        ATrinityGameManager.GetSpells().ManaComponent.OnOutOfMana += Release;
    }
    
    public override void CastStart()
    {
        if (ATrinityGameManager.GetSpells().ManaComponent.Current <= 0)
        {
            return;
        }

        ATrinityGameManager.GetBrain().bForcefieldActive = true;
        ForcefieldEffect.SetActive(true);
    }
    
    public override void CastUpdate()
    {
        
    }
    
    public override void CastEnd()
    {
        ATrinityGameManager.GetBrain().bForcefieldActive = false;
        ForcefieldEffect.SetActive(false);
    }
}
