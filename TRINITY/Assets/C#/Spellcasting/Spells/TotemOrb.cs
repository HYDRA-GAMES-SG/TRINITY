using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemOrb : MonoBehaviour
{

    [Header("Projectile Properties")] private int ChargeStacks;
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
        
        HitBox collisionHitBox = other.gameObject.GetComponent<HitBox>();
        
        if (!collisionHitBox) //if null
        {
            Destroy(this);
            return;
        }

        collisionHitBox.Health.Modify(-Damage);
        collisionHitBox.EnemyStatus.Ailments.ModifyStack(EAilmentType.EAT_Charge, ChargeStacks);
        Destroy(this);
    }
}
