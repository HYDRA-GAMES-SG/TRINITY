using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Huajin
{
    public class ARabbitController : IEnemyController

    {
        public ARabbitFSM RabbitFSM;

        void Start()
        {

        }

        void Update()
        {
            if (EnemyStatus.Health.Current <= 0f)
            {
                RabbitFSM.EnqueueTransition<RabbitDead>();
            }
        }

        public float CalculateDistance()
        {
            Vector3 PlayerPos = RabbitFSM.PlayerController.transform.position;

            float distanceToTarget = Vector3.Distance(PlayerPos, transform.position);
            return distanceToTarget;
        }
    }

}