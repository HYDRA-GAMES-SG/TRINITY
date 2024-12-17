using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ACrabPhaseTwoAnimationEvent : MonoBehaviour
{
    [SerializeField] ParticleSystem IceClawSwingAttack;
    [SerializeField] ParticleSystem SmashFrozenGroundAttack;
    [SerializeField] ParticleSystem JumpSmashFrozenGroundAttack;

    [SerializeField] Transform LeftClaw;
    [SerializeField] Transform RightClaw;

    ACrabController Controller;

    [SerializeField] AudioMixerGroup AttackMixer;

    [SerializeField] AudioSource CrabAttackSource;

    [SerializeField] AudioClip ClawSound;
    [SerializeField] AudioClip SmashSound;
    [SerializeField] AudioClip IceClawSound;
    [SerializeField] AudioClip IceSmashSound;


    private void Start()
    {
        Controller = GetComponent<ACrabController>();
    }
    
    public void RPhaseTwoComboClawAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem iceClawSwingAttack = Instantiate(IceClawSwingAttack, RightClaw.position, RightClaw.rotation);
            AAttackCollider projectileController = iceClawSwingAttack.GetComponentInChildren<AAttackCollider>();
            projectileController.GetCrabController(Controller);
        }
    }
    public void LPhaseTwoComboClawAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem iceClawSwingAttack = Instantiate(IceClawSwingAttack, LeftClaw.position, LeftClaw.rotation);
            AAttackCollider projectileController = iceClawSwingAttack.GetComponentInChildren<AAttackCollider>();
            projectileController.GetCrabController(Controller);
        }
    }


    public void RPhaseTwoComboSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem smashFrozenGroundAttack = Instantiate(SmashFrozenGroundAttack, RightClaw.position, RightClaw.rotation);
            AAttackCollider projectileController = smashFrozenGroundAttack.GetComponentInChildren<AAttackCollider>();
            projectileController.GetCrabController(Controller);
        }
    }
    public void LPhaseTwoComboSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem smashFrozenGroundAttack = Instantiate(SmashFrozenGroundAttack, LeftClaw.position, LeftClaw.rotation);
            AAttackCollider projectileController = smashFrozenGroundAttack.GetComponentInChildren<AAttackCollider>();
            projectileController.GetCrabController(Controller);

        }
    }


    public void PhaseTwoJumpSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem jumpSmashFrozenGroundAttack = Instantiate(JumpSmashFrozenGroundAttack, RightClaw.position, RightClaw.rotation);
            AAttackCollider projectileController = jumpSmashFrozenGroundAttack.GetComponent<AAttackCollider>();
            projectileController.GetCrabController(Controller);

        }
    }

    public void LeftClawAttackSound()
    {
        AudioSource source = Instantiate(CrabAttackSource, LeftClaw.transform);
        PlayAttackSound(source, ClawSound);
        Destroy(source, ClawSound.length);
    }
    public void RightClawAttackSound()
    {
        AudioSource source = Instantiate(CrabAttackSource, RightClaw.transform);
        PlayAttackSound(source, ClawSound);
        Destroy(source, ClawSound.length);
    }
    public void LeftSmashAttackSound()
    {
        AudioSource source = Instantiate(CrabAttackSource, LeftClaw.transform);
        PlayAttackSound(source, SmashSound);
        Destroy(source, SmashSound.length);
    }
    public void RightSmashAttackSound()
    {
        AudioSource source = Instantiate(CrabAttackSource, RightClaw.transform);
        PlayAttackSound(source, SmashSound);
        Destroy(source, SmashSound.length);
    }

    public void PlayAttackSound(AudioSource source, AudioClip attackSound)
    {
        source.outputAudioMixerGroup = AttackMixer;
        source.PlayOneShot(attackSound);
    }
}
