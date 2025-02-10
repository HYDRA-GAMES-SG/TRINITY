using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class AInvincibleAudio : IAudioManager
{
    AInvincibleBossController Controller;
    private void Awake()
    {
        base.Awake();
        UAttackColliderComponent.OnPlayerHit += PlayHitPlayerAudio;
        UAttackColliderComponent.OnGroundHit += PlayHitGroundAudio;

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
    public void PlayIBHandSwing()
    {
        Play("HandSwing");
    }
    public void PlayIBHandSmash()
    {
        Play("Smash");
    }
    public void PlayIBStompAndEffect()
    {
        Play("StompAndEffect");
    }
    public void PlayIBShockCharge()
    {
        Play("ShockCharge");
    }
    public void PlayIBShockRelease()
    {
        Play("ShockRelease");
    }
    public void PlayIBOrbCharge()
    {
        Play("OrbCharge");
    }
    public void PlayIBOrbRelease()
    {
        Play("OrbRelease");
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
        Play("Dead");
    }

}
