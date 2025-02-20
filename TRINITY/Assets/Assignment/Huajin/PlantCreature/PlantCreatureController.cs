using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Huajin
{
    public class PlantCreatureController : IEnemyController
    {
        public PlantCreatureFSM PlantFSM;

        public float AttackRange;

        public UHealthComponent health;

        void Start()
        {
            health = GetComponent<UHealthComponent>();
            AI.updateRotation = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (health.Current <= 0 && !(PlantFSM.CurrentState is HideState))
            {
                PlantFSM.EnqueueTransition<DeadState>();
            }
        }

        public void RotateTowardTarget(Vector3 directionToTarget)
        {
            Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }
}
