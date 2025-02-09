using UnityEngine;

public class UTutorialTriggerComponent : MonoBehaviour
{
    public int TutorialVideoIndex;
    
    private ATrinityVideos TrinityVideos; 
    
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
        }
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
        }
    }
}