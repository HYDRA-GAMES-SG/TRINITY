using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AForcefield : ASpell
{
    public float DamageAbsorbedPerMana = 1f;
    private GameObject ForcefieldEffect;
    static public System.Action<bool> ForcefieldStateChanged;
    
    public override void Initialize()
    {
        if (ForcefieldEffect == null)
        {
            ForcefieldEffect = Instantiate(SpellPrefab, Controller.Position, Quaternion.identity);
            ForcefieldEffect.transform.parent = Controller.transform;
            ForcefieldEffect.SetActive(false);
        }
    }
    
    public override void CastStart()
    {
        if (Spells.ManaComponent.Current <= 0)
        {
            return;
        }

        ForcefieldStateChanged?.Invoke(true);
        ForcefieldEffect.SetActive(true);
    }
    
    public override void CastUpdate()
    {
        if (Spells.ManaComponent.Current <= 0)
        {
            CastEnd();
        }
    }
    
    public override void CastEnd()
    {
        ForcefieldStateChanged?.Invoke(false);
        ForcefieldEffect.SetActive(false);
    }
}
