using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace Huajin
{

    public class RabbitFlee : RabbitState
    {
        [SerializeField] float FleeSpeed;
        [SerializeField] float WaitTime = 0.5f;
        [SerializeField] float FleeDistance = 3;
        float aiWaitTime_;
        bool hasFlee = false;

        public override void Awake()
        {
        }

        public override bool CheckEnterTransition(IState fromState)
        {
            return true;
        }

        public override void EnterBehaviour(float dt, IState fromState)
        {
            RabbitFSM.RabbitController.AI.speed = FleeSpeed;
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
            aiWaitTime_ -= Time.deltaTime;
            if (aiWaitTime_ <= 0)
            {
                Vector3 dirToTarget = RabbitFSM.PlayerController.transform.position - RabbitFSM.RabbitController.transform.position;
                Vector3 dirToFlee = -dirToTarget;
                Vector3 fleeVector = dirToFlee.normalized * FleeDistance;
                Vector3 fleeDestination = RabbitFSM.RabbitController.transform.position + fleeVector;
                if (NavMesh.Raycast(RabbitFSM.RabbitController.transform.position, fleeDestination, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        fleeDestination = RabbitFSM.RabbitController.transform.position + Quaternion.Euler(0, i * 30, 0) * fleeVector;
                        if (!NavMesh.Raycast(RabbitFSM.RabbitController.transform.position, fleeDestination, out hit, NavMesh.AllAreas))
                        {
                            break;
                        }
                        fleeDestination = RabbitFSM.RabbitController.transform.position + Quaternion.Euler(0, i * -30, 0) * fleeVector;
                        if (!NavMesh.Raycast(RabbitFSM.RabbitController.transform.position, fleeDestination, out hit, NavMesh.AllAreas))
                        {
                            break;
                        }
                    }
                }
                RabbitFSM.RabbitController.AI.SetDestination(fleeDestination);
                aiWaitTime_ = WaitTime;
            }
            if (RabbitFSM.RabbitController.CalculateDistance() >= 20f)
            {
                RabbitFSM.EnqueueTransition<RabbitIdle>();
            }
        }
        public override bool CheckExitTransition(IState fromState)
        {
            return true;
        }
        public bool HasFinishMoving()
        {
            return !RabbitFSM.RabbitController.AI.pathPending && !RabbitFSM.RabbitController.AI.hasPath;
        }
    }

}