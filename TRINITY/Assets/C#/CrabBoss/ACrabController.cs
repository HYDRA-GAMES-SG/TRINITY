using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI;
using UnityEngine.AI;
using Unity.VisualScripting;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(ACrabFSM))]

public class ACrabController : MonoBehaviour
{
    public Transform VictimTarget;

    private ACrabFSM fsm;
    private Rigidbody rb;
    private CapsuleCollider CapCollider;
    private NavMeshAgent CrabAgent;

    [SerializeField]
    float MaxPursueRangeLength = 35,
          MinPursueRangeLength = 25;
    void Start()
    {
        fsm = GetComponent<ACrabFSM>();
        rb = GetComponent<Rigidbody>();
        CapCollider = rb.GetComponent<CapsuleCollider>();
        GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (VictimTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, VictimTarget.position);

            if (distanceToTarget >= MinPursueRangeLength && distanceToTarget <= MaxPursueRangeLength)
                fsm.EnqueueTransition("Pursue");
            else if (distanceToTarget < MinPursueRangeLength)
                fsm.EnqueueTransition("Attack");
            else
                fsm.EnqueueTransition("Idle");
            
        }
    }
}
