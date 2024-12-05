using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryFire : ASpell
{
    public override void Initialize()
    {
        
    }

    public override void CastStart()
    {
        GameObject go = Instantiate(SpellPrefab.gameObject, Spells.CastPoint.position, Quaternion.identity);
        go.GetComponent<AFireball>().Spells = Spells;


    }

    public override void CastUpdate()
    {
        
    }

    public override void CastEnd()
    {
        
    }
}
