using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Huajin
{
    public class IdleState : PlantCreatureState
    {
        Vector3 PlayerPos;
        Vector3 PlantPos;

        [SerializeField] float TimeBackToHide;

        float Timer;
        public override bool CheckEnterTransition(IState fromState)
        {
            return !PlantCreatureFSM.PlantCreatureController.health.bDead;
        }

        public override void EnterBehaviour(float dt, IState fromState)
        {
            if (fromState is HideState)
            {
                PlantCreatureFSM.Animator.SetBool("Idle", true);
            }
            else
            {
                PlantCreatureFSM.Animator.SetBool("Attack", false);
            }

            Timer = TimeBackToHide;
        }

        public override void PreUpdateBehaviour(float dt)
        {
        }

        public override void UpdateBehaviour(float dt)
        {
            PlayerPos = PlantCreatureFSM.PlayerController.transform.position;
            PlantPos = PlantCreatureFSM.PlantCreatureController.transform.position;

            Vector3 playerPos = new Vector3(PlayerPos.x, 0, PlayerPos.z);
            Vector3 plantPos = new Vector3(PlantPos.x, 0, PlantPos.z);

            float distanceToTarget = Vector3.Distance(playerPos, plantPos);

            AnimatorStateInfo stateInfo = PlantCreatureFSM.Animator.GetCurrentAnimatorStateInfo(0);

            if (distanceToTarget <= PlantCreatureFSM.PlantCreatureController.AttackRange)
            {
                PlantCreatureFSM.EnqueueTransition<AttackState>();
            }
            else
            {
                Timer -= Time.deltaTime;
                if (Timer <= 0)
                {
                    PlantCreatureFSM.EnqueueTransition<HideState>();

                }
            }
        }
        public override void PostUpdateBehaviour(float dt)
        {
        }

        public override void ExitBehaviour(float dt, IState toState)
        {
            if (toState is AttackState)
            {
                Timer = TimeBackToHide;
            }
        }

        public override bool CheckExitTransition(IState toState)
        {
            return true;
        }
    }
}
