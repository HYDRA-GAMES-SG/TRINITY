using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics.Platform;
using UnityEngine;

public class UStunPlayerComponent : MonoBehaviour
{
    [SerializeField] float StunTime;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        ATrinityBrain playerBrain = other.gameObject.transform.root.GetComponentInChildren<ATrinityBrain>();
        if (playerBrain == null)
            return;

        playerBrain.SetStunnedState(StunTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        ATrinityBrain playerBrain = collision.gameObject.transform.root.GetComponentInChildren<ATrinityBrain>();
        if (playerBrain == null)
            return;

        playerBrain.SetStunnedState(StunTime);
    }
}
