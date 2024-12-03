using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryFire : ASpell
{
    public override void Initialize()
    {
        Brain = transform.root.Find("Brain").GetComponent<ATrinityBrain>();
        Spells = transform.parent.GetComponent<ATrinitySpells>();
        
    }

    public override void CastStart()
    {
        GameObject go = Instantiate(SpellPrefab.gameObject, Spells.CastPos.position, Quaternion.identity);
        go.GetComponent<AFireball>().Spells = Spells;


    }

    public override void CastUpdate()
    {
        
    }

    public override void CastEnd()
    {
        
    }
}
