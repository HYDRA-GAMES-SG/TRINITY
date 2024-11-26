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
    public Transform VictimTarget;
    public ACrabFSM CrabFSM;

    [SerializeField] float MaxPursueRangeLength = 35;
    [SerializeField] float MinPursueRangeLength = 25;
    [SerializeField] float AggroRange;

    private Rigidbody Physics;
    private CapsuleCollider Collider;
    private NavMeshAgent AI;
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
        if (CrabFSM == null || CrabFSM.CurrentState == null) return;

        if (VictimTarget == null) return;
        float distanceToTarget = Vector3.Distance(VictimTarget.position, transform.position);
        if (distanceToTarget > AI.stoppingDistance)
        {
            CrabFSM.EnqueueTransition<Pursue>();
        }
    }
}
