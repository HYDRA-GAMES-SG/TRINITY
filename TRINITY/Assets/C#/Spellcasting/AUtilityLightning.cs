using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUtilityLightning : ASpell
{
    public AudioSource UtilityLightningSource;
    public ATrinitySpells TrinitySpells;
    public override void Initialize()
    {
        UtilityLightningSource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
       //Speed boost that you can increase its duration by hitting up to a max
    }

    public override void CastUpdate()
    {

    }

    public override void CastEnd()
    {

    }
}
