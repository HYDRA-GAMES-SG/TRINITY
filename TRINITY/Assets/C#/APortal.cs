using UnityEngine;
using UnityEngine.SceneManagement;

public class APortal : MonoBehaviour
{
    [SerializeField] private string sceneName = "BossScene"; // Name of the boss scene to load
    
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
        SceneManager.LoadScene(sceneName);
    }
}
