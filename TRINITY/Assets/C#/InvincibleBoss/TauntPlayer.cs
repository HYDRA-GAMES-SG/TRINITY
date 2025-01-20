using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauntPlayer : MonoBehaviour
{
    [SerializeField] AInvincibleBossController IBController;

    [SerializeField] float PullForce;

    [SerializeField] float StunTime;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ATrinityBrain playerBrain = other.gameObject.transform.root.GetComponentInChildren<ATrinityBrain>();
            if (playerBrain != null)
            {
                playerBrain.SetStunnedState(StunTime);
            }
            //Debug.Log("Player inside range");
            Vector3 direction = (other.transform.position - IBController.transform.position).normalized;
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.AddForce(-direction * PullForce);


        }
    }
}
