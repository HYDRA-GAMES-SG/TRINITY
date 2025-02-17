using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightningTotem : MonoBehaviour
{
    [HideInInspector]
    public ELightningTotemStatus Status;
    [Header("Totem Properties")] 
    public float AttackFrequency = .85f;
    public float EnragedAttackFrequency = 1.2f;
    public float Duration;
    public Vector3 InvokePosition;
    public float SummonDepth;
    public float UnsummonSpeed = 1.5f;
    public float MaxPitchSpawn = 10f;
    public Transform ProjectileSpawnPoint;
    private float AttackTimer;
    private Transform TargetEnemy;
    private GameObject EnrageFX;
    public bool bCanFire;
    public bool bTutorial = false;
    
    private Light[] EyeLights;

    public GameObject EnragedOrbPrefab;

    public GameObject NormalVFX;

    private AudioSource TotemSource;
    public AudioClip UnsummonSFX;
    public AudioClip[] FireSFX;


    // Start is called before the first frame update
    void Start()
    {
        TotemSource = GetComponent<AudioSource>();
        TotemSource.clip = UnsummonSFX;
        Status = ELightningTotemStatus.ELTS_Summoning;
        Vector3 toPlayer = ATrinityGameManager.GetPlayerController().Position - transform.position;
        toPlayer.y = 0; //flatten
        
        transform.Find("Totem").transform.localRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        System.Random RNG = new System.Random();
        float spawnPitch = ((float)RNG.NextDouble() - .5f) * 2f * MaxPitchSpawn;
        transform.Find("Totem").transform.localRotation *= Quaternion.AngleAxis(spawnPitch, transform.forward); //makes no sense
        EnrageFX = transform.Find("EnrageFX").gameObject;
        EnrageFX.SetActive(false);
        AttackTimer = AttackFrequency;
        EyeLights = GetComponentsInChildren<Light>();
        NormalVFX.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY) 
        {
            return;
        }
        if (bTutorial) 
        {
            Status = ELightningTotemStatus.ELTS_Summoned;
            return;
        }
        if (Status == ELightningTotemStatus.ELTS_Unsummoned)
        {
            TotemSource.Play();
            float newY = Mathf.Lerp(transform.position.y, InvokePosition.y - SummonDepth, UnsummonSpeed * Time.deltaTime);
            Vector3 newPos = new Vector3(InvokePosition.x, newY, InvokePosition.z);
            transform.localPosition = newPos;

            if (Mathf.Abs(newY - InvokePosition.y - SummonDepth) < .05f)
            {
                Destroy(this.gameObject);
            }

            return;
        }
        
        Duration -= Time.deltaTime;
        AttackTimer -= Time.deltaTime;
        
        if (AttackTimer < 0 && (Status == ELightningTotemStatus.ELTS_Summoned || Status == ELightningTotemStatus.ELTS_Enraged))
        {
            Attack();
            if (Status == ELightningTotemStatus.ELTS_Enraged)
            {
                AttackTimer = EnragedAttackFrequency;
            }
            else
            {
                AttackTimer = AttackFrequency;
            }
        }
        
        if (Duration <= 0)
        {
            Unsummon();
        }
        
        if (Status != ELightningTotemStatus.ELTS_Unsummoned)
        {
            LookAtClosestEnemy();
            
        }
        
    }

    private void Enrage()
    {
        if (Status == ELightningTotemStatus.ELTS_Summoned)
        {
            Status = ELightningTotemStatus.ELTS_Enraged;
            float attackProgress = AttackTimer / AttackFrequency;

            AttackTimer = attackProgress * EnragedAttackFrequency;
            EnrageFX.SetActive(true);
            NormalVFX.SetActive(false);
            transform.Find("Totem").gameObject.GetComponent<MeshRenderer>().material.SetVector("_EmissionColor", new Vector4(.34f, .94f, .96f, 1f));
            
            foreach (Light light in EyeLights)
            {
                //light.color = Color.red;
                light.transform.parent.GetComponent<MeshRenderer>().material.SetVector("_EmissionColor", new Vector4(1f, 0, 0, 1f));
            }
        }
        
    }

    private void Attack()
    {

        if(TargetEnemy == null)
        {
            return;
        }

        if (!bCanFire) 
        {
            return;
        }
        GameObject orbPrefab = null;

        switch(Status)
        {
            case ELightningTotemStatus.ELTS_Summoned:
                orbPrefab = Instantiate(ATrinityGameManager.GetSpells().SecondaryLightning.ProjectilePrefab,
                ProjectileSpawnPoint.position, Quaternion.identity);
                break;
            case ELightningTotemStatus.ELTS_Enraged:
                orbPrefab = Instantiate(EnragedOrbPrefab,
                   ProjectileSpawnPoint.position, Quaternion.identity);
                orbPrefab.GetComponent<TotemOrb>().bEnraged = true;
                break;
            default:
               
                break;
        }
        TotemOrb totemOrb = orbPrefab.GetComponent<TotemOrb>();
        totemOrb.transform.SetParent(this.transform);
        totemOrb.SetTarget(TargetEnemy);
        Destroy(totemOrb.gameObject, ATrinityGameManager.GetSpells().SecondaryLightning.ProjectileDuration);

        //int rng = UnityEngine.Random.Range(0, FireSFX.Length);
        //TotemSource.PlayOneShot(FireSFX[rng]);
        
    }

    private void LookAtClosestEnemy()
    {
        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            float distanceToClosestEnemy = float.MaxValue;
            IEnemyController closestEnemy = null;
            foreach (IEnemyController ec in ATrinityGameManager.GetEnemyControllers())
            {
                if (ec.EnemyStatus.Health.bInvulnerable || ec.EnemyStatus.Health.bDead)
                {
                    continue;
                }
                float distance = Vector3.Distance(transform.position, ec.transform.position);
                if (distance < distanceToClosestEnemy)
                {
                    distanceToClosestEnemy = distance; // Add this line
                    closestEnemy = ec;
                }
            }

            if (closestEnemy != null)
            {
                TargetEnemy = closestEnemy.CoreCollider.transform;
                Vector3 toEnemy = closestEnemy.CoreCollider.transform.position - transform.position;
                toEnemy.y = 0; // flatten
                Transform totem = transform.Find("Totem");
                totem.localRotation = Quaternion.Lerp(totem.localRotation, 
                    Quaternion.LookRotation(toEnemy, Vector3.up), 
                    2f * Time.deltaTime);
            }

            if (closestEnemy != null)
            {
                if (closestEnemy.gameObject.GetComponent<UHealthComponent>().bInvulnerable)
                {
                    bCanFire = false;
                }
                else
                {
                    bCanFire = true;
                }
            }
        }
    }

    public void Unsummon()
    {
        Status = ELightningTotemStatus.ELTS_Unsummoned;

        if (NormalVFX != null)
        {
            NormalVFX.SetActive(false);
        }

        if (EnrageFX != null)
        {
            EnrageFX.SetActive(false);
        }

        ATrinityGameManager.GetSpells().SecondaryLightning.GetTotems().Remove(this.gameObject);
        Destroy(this.gameObject, 1f);
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<LightningBolt>())
        {
            Enrage();
        }
    }

    public void Summoned()
    {
        Status = ELightningTotemStatus.ELTS_Summoned;
        NormalVFX.SetActive(true);
    }
}
