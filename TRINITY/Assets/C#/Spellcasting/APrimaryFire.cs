using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryFire : ASpell
{
    void Start()
    {
        Brain = transform.root.Find("Brain").GetComponent<ATrinityBrain>();
        Spells = transform.parent.GetComponent<ATrinitySpells>();
        
    }
    
    // Update is called once per frame
    void Update()
    {
        UpdateCooldown();
        
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
