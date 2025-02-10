using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemOrb : AProjectile
{

    [Header("Projectile Properties")] 
    private int ChargeStacks;
    private float ProjectileSpeed;
    private float Range;
    private float Damage;
    public bool bEnraged;

    [HideInInspector]
    private Transform TargetTransform;

    private Vector3 InitialPosition;
    private Vector3 InitialTargetPosition;
    private Vector3 Direction;

    private AudioSource OrbSource;
    public GameObject HitSFX;
    public void SetTarget(Transform newTarget)
    {
        print("set target");
        TargetTransform = newTarget;
        InitialTargetPosition = TargetTransform.position;
        Direction = (InitialTargetPosition - transform.position).normalized;
    }

    private void Awake()
    {
        OrbSource = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        ASecondaryLightning secondaryLightning = ATrinityGameManager.GetSpells().SecondaryLightning;
        ChargeStacks = secondaryLightning.ProjectileChargeStacks;
        Range = secondaryLightning.ProjectileRange;
        ProjectileSpeed = secondaryLightning.ProjectileSpeed;
        Damage = secondaryLightning.ProjectileDamage;
        InitialPosition = transform.position;
    }

    public override void Despawn()
    {
        Destroy(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        if(TargetTransform != null)
        {
          
            // Calculate direction to target
            
            // Move towards target
            transform.position += Direction * (ProjectileSpeed * Time.deltaTime);
        }
        
        if (Vector3.Distance(transform.position, InitialTargetPosition) > Range)
        {
            Despawn();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        print(other.gameObject.name);
        if (other.gameObject.GetComponent<LightningTotem>()) 
        {
            return;
        }
        if (other.gameObject.GetComponent<TotemOrb>()) 
        {
            other.gameObject.GetComponent<TotemOrb>().Despawn();
            Despawn();
            return;
        }
        
        UEnemyColliderComponent enemyCollider = other.gameObject.GetComponent<UEnemyColliderComponent>();
       
        if(enemyCollider)
        {
            UEnemyStatusComponent enemyStatus = enemyCollider.EnemyStatus;
            FDamageInstance damageSource = new FDamageInstance(Damage, EAilmentType.EAT_Charge, ChargeStacks);
            enemyStatus += damageSource;
            Despawn();
        }
        else
        {
            //assume we are hitting something default etc
            Despawn();
        }

        GameObject hitSFX = Instantiate(HitSFX, other.transform.position, Quaternion.identity);
        AudioSource hitSource = hitSFX.GetComponent<AudioSource>();
        Destroy(hitSFX, hitSource.clip.length);

    }
}
