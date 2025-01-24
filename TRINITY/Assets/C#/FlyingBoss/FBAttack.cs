using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBAttack : FlyingBossState
{
    [SerializeField] string AnimKey = "FlyElectroShot";

    [SerializeField] float moveSpeed;

    [SerializeField] ElectricBoom ElecBomb;
    [SerializeField] LayerMask groundLayer;
    //[SerializeField] float thunderSpawnDelay = 0.5f;
    [SerializeField] int thunderCount = 3;
    [SerializeField] float thunderRadius = 5f;
    [SerializeField] float minDis = 3f;

    Vector3 AttackPos;
    bool thunderSpawned = false;


    public override bool CheckEnterTransition(IState fromState)
    {
        return ATrinityGameManager.GetGameFlowState() != EGameFlowState.DEAD;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        Vector3[] attackPositions = AttackPositionBehind();
        AttackPos = attackPositions[Random.Range(0, attackPositions.Length)];
        thunderSpawned = false;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        MoveTowardsTarget(dt);
        if (IsAtAttackPosition() && !thunderSpawned)
        {
            FlyingBossFSM.Animator.SetTrigger(AnimKey);
            thunderSpawned = true; // Prevent multiple thunder spawns
            StartCoroutine(SpawnMultipleThundersWithDelay(0.2f));
        }

        string layerName = GetType().Name;
        int layerIndex = FlyingBossFSM.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = FlyingBossFSM.Animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            FlyingBossFSM.EnqueueTransition<FBHover>();
        }
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        FlyingBossFSM.FlyingBossController.bCanElectricChargeAttack = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }

    Vector3[] AttackPositionBehind()
    {
        Vector3 IBPos = FlyingBossFSM.FlyingBossController.InvincibleBoss.transform.position;
        Transform bossTransform = FlyingBossFSM.FlyingBossController.InvincibleBoss.transform;

        float shoulderOffset = 5f;
        float shoulderHighOffset = 11f;
        float headOffset = 13f;

        Vector3 leftShoulder = IBPos - bossTransform.right * shoulderOffset + bossTransform.up * shoulderHighOffset;
        Vector3 rightShoulder = IBPos + bossTransform.right * shoulderOffset + bossTransform.up * shoulderHighOffset;
        Vector3 aboveHead = IBPos + bossTransform.up * headOffset;

        return new Vector3[] { leftShoulder, rightShoulder, aboveHead };
    }

    private void MoveTowardsTarget(float dt)
    {
        Vector3 direction = (AttackPos - FlyingBossFSM.FlyingBossController.transform.position).normalized;
        Vector3 smoothVelocity = Vector3.Lerp(FlyingBossFSM.FlyingBossController.RB.velocity, direction * moveSpeed, RotateSpeed * dt);
        FlyingBossFSM.FlyingBossController.RB.velocity = smoothVelocity;

        Vector3 faceDirection = (FlyingBossFSM.PlayerController.transform.position - FlyingBossFSM.FlyingBossController.transform.position).normalized;

        FlyingBossFSM.FlyingBossController.RotateTowardTarget(faceDirection, RotateSpeed);
    }

    private bool IsAtAttackPosition()
    {
        float positionTolerance = 0.5f;
        return Vector3.Distance(FlyingBossFSM.FlyingBossController.transform.position, AttackPos) <= positionTolerance;
    }
    //private IEnumerator SpawnThunderWithDelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);

    //    Vector3 groundPosition = GetGroundPosition(FlyingBossFSM.PlayerController.transform.position);
    //    if (groundPosition != Vector3.zero && ElecBomb != null)
    //    {
    //        ElectricBoom eb = Instantiate(ElecBomb, groundPosition, Quaternion.identity);
    //        eb.GetDamage(FlyingBossFSM.FlyingBossController.GetCurrentAttackDamage());
    //    }
    //}
    private IEnumerator SpawnMultipleThundersWithDelay(float interval)
    {
        List<Vector3> spawnPositions = new List<Vector3>();

        // Get the player's position and velocity
        Vector3 currentPlayerPos = FlyingBossFSM.PlayerController.transform.position;
        Rigidbody playerRb = FlyingBossFSM.PlayerController.GetComponent<Rigidbody>();
        Vector3 playerVelocity = playerRb ? playerRb.velocity : Vector3.zero;

        int spawnedCount = 0; // Track how many thunder strikes have been spawned
        int maxAttempts = 10; // Limit attempts for finding valid positions

        while (spawnedCount < thunderCount)
        {
            bool positionFound = false;
            int attempts = 0;

            while (!positionFound && attempts < maxAttempts)
            {
                // Predict the position based on player's velocity and interval
                float predictionTime = interval * (spawnedCount + 1); // Incremental time for each thunder
                Vector3 predictedPosition = currentPlayerPos + playerVelocity * predictionTime;

                // Apply a random offset for variety
                Vector3 randomOffset = Random.insideUnitSphere * thunderRadius;
                randomOffset.y = 0; // Keep offset on the ground plane
                predictedPosition += randomOffset;

                // Validate and get the ground position
                Vector3 groundPosition = GetGroundPosition(predictedPosition);
                if (groundPosition != Vector3.zero && IsPositionValid(groundPosition, spawnPositions, minDis))
                {
                    spawnPositions.Add(groundPosition);
                    positionFound = true;
                }
                attempts++;
            }

            if (positionFound)
            {
                // Spawn the thunder after the delay
                yield return new WaitForSeconds(interval);
                if (ElecBomb != null)
                {
                    ElectricBoom eb = Instantiate(ElecBomb, spawnPositions[spawnedCount], Quaternion.identity);
                    eb.GetControllerDamage(FlyingBossFSM.FlyingBossController, FlyingBossFSM.FlyingBossController.GetCurrentAttackDamage());
                }
                spawnedCount++;
            }
            else
            {
                Debug.LogWarning("Unable to find a valid position for thunder strike.");
                break; // Exit if no valid position is found after max attempts
            }
        }
    }

    private bool IsPositionValid(Vector3 newPosition, List<Vector3> existingPositions, float minDistance)
    {
        foreach (Vector3 position in existingPositions)
        {
            if (Vector3.Distance(newPosition, position) < minDistance)
            {
                return false; // Too close to an existing position
            }
        }
        return true; // Position is valid
    }
    private Vector3 GetGroundPosition(Vector3 startPosition)
    {
        RaycastHit hit;

        if (Physics.Raycast(startPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(AttackPos, 1f);
    }
}


