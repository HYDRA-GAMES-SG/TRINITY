using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASpell : MonoBehaviour
{
    public GameObject SpellPrefab;
    protected USpellComponent SpellComponent;
    protected ATrinityBrain Brain;
    protected ATrinityCharacter TrinitySpells;

    public virtual void Start()
    {
        Brain = transform.parent.GetComponent<ATrinityBrain>();
        TrinitySpells = transform.parent.GetComponent<ATrinityCharacter>();
    }

    public virtual void Invoke()
    {
        //initializes a prefab
    }

    public virtual void CastStart()
    {
    }

    public virtual void CastUpdate()
    {
        
    }

    public virtual void CastEnd()
    {
        
    }
}