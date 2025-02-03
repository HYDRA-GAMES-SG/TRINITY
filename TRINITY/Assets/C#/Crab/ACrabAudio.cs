using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACrabAudio : IAudioManager
{
    private void Awake()
    {

        UAttackColliderComponent.OnPlayerHit += PlayHitPlayerAudio;
        UAttackColliderComponent.OnGroundHit += PlayHitGroundAudio;
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
    void PlaySmashAudio()
    {
        Play("Smash");
    }

    void PlayHitPlayerAudio()   
    {
        Debug.Log("hit player audio");
        Play("HitPlayer");
    }

    //void PlayAudio()
    //{
    //    IState currentState = Controller.CrabFSM.CurrentState;

    //    if(currentState is Pursue)
    //    {
    //         //do whatever
    //    }
    //}
}
