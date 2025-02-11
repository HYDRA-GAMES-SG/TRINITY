using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIceCubeGateComponent : MonoBehaviour
{
    private AGate Gate;
    public UTutorialTriggerComponent Trigger;

    private bool bIceCubeExists = false;
    private bool bIceWaveCast = false;
    
    // Start is called before the first frame update
    void Start()
    {
        Gate = GetComponent<AGate>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (bIceCubeExists)
        {
            if (ATrinityGameManager.GetSpells().UtilityCold.bActive)
            {
                Gate.Open();
            }
        }
        

        if (bIceWaveCast)
        {
            IceCube ic = FindObjectOfType<IceCube>();

            if (ic == null)
            {
                return;
            }
            
            if (ic.Mesh.enabled == true)
            {
                if (ATrinityGameManager.GetBrain().GetCurrentSpell() == null)
                {
                    bIceCubeExists = true;

                    ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
                    Trigger.TutorialVideoIndex = 7;
                    ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
                    return;
                }
            }
        }

        if (ATrinityGameManager.GetInput().ElementalPrimaryInput &&
            ATrinityGameManager.GetBrain().GetElement() == ETrinityElement.ETE_Cold)
        {
            if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
            {
                return;
            }

            bIceWaveCast = true;
                
            ATrinityGameManager.GetGUI().GetVideos().StopTutorial();
            Trigger.TutorialVideoIndex = 8;
            ATrinityGameManager.GetGUI().GetVideos().PlayTutorial(Trigger.TutorialVideoIndex);
        }
    }
}