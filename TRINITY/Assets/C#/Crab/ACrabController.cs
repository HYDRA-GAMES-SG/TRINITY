using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI;
using UnityEngine.AI;
using Unity.VisualScripting;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(UHealthComponent))]
public class ACrabController : MonoBehaviour
{
    public ATrinityController PlayerController;
    public ACrabFSM CrabFSM;

    //[SerializeField] float MaxPursueRange = 35;
    //[SerializeField] float MinPursueRange = 25;
    //[SerializeField] float AggroRange;

    [HideInInspector]
    public Rigidbody Physics;
    [HideInInspector]
    public CapsuleCollider Collider;
    public NavMeshAgent AI;
    private UHealthComponent Health;
    void Start()
    {
        Physics = GetComponent<Rigidbody>();
        Physics.isKinematic = transform;

        Collider = GetComponent<CapsuleCollider>();
        Collider.enabled = false;

        AI = GetComponent<NavMeshAgent>();

        Health = GetComponent<UHealthComponent>();
    }

    void Update()
    {
        
    }
}
