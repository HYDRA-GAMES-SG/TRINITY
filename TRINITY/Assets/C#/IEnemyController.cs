using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UEnemyStatus))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class IEnemyController : MonoBehaviour
{
    public string Name = "Default";
    
    [HideInInspector]
    public Rigidbody RB;

    [HideInInspector]
    public Animator Animator;
    
    [HideInInspector]
    public UEnemyStatus EnemyStatus;
    
    [HideInInspector]
    public NavMeshAgent AI;
    
    public float NormalAttack;
    

    public float AttackForce = 150f;

    public bool bDead => EnemyStatus.Health.bDead;
    
    private void Awake()
    {
        AI = GetComponent<NavMeshAgent>();
        RB = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        EnemyStatus = GetComponent<UEnemyStatus>();
    }
    
    public virtual void TriggerGetHit()
    {
        
    }

    public virtual float GetCurrentAttackDamage()
    {
        return NormalAttack;
    }

    public virtual float GetParticleAttack()
    {
        return 0f;
    }
}
