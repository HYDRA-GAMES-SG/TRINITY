using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamageTrigger : MonoBehaviour
{
    private ExplosionMonsterController emController;

    void Start()
    {
        emController = FindObjectOfType<ExplosionMonsterController>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            emController.ApplyDamageToPlayer(other.gameObject);
        }
    }
}
