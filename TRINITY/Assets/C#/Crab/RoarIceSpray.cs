using UnityEngine;
using UnityEngine.InputSystem.XR;

public class RoarIceSpray : CrabState
{
    [SerializeField] ParticleSystem IceSpray;
    [SerializeField] Transform CrabMounth;
    ParticleSystem iceSpray;
    bool AnimFinish = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is Pursue && CrabFSM.CrabController.CanRoarStun;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        AnimFinish = false;

        CrabFSM.CrabController.bCanChill = false;

        CrabFSM.CrabController.AI.ResetPath();
        CrabFSM.Animator.SetBool("Roar1", true);

        if (iceSpray == null)
        {
            iceSpray = Instantiate(IceSpray, CrabMounth);
            UAttackColliderComponent projectileController = iceSpray.GetComponentInChildren<UAttackColliderComponent>();
        }
        else
        {
            iceSpray.Play();
        }
       
    }

    public override void PreUpdateBehaviour(float dt)
    {

    }

    public override void UpdateBehaviour(float dt)
    {
        CrabFSM.Animator.speed = 1;

        Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
        CrabFSM.CrabController.RotateTowardTarget(faceDirection, RotateSpeed);

        Vector3 iceSprayDirection = (CrabFSM.PlayerController.transform.position - CrabMounth.position).normalized;
        iceSpray.transform.rotation = Quaternion.LookRotation(iceSprayDirection, Vector3.up);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roar3") && stateInfo.normalizedTime >= 0.95f)
        {
            AnimFinish = true;
            CrabFSM.EnqueueTransition<Pursue>();
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.CrabController.CanRoarStun = false;
        iceSpray.Stop();
        CrabFSM.CrabController.bCanChill = true;

    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is Pursue || toState is Death || (toState is Idle && AnimFinish);
    }
}
