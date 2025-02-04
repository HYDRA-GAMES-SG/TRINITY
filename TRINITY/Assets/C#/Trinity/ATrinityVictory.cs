using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ATrinityVictory : MonoBehaviour
{
    public float FadeTime = 3.5f;
    public Slider DamageTakenSlider;
    public Slider TimeSlider;
    public TextMeshProUGUI DamageTakenText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI ScoreText;
    
    private bool bCoroComplete;
    private float FadeTimer;
    
    // Start is called before the first frame update
    void Start()
    {
        DamageTakenText.alpha = 0f;
        TimeText.alpha = 0f;
        ScoreText.alpha = 0f;

        TimeText.text = ATrinityGameManager.GetScore().GetTimer().ToString();
        DamageTakenText.text = ATrinityGameManager.GetScore().GetDamageTaken().ToString();
        ScoreText.text = ATrinityScore.GetScoreString(ATrinityGameManager.GetScore().GetScore());

        StartCoroutine(VictoryCoro());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator VictoryCoro()
    {
        FadeTimer = FadeTime;
        while (FadeTimer > 0f)
        {
            FadeTimer -= Time.deltaTime;
            Time.timeScale = FadeTimer / FadeTime;
            yield return null;
        }

        Time.timeScale = 0f;
        ATrinityGameManager.SetGameFlowState(EGameFlowState.PAUSED);
        
        ScoreText.alpha = 1f;
        TimeText.alpha = 1f;
        DamageTakenText.alpha = 1f;
    }
}
