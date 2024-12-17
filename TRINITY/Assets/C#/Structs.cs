using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public struct FHitInfo
{
    public GameObject Attacker;
    public Collision CollisionData;
    public float Damage;

    public FHitInfo(GameObject attacker, Collision collision, float damage)
    {
        Attacker = attacker;
        CollisionData = collision;
        Damage = damage;
    }
    
}

public struct FDamageInstance
{
    public float Damage;
    public int StatusStacks;
    public EAilmentType AilmentType;
    public FDamageInstance(float targetHealth, EAilmentType ailmentType , int targetAilmentStacks)
    {
        //Things included inside damage
        Damage = targetHealth;
        StatusStacks = targetAilmentStacks;
        AilmentType = ailmentType;
    }

    public static UEnemyStatus operator +(UEnemyStatus enemy, FDamageInstance damageSource) 
    {
        enemy.Health.Modify(damageSource);
        enemy.Ailment.ModifyStack(damageSource.AilmentType,damageSource.StatusStacks);
        Debug.Log($"Damage Taken : {damageSource.Damage}, Ailment type & stacks applied : {damageSource.AilmentType},{damageSource.StatusStacks}");
        return enemy;
    }
       
}
