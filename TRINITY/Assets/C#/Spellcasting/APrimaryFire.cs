using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryFire : ASpell
{
    
    public int StacksApplied;
    public EAilmentType AilmentType;
    public AudioSource FireSource;
    public AudioClip[] FireAttack;
    public override void Initialize()
    {
        FireSource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        GameObject go = Instantiate(SpellPrefab.gameObject, SpellsReference.CastPoint.position, Quaternion.identity);
        go.transform.parent = this.gameObject.transform;
        
        AFireball fireball = go.GetComponent<AFireball>();
        fireball.FireSource = FireSource;
        
        go.GetComponent<AFireball>().Spells = SpellsReference;
        int i = Random.Range(0, FireAttack.Length - 1);
        FireSource.PlayOneShot(FireAttack[i]);

    }

    public override void CastUpdate()
    {
        
    }

    public override void CastEnd()
    {
        
    }
}
