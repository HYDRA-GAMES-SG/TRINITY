using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class APenguinController : IEnemyController
{
    public APenguinBossFSM PenguinFSM;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
    public float CalculateGroundDistance()
    {
        Vector3 PlayerPos = new Vector3(PenguinFSM.PlayerController.transform.position.x, 0, PenguinFSM.PlayerController.transform.position.z);
        Vector3 IBPos = new Vector3(transform.position.x, 0, transform.position.z);

        float distanceToTarget = Vector3.Distance(PlayerPos, IBPos);
        return distanceToTarget;
    }
}
