using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Jerome
{

    public class MagicalMonster : MonoBehaviour
    {
        public float AITimer;
        public float AIWaitTime;

        public GameObject Player;
        private string _currentstate;
        private Animator MonsterController;
        private NavMeshAgent MonsterAgent;

        [SerializeField] 
        private ETrinityElement CurrentElement;
        [SerializeField]
        private ETrinityElement PreviousElement;

        const string CHASE = "";
        const string ATTACK = "";
        const string RANGEDATTACK = "";
        const string ICESURFING = "";

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        bool HasFinishMoving()
        {
            //navMeshAgent_.pathPending is to check if the pathfinder is calculating the path
            return !MonsterAgent.pathPending && !MonsterAgent.hasPath;
        }
        private void ChangeAnimationState(string newState)
        {
            if (newState == _currentstate)
            {
                return;
            }
            MonsterController.Play(newState);

            _currentstate = newState;
        }
        bool IsAnimationPlaying(Animator animator, string stateName)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                return true;
            }

            else
            {
                return false;
            }
        }
    }

}
