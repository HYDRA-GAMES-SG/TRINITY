using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ACrabAudio : IAudioManager
{
    public Transform LeftHand;
    public Transform RightHand;
    public Transform Mouth;
    ACrabController Controller;

    private void Awake()
    {
        base.Awake();
        //UAttackColliderComponent.OnPlayerHit += PlayHitPlayerAudio;
        //UAttackColliderComponent.OnGroundHit += PlayHitGroundAudio;

        Controller = GetComponentInParent<ACrabController>();
    }

    void PlayHitGroundAudio(float impulseMagnitude)//useless now
    {
        Debug.Log("Ground Hit Impulse Magnitude:" + impulseMagnitude);
        //need to test impulseMagnitude to find a reasonable max and min
        //float maxMagnitude = 50f;
        //float minMagnitude = 1f;
        //impulseMagnitude -= minMagnitude;
        //impulseMagnitude /= maxMagnitude;
        PlayWithVolume("HitGround", Mathf.Clamp01(impulseMagnitude));
    }
    void PlayHitPlayerAudio()
    {
        Debug.Log("HitPlayer");
        Play("HitPlayer");
    }

    public void PlayCrabWalkFootStep()
    {
        Play("Walk");
    }
    public void PlayPhase2IceEffectSound()//walk with ice sound bec ice came out when phase 2
    {
        Play("Phase2IceMove");
    }
    public void PlayCrawSwing(string craw)//normal/combo
    {
        if (craw.Contains("left"))
        {
            PlayAtPosition("NormalAttackSwing", LeftHand);
        }
        else
        {
            PlayAtPosition("NormalAttackSwing", RightHand);
        }
    }
    public void PlayCrawIceSwing(string craw)
    {
        if (craw.Contains("left"))
        {
            PlayAtPosition("IceSwing", LeftHand);
        }
        else
        {
            PlayAtPosition("IceSwing", RightHand);
        }
    }
    public void PlaySmashGround(string craw)//normal/combo
    {
        if (craw.Contains("left"))
        {
            PlayAtPosition("Smash", LeftHand);
        }
        else
        {
            PlayAtPosition("Smash", RightHand);
        }
    }
    public void PlayIceSmashGround(string craw)
    {
        if (craw.Contains("left"))
        {
            PlayAtPosition("Smash", LeftHand);
            PlayAtPosition("IceSmash", LeftHand);
        }
        else
        {
            PlayAtPosition("Smash", RightHand);
            PlayAtPosition("IceSmash", RightHand);
        }
    }
    public void PlayJump()
    {
        Play("Jump");
    }
    public void PlayLand()
    {
        Play("Land");
    }
    public void PlayJumpSmashGround()
    {
        PlayAtPosition("BigSmash", RightHand);
    }
    public void PlayIceJumpSmashGround()
    {
        PlayAtPosition("BigSmash", RightHand);
        PlayAtPosition("IceBigSmash", RightHand);
    }
    public void PlayIceSpray()
    {
        StartLoop("IceSpray");
    }
    public void StopIceSpray()
    {
        StopLoop("IceSpray");
    }
    public void PlayChargeAttackRoar()
    {
        PlayAtPosition("ChargeAttackRoar",Mouth);
        
    }
    public void PlayCrabGetHit()
    {
        Debug.Log("Get hit sfx");
        Play("GetHit");
    }
    public void PlayCrabDeath()
    {
        Play("Death");
    }
    public void PlayPhase2Roar()
    {
        PlayAtPosition("Phase2IceSound", Mouth);
    }
}
