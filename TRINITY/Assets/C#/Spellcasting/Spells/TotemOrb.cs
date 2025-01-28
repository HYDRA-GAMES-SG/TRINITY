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
    private Transform TargetTransform;
    private Vector3 InitialPosition;
    private Vector3 InitialTargetPosition;
    private Vector3 Direction;


    public void SetTarget(Transform newTarget)
    {
        TargetTransform = newTarget;
        InitialTargetPosition = TargetTransform.position;
        Direction = (InitialTargetPosition - transform.position).normalized;
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
        
        if (Vector3.Distance(transform.position, InitialPosition) > Range)
        {
            Destroy(this);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        print(other.gameObject.name);

        if (other.gameObject.name.Contains("LightningOrb")) 
        {
            Destroy(other.gameObject);
            Destroy(this.gameObject);
            return;
        }
        
        UEnemyColliderComponent enemyCollider = other.gameObject.GetComponent<UEnemyColliderComponent>();
       
        if (!enemyCollider) //if null
        {
            Destroy(this);
            return;
        }
        if (other.gameObject.tag == "Enemy") 
        {
            UEnemyStatusComponent enemyStatus = enemyCollider.EnemyStatus;
            FDamageInstance damageSource = new FDamageInstance(Damage, EAilmentType.EAT_Charge, ChargeStacks);
            enemyStatus += damageSource;
            Destroy(this.gameObject);
        }
    }
}
