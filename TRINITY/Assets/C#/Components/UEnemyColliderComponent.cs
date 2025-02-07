using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UEnemyColliderComponent : MonoBehaviour
{
    [HideInInspector] 
    public Rigidbody RB;
    [HideInInspector] 
    public Collider Collider;
    [HideInInspector]
    public UAilmentComponent Ailments;
    [HideInInspector]
    public UHealthComponent Health;
    [HideInInspector]
    public UEnemyStatusComponent EnemyStatus;
    //[HideInInspector]
    public IEnemyController EnemyController;

    public void Start()
    {
        EnemyController = transform.root.GetComponentInChildren<IEnemyController>();
        EnemyStatus = EnemyController.EnemyStatus;
        Ailments = EnemyController.EnemyStatus.Ailments;
        Health = EnemyStatus.Health;
        Collider = GetComponent<Collider>();
        RB = GetComponent<Rigidbody>();
    }
}
