using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APrimaryLightning : ASpell
{
    public int StacksApplied;
    public EAilmentType AilmentType;

    private AudioSource LightningSource;

    public float MinScale;
    public float MaxScale;
    public float MaxChannelTime;
    public float ChannelTime;
    public float BoltCreationThreshold;
    [HideInInspector]
    public Quaternion SpellRot = new Quaternion();

    [Header("VFX Prefabs")]
    public GameObject ChargeVFX;
    public GameObject FullyChargedVFX;

    private GameObject ChargeVFXObj;
    private GameObject FullyChargedVFXObj;

    [Header("SFX")]
    public AudioClip ChargeSFX;
    public AudioClip ReleaseSFX;
    public AudioClip SustainSFX;
    public AudioClip CompletionSFX;

    public override void Initialize()
    {
        LightningSource = GetComponent<AudioSource>();
        SpellRot = Quaternion.Euler(-90, 0, 0);
        LightningSource.clip = ChargeSFX;
    }

    public override void CastStart()
    {
        ChannelTime = 0f;
        if (ChargeVFXObj == null)
        {
            GameObject chargeVFX = Instantiate(ChargeVFX, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            chargeVFX.transform.parent = ATrinityGameManager.GetSpells().CastPoint.transform;
            ChargeVFXObj = chargeVFX;
            LightningSource.clip = ChargeSFX;
            LightningSource.Play();
        }

    }

    public override void CastUpdate()
    {
        ChannelTime += Time.deltaTime;

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);

        foreach (Transform children in ChargeVFXObj.GetComponentInChildren<Transform>())
        {
            children.localScale = Vector3.one * Mathf.Lerp(MinScale, MaxScale, t);
        }


        if (t > BoltCreationThreshold && FullyChargedVFXObj == null)
        {
            LightningSource.PlayOneShot(CompletionSFX);
            GameObject chargeVFX = Instantiate(FullyChargedVFX, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            chargeVFX.transform.parent = ATrinityGameManager.GetSpells().CastPoint.transform;
            FullyChargedVFXObj = chargeVFX;
            LightningSource.clip = SustainSFX;
            LightningSource.loop = true;
            LightningSource.Play();
        }
    }

    public override void CastEnd()
    {
        float channelPercentage = Mathf.Clamp01(ChannelTime / MaxChannelTime);

        LightningSource.loop = false;
        LightningSource.Stop();

        if (ChargeVFXObj != null && channelPercentage >= BoltCreationThreshold)
        {
            LightningSource.PlayOneShot(ReleaseSFX);
            ATrinityController playerController = ATrinityGameManager.GetPlayerController();

            Vector3 castPoint = playerController.transform.position + Vector3.up * playerController.Height + playerController.Forward * 1.5f;

            Quaternion spellRot = Quaternion.Euler(-90f, 0f, 0f);

            LightningBolt lightningBolt = Instantiate(SpellPrefab.gameObject, castPoint, spellRot).GetComponent<LightningBolt>();
            lightningBolt.SetChanneledDamage(channelPercentage);
            //lightningBolt.gameObject.transform.parent = this.transform;
            //lightningBolt.transform.localScale = Vector3.one * Mathf.Lerp(MinScale, MaxScale, t);

            Destroy(lightningBolt.gameObject, 0.5f);

            Destroy(ChargeVFXObj);
            ChargeVFXObj = null;
        }
        if (FullyChargedVFXObj != null)
        {
            Destroy(FullyChargedVFXObj);
            FullyChargedVFXObj = null;
        }
        if (ChargeVFXObj != null)
        {
            ChargeVFXObj.transform.parent = null;
            ChargeVFXObj.AddComponent<Rigidbody>();
            Destroy(ChargeVFXObj, 2);
            ChargeVFXObj = null;
        }

    }
}
