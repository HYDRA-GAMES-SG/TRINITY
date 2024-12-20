using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Huajin
{
    public class AttackState : PlantCreatureState
    {
        Vector3 PlayerPos;
        Vector3 PlantPos;

        [SerializeField] string AnimKey;
        public override bool CheckEnterTransition(IState fromState)
        {
            return true;
        }

        public override void EnterBehaviour(float dt, IState fromState)
        {
            PlayerPos = PlantCreatureFSM.PlayerController.transform.position;
            PlantPos = PlantCreatureFSM.PlantCreatureController.transform.position;

            PlantCreatureFSM.Animator.SetBool(AnimKey, true);
        }

        public override void PreUpdateBehaviour(float dt)
        {
        }

        public override void UpdateBehaviour(float dt)
        {
            Vector3 faceDirection = (PlayerPos - PlantPos).normalized;
            PlantCreatureFSM.PlantCreatureController.RotateTowardTarget(faceDirection);

            AnimatorStateInfo stateInfo = PlantCreatureFSM.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.9f)
            {
                PlantCreatureFSM.EnqueueTransition<IdleState>();
            }
        }
        public override void PostUpdateBehaviour(float dt)
        {
        }

        public override void ExitBehaviour(float dt, IState toState)
        {
        }

        public override bool CheckExitTransition(IState toState)
        {
            return true;
        }
    }
}
