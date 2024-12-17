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
            ForcefieldEffect = Instantiate(SpellPrefab, Controller.Position, Quaternion.identity);
            ForcefieldEffect.transform.parent = Controller.transform;
            ForcefieldEffect.SetActive(false);
        }
        SpellsReference.ManaComponent.OnOutOfMana += Release;
    }
    
    public override void CastStart()
    {
        if (SpellsReference.ManaComponent.Current <= 0)
        {
            return;
        }

        BrainReference.bForcefieldActive = true;
        ForcefieldEffect.SetActive(true);
    }
    
    public override void CastUpdate()
    {
        
    }
    
    public override void CastEnd()
    {
        BrainReference.bForcefieldActive = false;
        ForcefieldEffect.SetActive(false);
    }
}
