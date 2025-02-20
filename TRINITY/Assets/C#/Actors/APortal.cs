using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class APortal : MonoBehaviour
{
    [SerializeField] public string SceneName = "BossScene";
    private Collider Collider;
    public bool bActive = true;

    private void Start()
    {
        Collider = GetComponent<Collider>();
        int nBossesDefeated = ATrinityGameManager.TotalBossesDefeated();
        
        switch (SceneName)
        {
            case "CrabBossDungeon":
                SetActive(nBossesDefeated == 0);
                break;
            case "DevourerSentinelBossDungeon":
                SetActive(nBossesDefeated == 1);
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !bActive)
        {
            return;
        }
        
        ATrinityGameManager.LoadScene(SceneName);
    }

    public void SetActive(bool bSetActive)
    {
        Collider.enabled = bSetActive;
        transform.GetChild(0).gameObject.SetActive(bSetActive);
        transform.GetChild(1).gameObject.SetActive(bSetActive);
        bActive = bSetActive;
    }
    

}
