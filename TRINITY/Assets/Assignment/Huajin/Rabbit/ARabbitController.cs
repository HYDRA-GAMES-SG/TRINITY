using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARabbitController : IEnemyController
{
    public ARabbitFSM RabbitFSM;

    void Start()
    {

    }

    void Update()
    {
        //Debug.Log(CalculateDistance());
    }

    public float CalculateDistance()
    {
        Vector3 PlayerPos = RabbitFSM.PlayerController.transform.position;

        float distanceToTarget = Vector3.Distance(PlayerPos, transform.position);
        return distanceToTarget;
    }
}