using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnowFallFollowPlayer : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, 30f, 0);
    void Update()
    {
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
