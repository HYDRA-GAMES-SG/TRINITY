using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class AInvincibleAudio : IAudioManager
{
    AInvincibleBossController Controller;
    public Transform Lefthand;
    public Transform Righthand;
    public Transform Mounth;
    private void Awake()
    {
        base.Awake();
        //UAttackColliderComponent.OnPlayerHit += PlayHitPlayerAudio;
        //UAttackColliderComponent.OnGroundHit += PlayHitGroundAudio;

        Controller = GetComponentInParent<AInvincibleBossController>();
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

    public void PlayIBFootStep()
    {
        Play("FootStep");
    }
    public void PlayIBHandSwing(string hand)
    {
        if (hand.Contains("left"))
        {
            PlayAtPosition("HandSwing", Lefthand);
        }
        else
        {
            PlayAtPosition("HandSwing", Righthand);
        }
    }
    public void PlayRoar()
    {
        PlayAtPosition("Roar", Mounth);
    }
    public void PlayRoarSmall()
    {
        PlayAtPosition("RoarSmall", Mounth);
    }
    public void PlayIBHandSmash()
    {
        PlayAtPosition("Smash", Righthand);
    }
    public void PlayIBStompAndEffect()
    {
        Play("StompAndEffect");
    }
    public void PlayIBShockCharge()
    {
        PlayAtPosition("ShockCharge", Mounth);
    }
    public void PlayIBShockRelease()
    {
        Play("ShockRelease");
    }
    public void PlayIBOrbCharge()
    {
        PlayAtPosition("OrbCharge", Lefthand);
    }
    public void PlayIBOrbRelease()
    {
        PlayAtPosition("OrbRelease", Lefthand);
    }
    public void PlayIBOrbExplode()
    {
        Play("OrbExplode");
    }
    public void PlayIBTaunt()
    {
        Play("Taunt");
    }
    public void PlayIBDead()
    {
        PlayAtPosition("Dead", Mounth);
    }

}
