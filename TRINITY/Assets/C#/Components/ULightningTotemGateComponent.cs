using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ULightningTotemGateComponent : MonoBehaviour
{
    private AGate Gate;
    public UTutorialTriggerComponent Trigger;

    private bool bTotemSummoned = false;
    private bool bTotemEnraged = false;
    private bool bFlashed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        Gate = GetComponent<AGate>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bFlashed)
        {
            Gate.Open();
        }

        if (bTotemEnraged)
        {
            if (ATrinityGameManager.GetSpells().UtilityLightning.bCanBlink)
            {
                if (ATrinityGameManager.GetInput().ElementalUtililyInput &&
                    ATrinityGameManager.GetBrain().GetElement() == ETrinityElement.ETE_Lightning &&
                    ATrinityGameManager.GetGameFlowState() == EGameFlowState.PLAY)
                {
                    bFlashed = true;
                }
            }
        }



        if (bTotemSummoned)
        {
            foreach (GameObject go in ATrinityGameManager.GetSpells().SecondaryLightning.GetTotems())
            {
                if (go.GetComponent<LightningTotem>().Status == ELightningTotemStatus.ELTS_Enraged)
                {
                    bTotemEnraged = true;
                    ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                    Trigger.TutorialVideoIndex = 14;
                    ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                }
            }
            return;
        }

        LightningTotem totem = FindObjectOfType<LightningTotem>();

        if (totem)
        {
            foreach (GameObject go in ATrinityGameManager.GetSpells().SecondaryLightning.GetTotems())
            {
                if (go.GetComponent<LightningTotem>().Status == ELightningTotemStatus.ELTS_Summoned)
                {
                    bTotemSummoned = true;
                    ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                    Trigger.TutorialVideoIndex = 12;
                    ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                }
            }
        }
    }
}