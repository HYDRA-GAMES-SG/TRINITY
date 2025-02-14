using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ACrabAudio : IAudioManager
{
    public Transform LeftHand;
    public Transform RightHand;
    ACrabController Controller;

    private void Awake()
    {
        base.Awake();
        UAttackColliderComponent.OnPlayerHit += PlayHitPlayerAudio;
        UAttackColliderComponent.OnGroundHit += PlayHitGroundAudio;

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
        //.Debug.Log("Crab walk sound");
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
            //Debug.Log("LeftClawAttack");
            PlayAtPosition("NormalAttackSwing", LeftHand);
        }
        else
        {
            //Debug.Log("RightClawAttack");
            PlayAtPosition("NormalAttackSwing", RightHand);
        }
    }
    public void PlayCrawIceSwing(string craw)
    {
        if (craw.Contains("left"))
        {
            //Debug.Log("LeftClawAttack");
            PlayAtPosition("IceSwing", LeftHand);
        }
        else
        {
            //Debug.Log("RightClawAttack");
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
        Debug.Log("Jump sfx");
        Play("Jump");
    }
    public void PlayLand()
    {
        Debug.Log("Land sfx");
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
        Debug.Log("IceSpray sfx");
        StartLoop("IceSpray");
        //Play("IceSpray");
    }
    public void StopIceSpray()
    {
        Debug.Log("IceSpray sfx");
        StopLoop("IceSpray");
    }
    public void PlayChargeAttackRoar()
    {
        Debug.Log("Chrage attack Roar sfx");
        Play("ChargeAttackRoar");
    }
    public void PlayCrabGetHit()
    {
        Debug.Log("Get hit sfx");
        Play("GetHit");
    }
    public void PlayCrabDeath()
    {
        Debug.Log("Crab dead sfx");
        Play("Death");
    }
    public void PlayPhase2Roar()
    {
        Debug.Log("Phase 2 Roar sfx");
        Play("Phase2IceSound");
    }
}
