using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUtilityLightning : ASpell
{
    public AudioSource UtilityLightningSource;
    public ATrinitySpells TrinitySpells;
    public GameObject FlashPointObj;

    [SerializeField] private Vector3 BlinkPos;

    public float DurationToBlink;
    public bool bCanBlink;

    [Header("VFX")]
    public GameObject FlashPointVFX;
    public GameObject FlashBackVFX;
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
            FlashPointObj = Instantiate(FlashPointVFX, playerController.transform.position, Quaternion.identity);
            bCanBlink = true;
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
