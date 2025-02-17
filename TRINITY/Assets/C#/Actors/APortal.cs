using UnityEngine;
using UnityEngine.SceneManagement;

public class APortal : MonoBehaviour
{
    [SerializeField] public string sceneName = "BossScene";
    public bool bActive = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !bActive)
        {
            return;
        }
        
        ATrinityGameManager.LoadScene(sceneName);
    }

    public void Activate()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
        bActive = true;
    }

}
