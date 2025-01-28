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
        if (playerController != null)
        {
            ResetPlayerToOrigin(playerController);
        }
        else
        {
            Debug.LogWarning("PlayerController is null. Cannot reset to origin.");
        }
        SceneManager.LoadScene(sceneName);
    }
    void ResetPlayerToOrigin(ATrinityController playerController)
    {
        Transform playerTransform = playerController.transform;
        playerTransform.position = new Vector3(-75.98f, 0.59f, 55.2f);
    }

}
