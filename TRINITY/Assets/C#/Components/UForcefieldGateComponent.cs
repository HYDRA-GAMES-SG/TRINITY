using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UForcefieldGateComponent : MonoBehaviour
{
    public AMainMenuGate MainGate;
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
            if (ATrinityGameManager.GetGUI().GetOptions().TutorialButton != null) 
            {
                ATrinityGameManager.GetGUI().GetOptions().bTutorialDone = true;
                ATrinityGameManager.GetGUI().GetOptions().TutorialButton.SetActive(false);
                MainGate.Open();
            }
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
                if (ATrinityGameManager.GetPlayerController().HealthComponent.Current >
                    ManaDamagePerSecond * 2f * Time.deltaTime)
                {
                    ATrinityGameManager.GetPlayerController().HealthComponent
                        .Modify(-ManaDamagePerSecond * 2f * Time.deltaTime);
                }
            }
        }
    }
}
