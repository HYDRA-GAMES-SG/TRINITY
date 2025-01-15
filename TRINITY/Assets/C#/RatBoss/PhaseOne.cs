using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum ERatMovementState
{
    ERMS_Dead,
    ERMS_Reposition,
    ERMS_Idle
}

public class PhaseOne : RatState
{
    public float ThresholdAngle = 30f;
    public float WanderRadius = 10f;
    public ERatMovementState RatMovementState;
    public ABulletRingSpawner RingSpawner;
    public ABulletSphereSpawner SphereSpawner;
    private Dictionary<string, string> AnimKeys = new Dictionary<string, string>
    {
        { "Move", "Forward" }, { "Turn", "Turn" },{ "AcidWave", "AcidWave" }, { "AcidSphere", "AcidSphere" },
        {"Death", "bDeath"}, {"Idle", "bIdle"}, {"WaveTrigger", "WaveTrigger"}, {"SphereTrigger", "SphereTrigger"}
    };
    private Coroutine SpawnCoro;
    private bool bSpawningAcid = false;


    private int SpawnerFlipFlop; // modulo 3
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return !(fromState is Death);
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        if (RatFSM.PlayerController == null)
        {
            Debug.Log("No Player Found on RatState: " + this.name);
            return;
        }

        if (RatFSM.RatController == null)
        {
            Debug.Log("No Rat Ctrlr found on RateState:" + this.name);
            return;
        }

        RatMovementState = ERatMovementState.ERMS_Idle;

    }

    public override void PreUpdateBehaviour(float dt)
    {
        if (RatFSM.RatController.EnemyStatus.Health.Current <= 0f)
        {
            RatMovementState = ERatMovementState.ERMS_Dead;
        }
    }


    public override void UpdateBehaviour(float dt)
    {
        switch (RatMovementState)
        {
            case ERatMovementState.ERMS_Idle:
                if (!bSpawningAcid && SpawnCoro == null)  // Only spawn if we're not already spawning
                {
                    SpawnAcid();
                }
                break;
            
            case ERatMovementState.ERMS_Reposition:
                if (!RatFSM.RatController.AI.hasPath)
                {
                    // If we don't have a path, get a new wander point
                    MoveTowardTarget(GetWanderPoint());
                    RatFSM.Animator.SetFloat(AnimKeys["Move"], Mathf.Abs(RatFSM.RatController.RB.velocity.magnitude));
                }
                else 
                {
                    // Check if we've reached our destination
                    if (RatFSM.RatController.AI.remainingDistance <= RatFSM.RatController.AI.stoppingDistance)
                    {
                        print("Reached destination, returning to idle");
                        RatFSM.RatController.AI.ResetPath(); // Clear the path
                        RatMovementState = ERatMovementState.ERMS_Idle;
                        RatFSM.Animator.SetBool(AnimKeys["Idle"], true);
                    }
                    else 
                    {
                        // Still moving, update animation
                        RatFSM.Animator.SetFloat(AnimKeys["Move"], Mathf.Abs(RatFSM.RatController.RB.velocity.magnitude));
                    }
                }
                break;
            
            case ERatMovementState.ERMS_Dead:
                RatFSM.Animator.SetBool(AnimKeys["Death"], true);
                break;
        }
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    
    

    private void SpawnAcid()
    {
        if (bSpawningAcid || SpawnCoro != null) // Double-check to prevent multiple spawns
            return;
        
        bSpawningAcid = true;
        if (SpawnerFlipFlop % 3 == 0)
        {
            SpawnCoro = StartCoroutine(HandleSpawning(SphereSpawner.Spawn()));
        }
        else
        {
            SpawnCoro = StartCoroutine(HandleSpawning(RingSpawner.Spawn()));
        }
        SpawnerFlipFlop++;
    }

    private IEnumerator HandleSpawning(IEnumerator spawnRoutine)
    {
        yield return StartCoroutine(spawnRoutine);
    
        // Add a small delay to ensure animations have time to play
        yield return new WaitForSeconds(0.5f);
    
        SpawnCoro = null;
        bSpawningAcid = false;
        RatMovementState = ERatMovementState.ERMS_Reposition;
        RatFSM.Animator.SetBool(AnimKeys["Idle"], false);
    }
    
    public Vector3 GetWanderPoint()
    {
        Vector3 origin = RatFSM.RatController.transform.position;
        
        /* Generate a random point inside a unit sphere, then scale it by our wanderRadius */
        Vector3 randomDirection = Random.insideUnitSphere * WanderRadius; 
    
        /* Shift this random direction so that it’s relative to our origin point */
        randomDirection += origin;

        /* Try sampling this position on the NavMesh */
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, WanderRadius, NavMesh.AllAreas))
        {
            /* If successful, return the sampled position */
            return hit.position;
        }

        print("not a suitable sample");

        /* If no valid position found, just return the original position */
        return origin; 
    }
    
    void RotateAndMoveTowardTarget(Vector3 targetPosition)
    {
        Transform RatTransform = RatFSM.RatController.transform;

        Vector3 directionToTarget = (targetPosition - RatTransform.position).normalized;
        float distanceToTarget = Vector3.Distance(targetPosition, RatTransform.position);
        float angleToTarget = RotateTowardTarget(directionToTarget, RotateSpeed);

        if (angleToTarget > ThresholdAngle)
        {
            if (distanceToTarget > RatFSM.RatController.AI.stoppingDistance && angleToTarget <= ThresholdAngle + 30)
            {
                MoveTowardTarget(targetPosition);
            }
            else
            {
                TurnInPlace(directionToTarget);
            }
        }
        else
        {
            MoveTowardTarget(targetPosition);
        }
    }
    
    private void MoveTowardTarget(Vector3 targetPosition)
    {
        RatFSM.RatController.AI.SetDestination(targetPosition);

        Vector3 localDesiredVelocity = RatFSM.RatController.transform.InverseTransformDirection(RatFSM.RatController.AI.velocity);
        RatFSM.Animator.SetFloat(AnimKeys["Move"], localDesiredVelocity.z);
    }

    private void TurnInPlace(Vector3 directionToTarget)
    {
        float turnDirection =
            Mathf.Sign(Vector3.Cross(RatFSM.RatController.transform.forward, directionToTarget).y);

        RatFSM.RatController.AI.ResetPath();
    }

    private float RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        RatFSM.RatController.transform.rotation = Quaternion.Slerp(RatFSM.RatController.transform.rotation,
            targetRotation, rotateSpeed * Time.deltaTime);

        return Vector3.Angle(RatFSM.RatController.transform.forward, directionToTarget);
    }
}