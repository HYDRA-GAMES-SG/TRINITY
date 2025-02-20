using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Huajin
{
    public class DeadState : PlantCreatureState
    {
        public override bool CheckEnterTransition(IState fromState)
        {
            return true;
        }

        public override void EnterBehaviour(float dt, IState fromState)
        {
            PlantCreatureFSM.Animator.SetTrigger("Dead");
        }

        public override void PreUpdateBehaviour(float dt)
        {

        }

        public override void UpdateBehaviour(float dt)
        {
        }
        public override void PostUpdateBehaviour(float dt)
        {
        }

        public override void ExitBehaviour(float dt, IState toState)
        {
        }

        public override bool CheckExitTransition(IState toState)
        {
            return false;
        }
    }
}

