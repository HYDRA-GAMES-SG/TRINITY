using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TitusAssignment
{
    public class ExplosionMonsterPursueState : ExplosionMonsterState
    {
        NavMeshAgent AI;
        ExplosionMonsterController EMController;
        ATrinityController PlayerController;
        public override bool CheckEnterTransition(IState fromState)
        {
            return true;
        }

        public override bool CheckExitTransition(IState fromState)
        {
            return true;
        }

        public override void EnterBehaviour(float dt, IState fromState)
        {
            EMController = explosionMonsterFSM.explosionMonsterController;
            PlayerController = explosionMonsterFSM.PlayerController;

            AI = EMController.AI;
        }

        public override void ExitBehaviour(float dt, IState toState)
        {
        }

        public override void PostUpdateBehaviour(float dt)
        {
        }

        public override void PreUpdateBehaviour(float dt)
        {
        }

        public override void UpdateBehaviour(float dt)
        {
            AI.SetDestination(PlayerController.transform.position);
            if (EMController.CalculateDistance() <= EMController.ExplosionRange)
            {
                explosionMonsterFSM.EnqueueTransition<ExplosionMonsterExplosionState>();
            }
        }
    }
}
