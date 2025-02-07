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
        }
    }
    public void LPhaseTwoComboClawAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem iceClawSwingAttack = Instantiate(IceClawSwingAttack, LeftClaw.position, LeftClaw.rotation);

            UAttackColliderComponent projectileController = iceClawSwingAttack.GetComponentInChildren<UAttackColliderComponent>();
            projectileController.SetController(Controller);

        }
    }


    public void RPhaseTwoComboSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem smashFrozenGroundAttack = Instantiate(SmashFrozenGroundAttack, RightClaw.position, RightClaw.rotation);


            UAttackColliderComponent projectileController = smashFrozenGroundAttack.GetComponentInChildren<UAttackColliderComponent>();
            projectileController.SetController(Controller);
        }
    }
    public void LPhaseTwoComboSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem smashFrozenGroundAttack = Instantiate(SmashFrozenGroundAttack, LeftClaw.position, LeftClaw.rotation);

            UAttackColliderComponent projectileController = smashFrozenGroundAttack.GetComponentInChildren<UAttackColliderComponent>();
            projectileController.SetController(Controller);
        }
    }


    public void PhaseTwoJumpSmashAttack()
    {
        if (Controller.bElementPhase)
        {
            ParticleSystem jumpSmashFrozenGroundAttack = Instantiate(JumpSmashFrozenGroundAttack, RightClaw.position, RightClaw.rotation);

            UAttackColliderComponent projectileController = jumpSmashFrozenGroundAttack.GetComponentInChildren<UAttackColliderComponent>();
            projectileController.SetController(Controller);
        }
    }
}
