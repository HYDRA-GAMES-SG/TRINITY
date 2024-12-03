using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AForcefield : ASpell
{
    // public override void Initialize()
    // {
    //     if (Beam == null)
    //     {
    //         Beam = Instantiate(SpellPrefab, Spells.CastPos.position, Quaternion.identity);
    //         Beam.transform.parent = Spells.CastPos.transform;
    //     }
    // }
    //
    // public override void CastStart()
    // {
    //     Beam.SetActive(true);
    // }
    //
    // public override void CastUpdate()
    // {
    //     Quaternion newRot = Spells.CameraRef.transform.rotation * Quaternion.Euler(0f, 80f, 0f);
    //     Beam.transform.rotation = newRot;
    //
    // }
    //
    // public override void CastEnd()
    // {
    //     Brain.ChangeAction(ETrinityAction.ETA_None);
    //     Beam.SetActive(false);
    //     
    //
    // }
}
