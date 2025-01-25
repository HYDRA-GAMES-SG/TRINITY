using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AGameOver : MonoBehaviour
{
    [SerializeField] private string PortalScene;
    [SerializeField] private GameObject ContinueParent;
    [SerializeField] private AudioSource BGMReference;
    private AudioSource GameOverSource;
    [SerializeField] private float GameOverSFXVolume;
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private Image Background;
    [SerializeField] private float FadeTime = 2f;
    [SerializeField] private float FadeDelay = 2.5f;

    private bool bFadeComplete = false;
    
    private void Start()
    {
        GameOverSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (bFadeComplete)
        {
            if (!ContinueParent.activeSelf)
            {
                ContinueParent.SetActive(true);
            }

            if (ATrinityGameManager.GetInput().JumpInput)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (ATrinityGameManager.GetInput().ForcefieldInput)
            {
                SceneManager.LoadScene(PortalScene);
            }
        }
    }

    public void Display()
    {
        // Set initial alpha to 0
        SetTextAlpha(0f);
        SetBackgroundAlpha(0f);
        
        StartCoroutine(FadeIn());
        StartCoroutine(AnimateText());
        StartCoroutine(FadeMusic());
    }

    private IEnumerator FadeMusic()
    {
        float fadeTime = 0;

        float startVolume = BGMReference.volume;
        while (fadeTime < FadeDelay)
        {
            fadeTime += Time.deltaTime;
            BGMReference.volume = Mathf.Lerp(startVolume, 0f, fadeTime / FadeDelay);
            
            if (!GameOverSource.isPlaying)
            {
                GameOverSource.Play();
            }
            
            yield return null;
        }
    }

    private IEnumerator AnimateText()
    {
        yield return new WaitForSeconds(FadeDelay);
        
        float fadeTime = 0;
        while (fadeTime < FadeTime)
        {
            fadeTime += Time.deltaTime;
            Text.characterSpacing = Mathf.Lerp(65f, 165f, fadeTime / FadeTime);
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(FadeDelay);
        
        float fadeTime = 0f;
        
        while (fadeTime < FadeTime)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, fadeTime / FadeTime);
            SetTextAlpha(alpha);
            SetBackgroundAlpha(alpha);
            yield return null;
        }

        SetBackgroundAlpha(1f);
        SetTextAlpha(1f);

        bFadeComplete = true;
    }

    private void SetTextAlpha(float alpha)
    {
        Color textColor = Text.color;
        textColor.a = alpha;
        Text.color = textColor;
    }
    
    private void SetBackgroundAlpha(float alpha)
    {
        Color panelColor = Background.color;
        panelColor.a = alpha;
        Background.color = panelColor;
    }
}