using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFlameblastGateComponent : MonoBehaviour
{
    private AGate Gate;
    public UTutorialTriggerComponent Trigger;

    private bool bChanneling = false;
    private bool bCastFlameblast = false;
    
    // Start is called before the first frame update
    void Start()
    {
        Gate = GetComponent<AGate>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Gate.bShouldOpen && ATrinityGameManager.GetSpells().UtilityFire.bAura)
        {
            Gate.Open();
        }
        
        if (bCastFlameblast && Trigger.TutorialVideoIndex == 17)
        {
            if (FindObjectOfType<FlameblastZone>().FireballExplosionInstance != null)
            {
                ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                Trigger.TutorialVideoIndex = 11;
                ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);

                return;
            }
        }
        
        if (bCastFlameblast)
        {
            if (ATrinityGameManager.GetInput().ElementalPrimaryInput 
                && ATrinityGameManager.GetBrain().GetElement() == ETrinityElement.ETE_Fire)
            {
                ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                Trigger.TutorialVideoIndex = 17;
                ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                return;
            }
        }
        
        if (ATrinityGameManager.GetBrain().GetCurrentSpell() is ASecondaryFire)
        {
            bChanneling = true;
            return;
        }

        if (bChanneling)
        {
            if (ATrinityGameManager.GetBrain().GetCurrentSpell() == null)
            {
                bCastFlameblast = true;
                
                ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                Trigger.TutorialVideoIndex = 9;
                ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                return;
            }
        }
    }
}
