using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Huajin
{
    public abstract class PlantCreatureState : MonoBehaviour, IState
    {
        protected PlantCreatureFSM PlantCreatureFSM;

        public void Awake()
        {
        }
        public virtual bool CheckEnterTransition(IState fromState)
        {
            return false;
        }

        public virtual void EnterBehaviour(float dt, IState fromState)
        {
        }
        public virtual void PreUpdateBehaviour(float dt)
        {
        }
        public virtual void UpdateBehaviour(float dt)
        {
        }
        public virtual void PostUpdateBehaviour(float dt)
        {
        }
        public virtual bool CheckExitTransition(IState fromState)
        {
            return false;
        }
        public virtual void ExitBehaviour(float dt, IState toState)
        {
        }
        public virtual void SetStateMachine(PlantCreatureFSM plantCreatureMachine)
        {
            PlantCreatureFSM = plantCreatureMachine;
        }

    }
}
