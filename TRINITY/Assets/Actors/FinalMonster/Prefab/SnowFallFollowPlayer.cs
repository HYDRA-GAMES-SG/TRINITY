using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnowFallFollowPlayer : MonoBehaviour
{
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, 45f, 0);
    void Update()
    {
        Vector3 playerPosition = ATrinityGameManager.GetPlayerController().Position;
        playerPosition.y -= Mathf.Abs(ATrinityGameManager.GetPlayerController().Position.y);
        Vector3 targetPosition = playerPosition + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
