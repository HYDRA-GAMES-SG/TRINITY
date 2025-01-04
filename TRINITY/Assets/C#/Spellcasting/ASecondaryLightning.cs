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

    [HideInInspector]
    public Quaternion SpellRot = new Quaternion();

    [Header("VFX Prefabs")]
    public GameObject ChargeVFX;
    public GameObject FullyChargedVFX;
    public override void Initialize()
    {
        LightningSource = GetComponent<AudioSource>();
        SpellRot = Quaternion.Euler(-90, 0, 0);
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
        if (ChargeVFXObj != null) 
        {
            Vector3 castPoint = Controller.transform.position + Vector3.up * Controller.Height + Controller.Forward * 1.5f;
            GameObject go = Instantiate(SpellPrefab.gameObject, castPoint, SpellRot);

            LightningBolt lightningBolt = go.GetComponent<LightningBolt>();
            lightningBolt.LightningSource = LightningSource;
            lightningBolt.Spells = SpellsReference;

            go.GetComponent<LightningBolt>().Spells = SpellsReference;

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
