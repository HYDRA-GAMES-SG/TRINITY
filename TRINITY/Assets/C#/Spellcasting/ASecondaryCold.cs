using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASecondaryCold : ASpell
{
    [Header("Audio")]
    public AudioClip CastingSFX;

    [Header("VFX Prefabs")]
    public GameObject CastingVFX;
    public override void Initialize()
    {
        
    }
    public override void CastStart()
    {
        //ATrinityController playerController = ATrinityGameManager.GetPlayerController();
        //Vector3 castPoint = playerController.transform.position + Vector3.up * playerController.Height + playerController.Forward * 1.5f;
        //GameObject iceNado = Instantiate(SpellPrefab, castPoint, Quaternion.identity);
    }
    public override void CastUpdate()
    {
        
    }
    public override void CastEnd()
    {
        
    }
}
