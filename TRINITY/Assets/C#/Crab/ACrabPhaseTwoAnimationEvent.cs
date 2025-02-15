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
    [SerializeField] ACrabAudio Crabaudio;

    ACrabController Controller;

    private void Start()
    {
        Controller = GetComponent<ACrabController>();
    }

    public void RPhaseTwoComboClawAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem iceClawSwingAttack = Instantiate(IceClawSwingAttack, RightClaw.position, RightClaw.rotation);

            UAttackColliderComponent projectileController = iceClawSwingAttack.GetComponentInChildren<UAttackColliderComponent>();
            projectileController.SetController(Controller);
            Crabaudio.PlayCrawIceSwing("right");
        }
    }
    public void LPhaseTwoComboClawAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem iceClawSwingAttack = Instantiate(IceClawSwingAttack, LeftClaw.position, LeftClaw.rotation);

            UAttackColliderComponent projectileController = iceClawSwingAttack.GetComponentInChildren<UAttackColliderComponent>();
            projectileController.SetController(Controller);
            Crabaudio.PlayCrawIceSwing("left");

        }
    }


    public void RPhaseTwoComboSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem smashFrozenGroundAttack = Instantiate(SmashFrozenGroundAttack, RightClaw.position, RightClaw.rotation);


            //UAttackColliderComponent projectileController = smashFrozenGroundAttack.GetComponentInChildren<UAttackColliderComponent>();
            //projectileController.SetController(Controller);
            DelayedColliderActivator SmashParticle = smashFrozenGroundAttack.GetComponentInChildren<DelayedColliderActivator>();
            SmashParticle.GetControllerDamage(Controller, Controller.GetParticleAttack());

            Crabaudio.PlayIceSmashGround("right");
        }
        else
        {
            Crabaudio.PlaySmashGround("right");
        }
    }
    public void LPhaseTwoComboSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem smashFrozenGroundAttack = Instantiate(SmashFrozenGroundAttack, LeftClaw.position, LeftClaw.rotation);

            //UAttackColliderComponent projectileController = smashFrozenGroundAttack.GetComponentInChildren<UAttackColliderComponent>();
            //projectileController.SetController(Controller);

            DelayedColliderActivator SmashParticle = smashFrozenGroundAttack.GetComponentInChildren<DelayedColliderActivator>();
            SmashParticle.GetControllerDamage(Controller, Controller.GetParticleAttack());

            Crabaudio.PlayIceSmashGround("left");
        }
        else
        {
            Crabaudio.PlaySmashGround("left");
        }
    }


    public void PhaseTwoJumpSmashAttack()
    {
        //if (Controller.bElementPhase)
        {
            ParticleSystem jumpSmashFrozenGroundAttack = Instantiate(JumpSmashFrozenGroundAttack, RightClaw.position, RightClaw.rotation);

            //UAttackColliderComponent projectileController = jumpSmashFrozenGroundAttack.GetComponentInChildren<UAttackColliderComponent>();
            //projectileController.SetController(Controller);

            DelayedColliderActivator SmashParticle = jumpSmashFrozenGroundAttack.GetComponentInChildren<DelayedColliderActivator>();
            SmashParticle.GetControllerDamage(Controller, Controller.GetParticleAttack());

            //Crabaudio.PlayJumpSmashGround();
            Crabaudio.PlayIceJumpSmashGround();
        }
        //else
        {
        }
    }


}
