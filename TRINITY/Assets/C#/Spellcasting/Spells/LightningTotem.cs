using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightningTotem : MonoBehaviour
{
    [Header("Totem Properties")] 
    public float AttackFrequency = .85f;
    public float Duration;
    public Vector3 InvokePosition;
    public float SummonDepth;
    public bool bUnsummoned = false;
    public bool bSummoned = false;
    public float UnsummonSpeed = 1.5f;
    public float MaxPitchSpawn = 10f;
    public Transform ProjectileSpawnPoint;
    private float AttackTimer;
    private Transform TargetEnemy;
    
    
    // Start is called before the first frame update
    void Start()
    {
        bUnsummoned = false;
        Vector3 toPlayer = ATrinityGameManager.GetPlayerController().Position - transform.position;
        toPlayer.y = 0; //flatten
        
        transform.Find("Totem").transform.localRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        System.Random RNG = new System.Random();
        float spawnPitch = ((float)RNG.NextDouble() - .5f) * 2f * MaxPitchSpawn;
        transform.Find("Totem").transform.localRotation *= Quaternion.AngleAxis(spawnPitch, transform.forward); //makes no sense
        AttackTimer = AttackFrequency;
    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;
        AttackTimer -= Time.deltaTime;
        
        if (AttackTimer < 0 && !bUnsummoned)
        {
            Attack();
            AttackTimer = AttackFrequency;
        }
        
        if (Duration <= 0)
        {
            Unsummon();
        }
        
        if (bSummoned  && !bUnsummoned)
        {
            LookAtClosestEnemy();
            
        }
        
        if (bUnsummoned)
        {
            float newY = Mathf.Lerp(transform.position.y, InvokePosition.y - SummonDepth, UnsummonSpeed * Time.deltaTime);
            Vector3 newPos = new Vector3(InvokePosition.x, newY, InvokePosition.z);
            transform.localPosition = newPos;

            if (Mathf.Abs(newY - InvokePosition.y - SummonDepth) < .05f)
            {
                Destroy(this);
            }
        }
    }

    private void Attack()
    {
        if (TargetEnemy != null)
        {
            GameObject orbPrefab = Instantiate(ATrinityGameManager.GetSpells().SecondaryLightning.ProjectilePrefab,
                ProjectileSpawnPoint.position, Quaternion.identity);
            TotemOrb totemOrb = orbPrefab.GetComponent<TotemOrb>();
            totemOrb.transform.SetParent(this.transform);
            totemOrb.SetTarget(TargetEnemy);
            Destroy(totemOrb, ATrinityGameManager.GetSpells().SecondaryLightning.ProjectileDuration);
        }
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
        //lerp the totem back underground
        bUnsummoned = true;
    }
}
