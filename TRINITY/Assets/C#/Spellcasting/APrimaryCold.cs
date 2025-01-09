using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryCold : ASpell
{
    [Header("Spell Properties")]
    public int StacksOfChillApplied;
    //public float ConvergenceRange = 5;
    
    [HideInInspector]
    public Quaternion SpellRot = new Quaternion();
    
    [HideInInspector]
    public int CastNumber;

    [Header("Audio")] 
    public AudioClip CastingSFX;
    
    [Header("VFX Prefabs")]
    public GameObject CastingVFX;
    
    // Start is called before the first frame update
    public override void Initialize()
    {
        SpellRot = Quaternion.Euler(0,180,45);
    }
    
    public override void CastStart()
    {

        GameObject castFX = Instantiate(CastingVFX, ATrinityGameManager.GetSpells().CastPoint.transform.position, Quaternion.identity);
        castFX.GetComponent<FollowCastPoint>().CastPoint = ATrinityGameManager.GetSpells().CastPoint;

        ATrinityController playerController = ATrinityGameManager.GetPlayerController();
        
        Vector3 castPoint = playerController.transform.position + Vector3.up * playerController.Height + playerController.Forward * 1.5f;
        GameObject go = Instantiate(SpellPrefab, castPoint, SpellRot);
        CastNumber++;
        go.transform.parent = this.gameObject.transform; 
        
        IceWave iceWave = go.GetComponent<IceWave>();

        iceWave.Controller = playerController;
        
        Release();
    }

    public override void CastUpdate()
    {

    }

    public override void CastEnd()
    {
      
    }
}
