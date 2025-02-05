using UnityEngine;
using UnityEngine.SceneManagement;

public class APortal : MonoBehaviour
{
    [SerializeField] private string sceneName = "BossScene";
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        StartPortalSequence();
    }

    private void StartPortalSequence()
    {
        ATrinityController playerController = ATrinityGameManager.GetPlayerController();
        
        SceneManager.LoadScene(sceneName);
    }

}
