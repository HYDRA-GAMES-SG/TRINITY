using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Huajin
{

    public class RabbitDead : RabbitState
    {
        public override void Awake()
        {
        }

        public override bool CheckEnterTransition(IState fromState)
        {
            return true;
        }

        public override void EnterBehaviour(float dt, IState fromState)
        {
            RabbitFSM.RabbitController.AI.ResetPath();
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
        }
        public override bool CheckExitTransition(IState fromState)
        {
            return true;
        }
    }

}