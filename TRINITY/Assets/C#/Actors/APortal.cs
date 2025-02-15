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
        
        ATrinityGameManager.LoadScene(sceneName);
    }

}
