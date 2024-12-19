using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Jerome
{
    public class MonsterGuard : MonoBehaviour
    {
        public GameObject Player;
        private string _currentstate;
        private Animator MonsterController;
        private NavMeshAgent MonsterAgent;
        public List<Transform> Waypoints = new List<Transform>();

        public float AITimer;
        public float AIWaitTime;
        public float GuardTime;
        public float GuardTimer;
        public float ChaseRange;
        public float HitRange;
        public float AFKOutOfRange;
        private int WaypointIndex;

        const string WALKFORWARD = "WalkForward";
        const string IDLE = "Idle";
        const string BITEFORWARD = "BiteForward";

        // Start is called before the first frame update

        private void Awake()
        {
            MonsterController = GetComponent<Animator>();
            MonsterAgent = GetComponent<NavMeshAgent>();
        }
        void Start()
        {

            ChangeAnimationState(WALKFORWARD);
        }

        // Update is called once per frame
        void Update()
        {
            float distanceFromPlayer = (transform.position - Player.transform.position).magnitude;
            AITimer -= Time.deltaTime;
            if (AITimer <= 0)
            {
                if (distanceFromPlayer <= ChaseRange && distanceFromPlayer > HitRange)
                {
                    GuardTimer = AFKOutOfRange;
                    MonsterAgent.destination = Player.transform.position;
                    ChangeAnimationState(WALKFORWARD);
                }
                else if (distanceFromPlayer <= HitRange)
                {
                    GuardTimer = AFKOutOfRange;
                    ChangeAnimationState(BITEFORWARD);
                }
                else if (HasFinishMoving())
                {
                    GuardTimer -= Time.deltaTime;
                    ChangeAnimationState(IDLE);

                    if (GuardTimer > 0)
                    {
                        return;
                    }

                    MonsterAgent.destination = Waypoints[WaypointIndex].position;
                    WaypointIndex++;
                    if (WaypointIndex >= Waypoints.Count)
                    {
                        WaypointIndex = 0;
                    }
                    ChangeAnimationState(WALKFORWARD);

                    GuardTimer = GuardTime;
                }
                AITimer = AIWaitTime;
            }
        }
        bool HasFinishMoving()
        {
            //navMeshAgent_.pathPending is to check if the pathfinder is calculating the path
            return !MonsterAgent.pathPending && !MonsterAgent.hasPath;
        }

        private void OnDrawGizmos()
        {
            if (Waypoints.Count > 2)
            {
                Gizmos.color = Color.blue;
                Vector3 start = Waypoints[0].position;
                Gizmos.DrawWireSphere(start, 0.5f);
                for (int i = 1; i < Waypoints.Count; i++)
                {
                    Vector3 pos = Waypoints[i].position;
                    Gizmos.DrawLine(start, pos);
                    Gizmos.DrawWireSphere(pos, 0.5f);
                    start = pos;
                }
                Gizmos.DrawLine(start, Waypoints[0].position);//draw the last line
            }
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