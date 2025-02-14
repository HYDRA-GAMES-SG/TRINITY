using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFlameblastGateComponent : MonoBehaviour
{
    private AGate Gate;
    public UTutorialTriggerComponent Trigger;

    private bool bChanneling = false;
    private bool bCastFlameblast = false;
    private bool bFlameblastHit = false;

    // Start is called before the first frame update
    void Start()
    {
        Gate = GetComponent<AGate>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Trigger.bPlayerInside)
        {
            return;
        }
        if (bFlameblastHit)
        {
            if (ATrinityGameManager.GetSpells().UtilityFire.bAura)
            {
                Gate.Open();
                ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
            }
            return;
        }

        if (bCastFlameblast && Trigger.TutorialVideoIndex == 17 && !bFlameblastHit)
        {
            if (FindObjectOfType<FlameblastZone>().FireballExplosionInstance != null)
            {
                ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                Trigger.TutorialVideoIndex = 11; //Overheat
                ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                bFlameblastHit = true;
                return;
            }

            return;
        }

        if (bCastFlameblast)
        {
            if (!bFlameblastHit)
            {
                if (ATrinityGameManager.GetInput().ElementalPrimaryInput
                    && ATrinityGameManager.GetBrain().GetElement() == ETrinityElement.ETE_Fire
                    && ATrinityGameManager.GetGameFlowState() == EGameFlowState.PLAY)
                {
                    ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                    Trigger.TutorialVideoIndex = 17; //Hit Flameblast
                    ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                    return;
                }
            }
            return;
        }

        if (ATrinityGameManager.GetBrain().GetCurrentSpell() is ASecondaryFire)
        {
            bChanneling = true;
        }

        if (bChanneling)
        {
            if (ATrinityGameManager.GetBrain().GetCurrentSpell() == null)
            {
                bCastFlameblast = true;
                FindObjectOfType<FlameblastZone>().bTutorial = true;

                ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                Trigger.TutorialVideoIndex = 9;
                ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                return;
            }

            return;
        }
        else
        {
            print("Hello");
        }
    }
}
