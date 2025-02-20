using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class APortal : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    [SerializeField] public string SceneName;
    private Collider Collider;
    public bool bActive = true;

    private void Start()
    {
        Collider = GetComponent<Collider>();
        
        switch (SceneName)
        {
            case "CrabBossDungeon":
                SetActive(ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu);
                break;
            case "DevourerSentinelBossDungeon":
                SetActive(ATrinityGameManager.TotalBossesDefeated() == 1);
                break;
            case "PORTAL":
                SetActive(true);
                break;
            default:
                break;
        }


        if (ScoreText != null)
        {
            if (ATrinityGameManager.GetScore().SceneScoreLookup.ContainsKey(SceneName))
            {
                ScoreText.text =
                    ATrinityScore.GetScoreString(ATrinityGameManager.GetScore().SceneScoreLookup[SceneName]);
            }
            else
            {
                ScoreText.text = "";
            }
        }

    }

    private void Update()
    {
        if (SceneName == "CrabBossDungeon" && ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu && bActive == false)
        {
            SetActive(true);
        }
        
        if (SceneName == "DevourerSentinelBossDungeon" && ATrinityGameManager.TotalBossesDefeated() == 1 && bActive == false)
        {
            SetActive(true);
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
