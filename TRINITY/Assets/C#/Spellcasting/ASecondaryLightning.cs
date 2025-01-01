using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASecondaryLightning : ASpell
{
    public int StacksApplied;
    public EAilmentType AilmentType;
    public AudioSource LightningSource;
    public GameObject ChargeVFXObj;
    public GameObject FullyChargedVFXObj;

    public float MaxScale;
    public float MaxChannelTime;
    public float ChannelTime;

    [Header("VFX Prefabs")]
    public GameObject ChargeVFX;
    public GameObject FullyChargedVFX;
    public override void Initialize()
    {
        LightningSource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        if (ChargeVFXObj == null) 
        {
            GameObject chargeVFX = Instantiate(ChargeVFX, SpellsReference.CastPoint.position, Quaternion.identity);
            chargeVFX.transform.parent = SpellsReference.CastPoint.transform;
            ChargeVFXObj = chargeVFX;
        }
        
    }

    public override void CastUpdate()
    {
        print("Channeling lightning bolt");
        ChannelTime += Time.deltaTime;

        foreach (Transform children in ChargeVFXObj.GetComponentInChildren<Transform>()) 
        {
            children.localScale = transform.localScale;
        }

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);
        transform.localScale = Vector3.one * Mathf.Lerp(0, MaxScale, t);
        if (ChannelTime >= MaxChannelTime && FullyChargedVFXObj == null) 
        {
            GameObject chargeVFX = Instantiate(FullyChargedVFX, SpellsReference.CastPoint.position, Quaternion.identity);
            chargeVFX.transform.parent = SpellsReference.CastPoint.transform;
            FullyChargedVFXObj = chargeVFX;
        }
    }

    public override void CastEnd()
    {
        //GameObject go = Instantiate(SpellPrefab.gameObject, SpellsReference.CastPoint.position, Quaternion.identity);
        //go.transform.parent = this.gameObject.transform;

        //LightningBolt lightningBolt = go.GetComponent<LightningBolt>();
        //lightningBolt.LightningSource = LightningSource;

        //go.GetComponent<LightningBolt>().Spells = SpellsReference;
        if (ChargeVFXObj != null) 
        {
            Destroy(ChargeVFXObj);
            ChargeVFXObj = null;
            transform.localScale = Vector3.zero;
        }
        if (FullyChargedVFXObj != null) 
        {
            Destroy(FullyChargedVFXObj);
            FullyChargedVFXObj = null;
        }
        ChannelTime = 0;
    }
}
