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
    [HideInInspector]
    public Rigidbody RB;

    [HideInInspector]
    public Animator Animator;
    
    [HideInInspector]
    public UEnemyStatus EnemyStatus;
    
    [HideInInspector]
    public NavMeshAgent AI;

    public void Initialize()
    {
        AI = GetComponent<NavMeshAgent>();
        RB = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        EnemyStatus = GetComponent<UEnemyStatus>();
    }
}
