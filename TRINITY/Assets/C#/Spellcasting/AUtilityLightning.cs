using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUtilityLightning : ASpell
{
    public AudioSource UtilityLightningSource;

    [HideInInspector]
    public GameObject FlashPointObj;

    [SerializeField] private Vector3 BlinkPos;

    public float DurationToBlink;
    public bool bCanBlink;

    [Header("VFX")]
    public GameObject FlashBackVFX;

    [Header("SFX")]
    public AudioClip FlashBackSFX;
    public AudioClip SetFlashBackSFX;
    public override void Initialize()
    {
        UtilityLightningSource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        ATrinityController playerController = ATrinityGameManager.GetPlayerController();
        
        if (!bCanBlink)
        {
            BlinkPos = playerController.transform.position;
            FlashPointObj = Instantiate(SpellPrefab, playerController.transform.position, Quaternion.identity);
            bCanBlink = true;
            UtilityLightningSource.PlayOneShot(SetFlashBackSFX);
        }
        else
        {
            if (FlashPointObj != null) 
            {
                Destroy(FlashPointObj);
            }
            playerController.transform.position = BlinkPos;
            GameObject flashBackVFX = Instantiate(FlashBackVFX, playerController.transform.position, Quaternion.identity);
            bCanBlink = false;
            UtilityLightningSource.PlayOneShot(FlashBackSFX);
            StartCooldown();
        }
    }

    public override void CastUpdate()
    {
        DurationToBlink -= Time.deltaTime;
        if (DurationToBlink < 0)
        {
            Destroy(FlashPointObj);
            bCanBlink = false;
            print("Flashpoint Duration expired");
        }
    }

    public override void CastEnd()
    {

    }
}
