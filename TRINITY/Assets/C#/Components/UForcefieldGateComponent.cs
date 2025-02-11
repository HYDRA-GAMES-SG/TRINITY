using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UForcefieldGateComponent : MonoBehaviour
{
    public AGate Gate;
    public static float ManaShieldDamageTakenByPlayer = 0f;
    public static float ManaDamagePerSecond = 3f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {      
        if (ManaShieldDamageTakenByPlayer > 9f)
        {
            Gate.Open();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (ATrinityGameManager.GetBrain().bForcefieldActive)
            {
                ATrinityGameManager.GetSpells().ManaComponent.Modify(-ManaDamagePerSecond * Time.deltaTime);
                ManaShieldDamageTakenByPlayer += ManaDamagePerSecond * Time.deltaTime;
            }
            else
            {
                ATrinityGameManager.GetPlayerController().HealthComponent
                    .Modify(-ManaDamagePerSecond * 2f * Time.deltaTime);
            }
        }
    }
}
