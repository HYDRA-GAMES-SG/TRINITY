using UnityEngine;
using UnityEngine.SceneManagement;

public class APortal : MonoBehaviour
{
    [SerializeField] private GameObject portalLoadingScreen; // Reference to the portal loading screen prefab
    [SerializeField] private float portalScreenDuration = 3.0f; // Duration of the portal screen
    [SerializeField] private string bossSceneName = "BossScene"; // Name of the boss scene to load

    private bool bIsLoading = false; // Tracks whether the portal sequence is active

    private void OnTriggerEnter(Collider other)
    {
        if (bIsLoading || !other.CompareTag("Player"))
        {
            return;
        }

        StartPortalSequence();
    }

    private void StartPortalSequence()
    {
        bIsLoading = true;
        portalLoadingScreen.SetActive(true);
        Invoke(nameof(LoadBossScene), portalScreenDuration); // Wait for the screen duration before loading
    }

    private void LoadBossScene()
    {
        SceneManager.LoadScene(bossSceneName);
    }
}
