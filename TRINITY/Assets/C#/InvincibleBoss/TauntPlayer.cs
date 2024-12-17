using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauntPlayer : MonoBehaviour
{
    [SerializeField] AInvincibleBossController IBController;

    [SerializeField] float PullForce;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player inside range");
            Vector3 direction = (other.transform.position - IBController.transform.position).normalized;
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.AddForce(-direction * PullForce);
        }
    }
}
