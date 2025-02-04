using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ATrinityVictory : MonoBehaviour
{
    public float ScoreTextSize = 2.3f;
    public float PauseTime = 3.5f;
    public Slider DamageTakenSlider;
    public Slider TimeSlider;
    public GameObject ScorePanel;
    public TextMeshProUGUI DamageTakenText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI ScoreText;
    public Button QuitButton;
    public CanvasGroup TrinityGUICanvasGroup;
    
    private bool bScoreDisplayComplete;
    private float PauseTimer;
    
    // Start is called before the first frame update
    void Start()
    {
         ScoreText.text = ATrinityScore.GetScoreString(ATrinityGameManager.GetScore().GetScore());
        
         float totalTime = ATrinityGameManager.GetScore().GetTimer();
         int minutes = (int)(totalTime / 60f);
         int seconds = (int)(totalTime % 60f);
         int milliseconds = (int)((totalTime * 1000f) % 1000f);
        
         // Format: mm:ss:millisecond
         TimeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        
         // Format damage with thousands separator, e.g., 1,234
         DamageTakenText.text = ATrinityGameManager.GetScore().GetDamageTaken().ToString("N0");

        DamageTakenSlider.value = ATrinityGameManager.GetScore().NormalizedDamageTakenScore;
        TimeSlider.value = ATrinityGameManager.GetScore().NormalizedTimeScore;
        
        ScorePanel.SetActive(false);
        DamageTakenSlider.gameObject.SetActive(false);
        TimeSlider.gameObject.SetActive(false);
        ScoreText.gameObject.SetActive(false);
        QuitButton.gameObject.SetActive(false);

        StartCoroutine(ScoreDisplayCoro());
    }

    // Update is called once per frame
    void Update()
    {
        bool bExitScoreDisplay = ATrinityGameManager.GetInput().ForcefieldInput || ATrinityGameManager.GetInput().MenuInput;
        
        if (bScoreDisplayComplete && bExitScoreDisplay)
        {
            Time.timeScale = 1f;
            ATrinityGameManager.SetGameFlowState(EGameFlowState.MAIN_MENU);
            SceneManager.LoadScene("PORTAL");
        }
    }

    private IEnumerator ScoreDisplayCoro()
    {
        PauseTimer = PauseTime;

        yield return null;
        
        while (PauseTimer > 0f)
        {
            Time.timeScale = PauseTimer / PauseTime;
            TrinityGUICanvasGroup.alpha = PauseTimer / PauseTime;
            PauseTimer -= Time.unscaledDeltaTime;
            
            yield return null;
        }

        Time.timeScale = 0f;
        ATrinityGameManager.SetGameFlowState(EGameFlowState.PAUSED);
        ScorePanel.gameObject.SetActive(true);
        
        yield return new WaitForSecondsRealtime(1.25f);
        
        TimeSlider.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(1.25f);
        
        DamageTakenSlider.gameObject.SetActive(true);
        
        yield return new WaitForSecondsRealtime(1.25f);
        
        ScoreText.gameObject.SetActive(true);

        while (ScoreText.transform.localScale.x > ScoreTextSize)
        {
            float currentScale = ScoreText.transform.localScale.x;
            
            float newScale = Mathf.Lerp(currentScale, ScoreTextSize, Time.unscaledDeltaTime * 24f);

            ScoreText.transform.localScale = new Vector3(newScale, newScale, newScale);

            yield return null;
        }
        
        QuitButton.gameObject.SetActive(true);
        bScoreDisplayComplete = true;
        
    }
}
