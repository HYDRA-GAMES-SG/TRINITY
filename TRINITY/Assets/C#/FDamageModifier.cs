using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public struct FDamageModifier
{
    public float Damage;
    public int StatusStacks;
    public EAilmentType AilmentType;
    public FDamageModifier(float targetHealth, EAilmentType ailmentType , int targetAilmentStacks)
    {
        //Things included inside damage
        Damage = targetHealth;
        StatusStacks = targetAilmentStacks;
        AilmentType = ailmentType;
    }

    public static UEnemyEntity operator +(UEnemyEntity enemy, FDamageModifier damageSource) 
    {
        enemy.Health.Current -= damageSource.Damage;    
        enemy.Ailment.ModifyStack(damageSource.AilmentType,damageSource.StatusStacks);
        Debug.Log($"Damage Taken : {damageSource.Damage}, Ailment type & stacks applied : {damageSource.AilmentType},{damageSource.StatusStacks}");
        return enemy;
    }
       
}
