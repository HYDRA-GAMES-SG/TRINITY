using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TitusAssignment
{
    public abstract class ExplosionMonsterState : MonoBehaviour, IState
    {
        protected ExplosionMonsterFSM explosionMonsterFSM;

        public void Awake()
        {
        }

        public virtual bool CheckEnterTransition(IState fromState)
        {
            return false;
        }

        public virtual bool CheckExitTransition(IState fromState)
        {
            return false;
        }

        public virtual void EnterBehaviour(float dt, IState fromState)
        {
        }

        public virtual void ExitBehaviour(float dt, IState toState)
        {
        }

        public virtual void PostUpdateBehaviour(float dt)
        {
        }

        public virtual void PreUpdateBehaviour(float dt)
        {
        }

        public virtual void UpdateBehaviour(float dt)
        {
        }

        public virtual void SetStateMachine(ExplosionMonsterFSM _explosionMonsterFSM)
        {
            explosionMonsterFSM = _explosionMonsterFSM;
        }
    }
}

