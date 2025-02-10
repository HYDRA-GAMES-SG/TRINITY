using System;
using UnityEngine;

public class UTutorialTriggerComponent : MonoBehaviour
{
    public int TutorialVideoIndex;
    
    private ATrinityVideos TrinityVideos;
    [HideInInspector]
    public bool bPlayerInside;
    
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        TrinityVideos = ATrinityGameManager.GetGUI().GetVideos();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TrinityVideos.PlayTutorial(TutorialVideoIndex);
            bPlayerInside = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    void OnTriggerExit(Collider other)
    {
        /* ------------------------------------------------------------------
           Check if the collider belongs to the player.
           ------------------------------------------------------------------ */
        if (other.CompareTag("Player"))
        {
            /* ------------------------------------------------------------------
               Stop the tutorial video.
               ------------------------------------------------------------------ */
            TrinityVideos.StopTutorial();
            bPlayerInside = false;
        }
    }
}