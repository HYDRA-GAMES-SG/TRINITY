using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryCold : ASpell
{
    Rigidbody Rigidbody;
    public Quaternion SpellRot = new Quaternion();
    private int CastNumber;
    
    public int StacksApplied;
    public EAilmentType AilmentType;
    public AUtilityFire UtilityFire;
    public AudioSource ColdSource;
    public AudioClip ColdAttack, ColdSustain, ColdRelease;
    [Header("VFX Prefabs")]
    public GameObject CastVFX;
    // Start is called before the first frame update
    public override void Initialize()
    {
        ColdSource = GetComponent<AudioSource>();
        SpellRot = Quaternion.Euler(0,180,45);
    }
    public override void CastStart()
    {
        Instantiate(CastVFX, SpellsReference.CastPoint.position, CastVFX.transform.rotation);
        GameObject go = Instantiate(SpellPrefab.gameObject, SpellsReference.CastPoint.position, SpellRot);
        CastNumber++;
        if (CastNumber % 2 != 0)
        {
            SpellRot = Quaternion.Euler(0, 180, -45);
        }
        else
        {
            SpellRot = Quaternion.Euler(0, 180, 45);
        }
        go.transform.parent = this.gameObject.transform; 
        
        AIceWave iceWave = go.GetComponent<AIceWave>();
        iceWave.ColdSource = ColdSource;

        go.GetComponent<AIceWave>().Spells = SpellsReference;
    }

    public override void CastUpdate()
    {
       
    }

    public override void CastEnd()
    {
      
    }
}
