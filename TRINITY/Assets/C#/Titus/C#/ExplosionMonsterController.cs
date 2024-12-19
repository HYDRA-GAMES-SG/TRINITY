using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace TitusAssignment
{
    public class ExplosionMonsterController : IEnemyController
    {
        public ExplosionMonsterFSM fsm;
        public ParticleSystem ExplosionEffect;
        public GameObject ExplosionMonster;
        public float PursueRange = 20f;
        public float ExplosionRange = 5f;
        public float ExplosionSizeScale = 3f;
        public float JumpForce = 40f;
        public float ExplodeTimer = 1f;
        public float ExplodeDelay = 1f;
        public bool bExploded;
        public Rigidbody rb;

        [SerializeField] float ExplosionDamage;
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
        void Update()
        {

        }
        public float CalculateDistance()
        {
            Vector3 PlayerPos = new Vector3(fsm.PlayerController.transform.position.x, fsm.PlayerController.transform.position.y, fsm.PlayerController.transform.position.z);
            Vector3 EMPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            float distanceToTarget = Vector3.Distance(PlayerPos, EMPos);
            return distanceToTarget;
        }
        public void ApplyDamageToPlayer(GameObject player)
        {
            UHealthComponent playerHealth = player.GetComponent<UHealthComponent>();
            if (playerHealth != null)
            {
                playerHealth.Modify(ExplosionDamage);
            }
        }
    }
}