using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UElementSwitchGateComponent : MonoBehaviour
{
    /* -- Start of Modification --
       Renamed to PascalCase for member variable per your coding style.
    -- End of Modification -- */
    private AGate Gate;
    public UTutorialTriggerComponent Trigger;

    /* -- Start of Modification --
       Renamed member variable to PascalCase.
    -- End of Modification -- */
    private int CurrentElementNeeded = 0; // Track which element we're waiting for (0-2)

    private bool bColdSwitched = false;
    private bool bFireSwitched = false;
    private bool bLightningSwitched = false;
    private bool bAllSwitched => bColdSwitched && bFireSwitched && bLightningSwitched;
    private bool bNoneSwitched => !bColdSwitched && !bFireSwitched && !bLightningSwitched;
    
    void Start()
    {
        ATrinityGameManager.GetBrain().OnElementChanged += ElementChanged;
        
        Gate = GetComponent<AGate>();
    }

    private void ElementChanged(ETrinityElement newElement)
    {
        if (!Trigger.bPlayerInside)
        {
            return;
        }
        print("Player inside"); 
        /* -- Start of Modification --
           Only enforce the TutorialVideoIndex check if not all elements have been switched.
        -- End of Modification -- */
        if (!bAllSwitched && ((int)newElement != Trigger.TutorialVideoIndex))
        {
            return;
        }
        
        switch (newElement)
        {
            case ETrinityElement.ETE_Cold:
                Trigger.TutorialVideoIndex = 2;
                bColdSwitched = true;
                break;
            case ETrinityElement.ETE_Fire:
                Trigger.TutorialVideoIndex = 1;
                bFireSwitched = true;
                break;
            case ETrinityElement.ETE_Lightning:
                Trigger.TutorialVideoIndex = 0;
                bLightningSwitched = true;
                break;
        }
        
        if (bAllSwitched)
        {
            Trigger.TutorialVideoIndex = (int)newElement;
        }
        
        ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
        ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
    }
    
    void Update()
    {
        if (bAllSwitched)
        {
            Gate.Open();
            Trigger.TutorialVideoIndex = (int)ATrinityGameManager.GetBrain().GetElement();
        }
        
        if (!Trigger.bPlayerInside)
        {
            if (Trigger.TutorialVideoIndex == (int)ATrinityGameManager.GetBrain().GetElement())
            {
                if (!bAllSwitched)
                {
                    Trigger.TutorialVideoIndex++;
                    Trigger.TutorialVideoIndex %= 3;
                }
            }
        }
    }
}