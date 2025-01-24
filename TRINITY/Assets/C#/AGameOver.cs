using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class AGameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI GameOverText;
    [SerializeField] private Image Background;
    [SerializeField] private float GameOverFadeTime = 2f;
    [SerializeField] private float GameOverFadeDelay = 2.5f;
    private void Start()
    {
        
    }

    public void Display()
    {
        // Set initial alpha to 0
        SetTextAlpha(0f);
        SetBackgroundAlpha(0f);
        
        StartCoroutine(FadeIn());
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        yield return new WaitForSeconds(GameOverFadeDelay);
        
        float fadeTime = 0;
        while (fadeTime < GameOverFadeTime)
        {
            fadeTime += Time.deltaTime;
            GameOverText.characterSpacing = Mathf.Lerp(65f, 165f, fadeTime / GameOverFadeTime);
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(GameOverFadeDelay);
        
        float fadeTime = 0f;
        
        while (fadeTime < GameOverFadeTime)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, fadeTime / GameOverFadeTime);
            SetTextAlpha(alpha);
            SetBackgroundAlpha(alpha);
            yield return null;
        }

        SetBackgroundAlpha(1f);
        SetTextAlpha(1f);
    }

    private void SetTextAlpha(float alpha)
    {
        Color textColor = GameOverText.color;
        textColor.a = alpha;
        GameOverText.color = textColor;
    }
    
    private void SetBackgroundAlpha(float alpha)
    {
        Color panelColor = Background.color;
        panelColor.a = alpha;
        Background.color = panelColor;
    }
}