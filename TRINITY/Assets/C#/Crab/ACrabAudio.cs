using System;
using System.Collections;
using System.Collections.Generic;
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
   
    void PlayHitGroundAudio(float impulseMagnitude)
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
        Debug.Log("hit player audio");
        Play("HitPlayer");
    }

    public void PlayCrabWalkFootStep()
    {
        //.Debug.Log("Crab walk sound");
        Play("Walk");
    }
    public void PlayPhase2IceEffectSound()
    {

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
    public void PlayCrawIceSwing()
    {

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
    public void PlayIceSmashGround()
    {

    }
    public void PlayJump()
    {
        Debug.Log("Jump sfx");
    }
    public void PlayLand()
    {
        Debug.Log("Land sfx");

    }
    public void PlayJumpSmashGround()
    {
        PlayAtPosition("BigSmash", RightHand);
    }
    public void PlayIceJumpSmashGround()
    {
        
    }
    public void PlayIceSpray()
    {
        Debug.Log("IceSpray sfx");
    }
    public void PlayPhase2Roar()
    {
        Debug.Log("Phase 2 Roat sfx");
    }
    public void PlayCrabGetHit()
    {
        Debug.Log("Get hit sfx");
    }
    public void PlayCrabDeath()
    {
        Debug.Log("Crab dead sfx");
    }
}
