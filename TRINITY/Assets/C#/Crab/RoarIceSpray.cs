using UnityEngine;
using UnityEngine.InputSystem.XR;

public class RoarIceSpray : CrabState
{
    [SerializeField] ParticleSystem IceSpray;
    [SerializeField] Transform CrabMounth;
    ParticleSystem iceSpray;
    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is Pursue)
        {
            if (CrabFSM.CrabController.CanRoarStun)
            {
                return true;
            }
        }
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabFSM.CrabController.AI.ResetPath();
        CrabFSM.Animator.SetBool("Roar1", true);

        if (iceSpray == null)
        {
            iceSpray = Instantiate(IceSpray, CrabMounth);
            AAttackCollider projectileController = iceSpray.GetComponentInChildren<AAttackCollider>(); 
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
        Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
        CrabFSM.CrabController.RotateTowardTarget(faceDirection, RotateSpeed);

        Vector3 iceSprayDirection = (CrabFSM.PlayerController.transform.position - CrabMounth.position).normalized;
        iceSpray.transform.rotation = Quaternion.LookRotation(iceSprayDirection, Vector3.up);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roar3") && stateInfo.normalizedTime >= 0.9f)
        {
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
    }

    public override bool CheckExitTransition(IState toState)
    {
        if (toState is Pursue || toState is Death)
        {
            return true;
        }
        return false;
    }
}
