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
    
    private Light[] EyeLights;

    public GameObject EnragedOrbPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        Status = ELightningTotemStatus.ELTS_Summoning;
        Vector3 toPlayer = ATrinityGameManager.GetPlayerController().Position - transform.position;
        toPlayer.y = 0; //flatten
        
        transform.Find("Totem").transform.localRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        System.Random RNG = new System.Random();
        float spawnPitch = ((float)RNG.NextDouble() - .5f) * 2f * MaxPitchSpawn;
        transform.Find("Totem").transform.localRotation *= Quaternion.AngleAxis(spawnPitch, transform.forward); //makes no sense
        EnrageFX = transform.Find("EnrageFX").gameObject;
        AttackTimer = AttackFrequency;
        EyeLights = GetComponentsInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY) 
        {
            return;
        }
        if (Status == ELightningTotemStatus.ELTS_Unsummoned)
        {
            float newY = Mathf.Lerp(transform.position.y, InvokePosition.y - SummonDepth, UnsummonSpeed * Time.deltaTime);
            Vector3 newPos = new Vector3(InvokePosition.x, newY, InvokePosition.z);
            transform.localPosition = newPos;

            if (Mathf.Abs(newY - InvokePosition.y - SummonDepth) < .05f)
            {
                Destroy(this);
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
            
            transform.Find("Totem").gameObject.GetComponent<MeshRenderer>().material.SetVector("_BaseColor", new Vector4(.6f, .76f, .58f, 1f));
            
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
                break;
            default:
               
                break;
        }
        TotemOrb totemOrb = orbPrefab.GetComponent<TotemOrb>();
        totemOrb.transform.SetParent(this.transform);
        totemOrb.SetTarget(TargetEnemy);
        Destroy(totemOrb.gameObject, ATrinityGameManager.GetSpells().SecondaryLightning.ProjectileDuration);
        
    }

    private void LookAtClosestEnemy()
    {
        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            float distanceToClosestEnemy = float.MaxValue;
            IEnemyController closestEnemy = null;
            foreach (IEnemyController ec in ATrinityGameManager.GetEnemyControllers())
            {
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
        }
    }

    public void Unsummon()
    {
        Status = ELightningTotemStatus.ELTS_Unsummoned;
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
    }
}
