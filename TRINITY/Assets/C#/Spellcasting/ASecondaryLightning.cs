using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class ASecondaryLightning : ASpell
{
    public int StacksApplied;
    public EAilmentType AilmentType;
    public AudioSource LightningSource;
    public GameObject ChargeVFXObj;
    public GameObject FullyChargedVFXObj;

    public float MinScale;
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
            GameObject chargeVFX = Instantiate(ChargeVFX, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            chargeVFX.transform.parent = ATrinityGameManager.GetSpells().CastPoint.transform;
            ChargeVFXObj = chargeVFX;
        }
        
    }

    public override void CastUpdate()
    {
        print("Channeling lightning bolt");
        ChannelTime += Time.deltaTime;

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);

        foreach (Transform children in ChargeVFXObj.GetComponentInChildren<Transform>()) 
        {
            children.localScale = Vector3.one * Mathf.Lerp(MinScale, MaxScale, t);
        }


        if (ChannelTime >= MaxChannelTime && FullyChargedVFXObj == null) 
        {
            GameObject chargeVFX = Instantiate(FullyChargedVFX, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            chargeVFX.transform.parent = ATrinityGameManager.GetSpells().CastPoint.transform;
            FullyChargedVFXObj = chargeVFX;
        }
    }

    public override void CastEnd()
    {
        if (ChargeVFXObj != null) 
        {
            float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);

            //Vector3 castPoint = Controller.transform.position + Vector3.up * Controller.Height + Controller.Forward * 1.5f;   
            
            LightningBolt lightningBolt = Instantiate(SpellPrefab.gameObject, ATrinityGameManager.GetSpells().CastPoint.position, SpellRot).GetComponent<LightningBolt>();
            //lightningBolt.gameObject.transform.parent = this.transform;
            lightningBolt.transform.localScale = Vector3.one * Mathf.Lerp(MinScale, MaxScale, t);

            Destroy(lightningBolt.gameObject, 0.5f);

            Destroy(ChargeVFXObj);
            ChargeVFXObj = null;         
        }
        if (FullyChargedVFXObj != null) 
        {
            Destroy(FullyChargedVFXObj);
            FullyChargedVFXObj = null;
        }
        ChannelTime = 0;
    }
}
