using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class AUtilityFire : ASpell
{
    public AudioSource FireUtilitySource;
    public ATrinityBrain TrinityBrain;
    public ATrinitySpells TrinitySpells;
    private ETrinityAction Cleanse;
    public override void Initialize()
    {
        Cleanse = ETrinityAction.ETA_None;
    }

    public override void CastStart()
    {
        FireUtilitySource = GetComponent<AudioSource>();
        //Get Trinity Brain on player and change action to cleanse
        //Apply MS & heal
        UManaComponent playerMana = TrinitySpells.GetComponent<UManaComponent>();
        playerMana.Modify(-20);
        print(playerMana.Current);
    }

    public override void CastUpdate()
    {

    }

    public override void CastEnd()
    {

    }
}
