using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABulletSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")] 
        
        public UBulletPool BulletPool;

        public IEnemyController EnemyController;

        [Header("Ring Settings")] [Tooltip("Is the spawner aimed at the player?")]
        public bool bIsAimed = true;

        [Tooltip("Velocity of the spawner.")] 
        public Vector3 Velocity = Vector3.zero;

        [Tooltip("Rotation of the spawner.")]
        public Vector3 Rotation = Vector3.zero;
        
        [Tooltip("Position offset of the ring.")]
        public Vector3 Offset = Vector3.zero;

        [Tooltip("Rotation of the ring.")] 
        public Vector3 SpawnRotation = Vector3.zero;

        [Tooltip("Number of Bullets Including Skipped Bulllets.")] 
        public int nBullets = 0;

        [Header("Advanced")]
        [Tooltip("Number of times the pattern should repeat, 0 means it will not repeat at all.")]
        public int NumRepeats = 0;

        [Tooltip("Skips spawning every n objects")]
        public int SkipInterval = 0;

        [Tooltip("When skipping, how many objects to skip")]
        public int SkipStrideAmt = 1;

        [Header("Timer Settings")] [Tooltip("Time between spawn calls.")]
        public float CooldownTime = 1f;

        [Tooltip("Delay between each spawned objects.")]
        public float SpawnDelay = 0f;

        [Tooltip("Time between repeats.")] 
        public float RepeatDelay = .1f;

        [Tooltip("Time before the spawner becomes alive.")]
        public float StartDelay = 0f;

        



        //*****************
        //private fields
        //*****************
        protected Coroutine SpawnCoro;
        protected bool bIsSpawning = false;
        protected float CurrentCooldown = 0f;
        protected int SkipStride;
        
        
        /// <summary>
        /// Events
        /// </summary>
        public System.Action OnSpawnFinished;


        protected virtual void Awake()
        {
            NumRepeats++; //repeats should be 1 higher than the user inputs since we need to spawn at least once
            SkipStride = SkipStrideAmt;
        }
        
        protected virtual void Start()
        {
            if (BulletPool == null)
            {
                BulletPool = gameObject.GetComponent<UBulletPool>();
            }
            
        }

        protected void Update()
        {
            if (ATrinityManager.GetPlayer() == null)
            {
                return;
            }
            
            if (Time.time > StartDelay)
            {
                HandleSpawning();
            }
        }

        private void HandleSpawning()
        {
            if (EnemyController != null && EnemyController.EnemyStatus.Health.Current <= 0)
            {
                return;
            }
            
            if (!bIsSpawning) //if we aren't spawning
            {
                CurrentCooldown -= Time.deltaTime;

                if (CurrentCooldown <= 0f) //cooldown is complete
                {
                    StartCoroutine(nameof(Spawn));
                }
            }
        }

        protected virtual IEnumerator Spawn()
        {
            int bulletsSpawnedThisFrame = 0;
            int bulletsPerFrame = Mathf.Clamp(Mathf.RoundToInt(nBullets / (nBullets * SpawnDelay) * Time.deltaTime), 1, Int32.MaxValue);
            
            List<ABullet> WaitingBullets = new List<ABullet>();
            bool bSkipping = false;
            bIsSpawning = true;

            InitializeSpawner();

            for (int i = 0; i < NumRepeats; i++)
            {
                if (!bIsSpawning)
                {
                    break;
                }

                for (int j = 0; j < nBullets; j++)
                {
                    
                    if (SkipInterval > 0 && (j + 1) % SkipInterval == 0)
                    {
                        bSkipping = true; //set skip flag
                    }

                    if (bSkipping)
                    {
                        SkipStride--;

                        if (SkipStride <= 0)
                        {
                            bSkipping = false; //reset skip flag once skip stride is over
                        }

                        continue;
                    }

                    Vector3 spawnLocation = GetBulletSpawnLocation(j);
                    Quaternion spawnRotation = Quaternion.Euler(GetBulletSpawnRotation(j));
                    
                    if (SpawnDelay > 0 && bulletsSpawnedThisFrame == bulletsPerFrame)
                    {
                        yield return new WaitForSeconds(SpawnDelay);
                        bulletsSpawnedThisFrame = 0;
                    }
                        
                    if (!bIsSpawning)
                    {
                        continue;
                    }
                    
                    
                    GameObject spawnedObject = BulletPool.GetBullet();
                    bulletsSpawnedThisFrame++;
                    spawnedObject.transform.position = spawnLocation;
                    spawnedObject.transform.rotation = spawnRotation;
                    
                    //if we are spawning a bullet
                    if (spawnedObject.GetComponent<ABullet>() != null)
                    {
                        ABullet spawnedBullet = spawnedObject.GetComponent<ABullet>();
                        
                        if (spawnedBullet.Lifetime != 0)
                        {
                            StartCoroutine(DeactivateBulletAfterTime(spawnedBullet.gameObject, spawnedBullet.Lifetime));
                        }
                        spawnedBullet.SetSpawner(this.gameObject);
                        spawnedBullet.SetInitialValues();

                        if (spawnedBullet.bWaitForSpawner)
                        {
                            WaitingBullets.Add(spawnedBullet);
                        }
                    }

                    HandleModifiers();
                }

                foreach (ABullet b in WaitingBullets)
                {
                    b.EnableMotion();
                }

                WaitingBullets.Clear();
                Reset();
                SkipStride = SkipStrideAmt;
                yield return new WaitForSeconds(RepeatDelay);
            }

            OnSpawnFinished?.Invoke();
            bIsSpawning = false;
            CurrentCooldown = CooldownTime;
        
        }

        private IEnumerator DeactivateBulletAfterTime(GameObject bullet, float time)
        {
            yield return new WaitForSeconds(time);
            BulletPool.ReturnBullet(bullet);
        }

        
        protected virtual Vector3 GetBulletSpawnRotation(int currenetBulletNumber)
        {
            if (bIsAimed)
            {
                Vector3 directionToGhost = transform.position - ATrinityManager.GetPlayer().transform.position;
                if (directionToGhost != Vector3.zero)
                {
                    return Quaternion.LookRotation(directionToGhost).eulerAngles;
                }
            }

            return transform.forward;
        }

        protected virtual Vector3 GetBulletSpawnLocation(int currentBulletNumber)
        {
            return transform.position + transform.forward * 2f;
        }

        protected virtual void InitializeSpawner()
        {
        }

        protected virtual void HandleModifiers()
        {
        }

        protected virtual void Reset()
        {
            
        }

        void OnDrawGizmos()
        {
            DrawGizmos();
          
        }

        protected virtual void DrawGizmos()
        {
            
        }
    }