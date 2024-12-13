using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UtilityCold : ASpell
{
    public AudioSource ColdUtilitySource;
    public ATrinitySpells TrinitySpells;
    public override void Initialize()
    {
        ColdUtilitySource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
       //Frozen pulse that launches you into the air & chills enemies AOE
    }

    public override void CastUpdate()
    {

    }

    public override void CastEnd()
    {

    }
}
