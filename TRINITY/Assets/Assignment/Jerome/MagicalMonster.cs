using JetBrains.Annotations;
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
        public float IceBreathTimer;
        public float IceBreathWaitTime;
        public float FireBeamTimer;
        public float FireBeamWaitTime;
        public float ExplosionTimer;
        public float ExplosionWaitTime;
        public float WarpTimer;
        public float WarpWaitTime;

        public float AggroRange;
        private Transform InitialPosition;

        public GameObject Player;
        private string _currentstate;
        private Animator MonsterController;
        private NavMeshAgent MonsterAgent;

        [SerializeField]
        private ETrinityElement CurrentElement;
        [SerializeField]
        private ETrinityElement PreviousElement;

        public GameObject IceVFX;
        public GameObject IceBreathVFX;
        public Transform IceBreathPos;
        public float IceBreathRange;

        public GameObject FireBeamVFX;
        public Transform FireBeamPos;
        public GameObject ExplosionVFX;
        public float ExplosionRange;

        public GameObject WarpVFX;
        public GameObject ElectricRainVFX;

        const string CRAWL = "CrawlForward";
        const string ICESURFING = "Swim";
        const string ROAR = "Roar";
        const string FIREBEAM = "StingerAttack";
        const string IDLE = "Idle";

        private void Awake()
        {
            MonsterAgent = GetComponent<NavMeshAgent>();
            MonsterController = GetComponent<Animator>();
            InitialPosition = transform;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float distanceFromPlayer = (transform.position - Player.transform.position).magnitude;

            AITimer -= Time.deltaTime;
            if (AITimer < 0)
            {
                if (distanceFromPlayer > AggroRange)
                {
                    MonsterAgent.destination = InitialPosition.position;
                    if (HasFinishMoving()) 
                    {
                        ChangeAnimationState(IDLE);
                    }
                    else 
                    {
                        ChangeAnimationState(CRAWL);
                    }
                    return;
                }
                if (ATrinityGameManager.GetBrain().GetElement() == ETrinityElement.ETE_Fire)
                {
                    FireBeamTimer -= Time.deltaTime;
                    ExplosionTimer -= Time.deltaTime;
                    if (FireBeamTimer < 0)
                    {
                        GameObject firebeam = Instantiate(FireBeamVFX, FireBeamPos.position, Quaternion.identity);
                        firebeam.transform.LookAt(Player.transform.position);
                        FireBeamTimer = FireBeamWaitTime;
                        ChangeAnimationState(FIREBEAM);
                    }
                    if (ExplosionTimer < 0 && distanceFromPlayer <= ExplosionRange)
                    {
                        Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
                        ExplosionTimer = ExplosionWaitTime;
                        ChangeAnimationState(ROAR);
                    }
                }

                if (ATrinityGameManager.GetBrain().GetElement() == ETrinityElement.ETE_Cold)
                {
                    IceBreathTimer -= Time.deltaTime;
                    MonsterAgent.destination = Player.transform.position;
                    ChangeAnimationState(ICESURFING);
                    IceVFX.SetActive(true);
                    if (IceBreathTimer < 0 && distanceFromPlayer < IceBreathRange)
                    {
                        GameObject iceBreath = Instantiate(IceBreathVFX, IceBreathPos.position, Quaternion.identity);
                        iceBreath.transform.rotation = Quaternion.LookRotation(transform.forward,Vector3.up);
                        IceBreathTimer = IceBreathWaitTime;
                    }
                }
                else
                {
                    IceVFX.SetActive(false);
                }

                if (ATrinityGameManager.GetBrain().GetElement() == ETrinityElement.ETE_Lightning)
                {
                    WarpTimer -= Time.deltaTime;
                    if (WarpTimer < 0)
                    {
                        float rng = Random.Range(0, 2);
                        float xRandom;
                        float zRandom;
                        if (rng < 1)
                        {
                            xRandom = Random.Range(5, 11);
                            zRandom = Random.Range(5, 11);
                        }
                        else
                        {
                            xRandom = Random.Range(-5, -11);
                            zRandom = Random.Range(-5, -11);
                        }
                        Vector3 warpPosOffset = new(xRandom, 0 , zRandom);
                        MonsterAgent.Warp(Player.transform.position + warpPosOffset);
                        Instantiate(WarpVFX,transform.position, Quaternion.identity);
                        Instantiate(ElectricRainVFX,transform.position, Quaternion.identity);
                        ChangeAnimationState(ROAR);
                        WarpTimer = WarpWaitTime;
                    }
                }

            }
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
