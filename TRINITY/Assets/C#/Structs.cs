using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public struct FScoreLimits
{
    public string SceneName { get; set; }
    public float BestTime { get; set; }
    public float WorstTime { get; set; }
    public float BestDamageTaken { get; set; }
    public float WorstDamageTaken { get; set; }

    /* Constructor with default values using optional parameters */
    public FScoreLimits(
        string sceneName = "", 
        float bestTime = 120f, 
        float worstTime = 240f, 
        float bestDamageTaken = 0f, 
        float worstDamageTaken = 100f)
    {
        SceneName = sceneName;
        BestTime = bestTime;
        WorstTime = worstTime;
        BestDamageTaken = bestDamageTaken;
        WorstDamageTaken = worstDamageTaken;
    }
}

public struct FGameSettings
{
    public bool bCrossHairEnabled;
    public float GamepadSensitivity;
    public float MouseSensitivity;
    public float MasterVolume;

    public FGameSettings(bool bCrossHair, float gamepadSensitivity, float mouseSensitivity, float masterVolume)
    {
        bCrossHairEnabled = bCrossHair;
        GamepadSensitivity = gamepadSensitivity;
        MouseSensitivity = mouseSensitivity;
        MasterVolume = masterVolume;
    }
}

public struct FHitInfo
{
    public GameObject Attacker;
    public GameObject CollidingObject;
    public Collision CollisionData;
    public float Damage;

    public FHitInfo(GameObject attacker, GameObject collidingObject, Collision collision, float damage)
    {
        Attacker = attacker;
        CollidingObject = collidingObject;
        CollisionData = collision;
        Damage = damage;
    }
    
}

public struct FCameraShakeInfo
{
    public float NormalizedIntensity;
    public float Duration;

    public FCameraShakeInfo(float normalizedIntensity, float duration)
    {
        NormalizedIntensity = normalizedIntensity;
        Duration = duration;
    }
}

public struct FDamageInstance
{
    public float Damage;
    public int StatusStacks;
    public EAilmentType AilmentType;
    
    public FDamageInstance(float damage, EAilmentType ailmentType , int targetAilmentStacks)
    {
        //Things included inside damage
        Damage = damage;
        StatusStacks = targetAilmentStacks;
        AilmentType = ailmentType;
    }

    public static UEnemyStatusComponent operator +(UEnemyStatusComponent enemy, FDamageInstance damageSource) 
    {
        enemy.Health.Modify(damageSource);
        enemy.Ailments.ModifyStack(damageSource.AilmentType,damageSource.StatusStacks);
        
        //Debug.Log($"Damage Taken : {damageSource.Damage}, Ailment type & stacks applied : {damageSource.AilmentType},{damageSource.StatusStacks}");
        return enemy;
    }
       
}
