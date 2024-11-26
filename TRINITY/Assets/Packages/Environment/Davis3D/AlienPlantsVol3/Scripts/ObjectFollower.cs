using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    [SerializeField] private Transform follower;
    [SerializeField] private Transform objectFollow;
    [SerializeField] private Vector3 offset;
    // Update is called once per frame
    void LateUpdate()
    {
        follower.position = objectFollow.position + offset;
    }
}
