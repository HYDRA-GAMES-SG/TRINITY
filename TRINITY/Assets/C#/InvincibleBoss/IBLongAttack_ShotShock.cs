using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBLongAttack_ShotShock : InvincibleBossState
{
    [SerializeField]
    string AnimKey = "SpitterShot2";

    [SerializeField] Transform ShockBluePos;
    [SerializeField] ParticleSystem ShockBlue;

    [SerializeField] float rotateSpeed;

    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.bCanShotShock && fromState is IBPursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKey);

        ShockBlue.Play();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (InvincibleBossFSM.PlayerController.transform.position - InvincibleBossFSM.InvincibleBossController.transform.position).normalized;
        InvincibleBossFSM.InvincibleBossController.RotateTowardTarget(faceDirection, rotateSpeed);

        string layerName = GetType().Name;
        int layerIndex = InvincibleBossFSM.InvincibleBossController.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = InvincibleBossFSM.InvincibleBossController.Animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            InvincibleBossFSM.EnqueueTransition<IBPursue>();
        }
        //have to adjust the particle or have some delay 
        Vector3 playerBasePos = InvincibleBossFSM.PlayerController.transform.position;
        float playerMidHeight = InvincibleBossFSM.PlayerController.GetComponent<CapsuleCollider>().height / 2f;
        Vector3 playerMidPos = new Vector3(playerBasePos.x, playerBasePos.y + playerMidHeight, playerBasePos.z);
        Vector3 ShockBlueDirection = (playerMidPos - ShockBluePos.position).normalized;
        ShockBluePos.transform.rotation = Quaternion.LookRotation(ShockBlueDirection, Vector3.up);

    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        ShockBlue.Stop();
        InvincibleBossFSM.InvincibleBossController.bCanShotShock = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
