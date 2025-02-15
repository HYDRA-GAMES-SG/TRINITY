using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Security.Cryptography;
using TMPro;

public class ATrinityLoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Sprite[] SceneImages;
    [SerializeField] private string[] SceneNames;

    [SerializeField] private Image LoadingImage;
    [SerializeField] private Slider ProgressBar;
    [SerializeField] private TextMeshProUGUI ProgressText;
    [SerializeField] private CanvasGroup CanvasGroup;
    
    [Header("Configuration")]
    [SerializeField] private float MinimumLoadingTime = 3f;
    [SerializeField] private float FadeDuration = 1f;
    
    private AsyncOperation ASYNC_LOAD_ROUTINE;
    private float LoadStartTime;
    private bool bIsAsyncLoading = false;

    public void LoadScene(string sceneName)
    {
        if (bIsAsyncLoading) return; // Prevents multiple scene loads

        int sceneIndex = -1;
        for (int i = 0; i < SceneNames.Length; i++)
        {
            if (SceneNames[i] == sceneName)
            {
                sceneIndex = i;
                break;
            }
        }

        if (sceneIndex == -1)
        {
            Debug.LogError($"Scene {sceneName} not found in SceneNames array.");
            return;
        }

        bIsAsyncLoading = true; // Mark as loading
        ATrinityGameManager.SetGameFlowState(EGameFlowState.PAUSED);
        ATrinityGameManager.GetAudio().Mixer.FindMatchingGroups("Master")[0].audioMixer.SetFloat("Volume", PlayerPrefs.GetFloat("MasterVolume", 0f));
        LoadStartTime = Time.unscaledTime;
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        /* Check if a scene is already loading */
        if (ASYNC_LOAD_ROUTINE != null && !ASYNC_LOAD_ROUTINE.isDone)
        {
            Debug.LogWarning("A scene is already loading!");
            yield break;
        }

        /* Begin asynchronous scene loading */
        ASYNC_LOAD_ROUTINE = SceneManager.LoadSceneAsync(SceneNames[sceneIndex]);
        ASYNC_LOAD_ROUTINE.allowSceneActivation = false; // Prevents immediate activation

        /* Set loading screen image and canvas alpha */
        if (SceneImages[sceneIndex] != null)
        {
            LoadingImage.sprite = SceneImages[sceneIndex];
        }
        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 1f;
        }

        LoadStartTime = Time.unscaledTime;

        /* Update progress bar until both async loading is ready and the minimum time has passed */
        while (true)
        {
            // asyncProgress goes from 0 to 1 once async progress reaches 0.9
            float asyncProgress = Mathf.Clamp01(ASYNC_LOAD_ROUTINE.progress / 0.9f);
            // timeProgress goes from 0 to 1 based on the elapsed time vs. MinimumLoadingTime
            float timeProgress = Mathf.Clamp01((Time.unscaledTime - LoadStartTime) / MinimumLoadingTime);
            // effectiveProgress is the slower of the two
            float effectiveProgress = Mathf.Min(asyncProgress, timeProgress);

            ProgressText.text = $"[ {(effectiveProgress * 100):0}% ]";
            ProgressBar.value = effectiveProgress;

            // If both conditions are met, break out of the loop
            if (asyncProgress >= 1f && timeProgress >= 1f)
            {
                break;
            }
            yield return null;
        }

        /* Allow the async operation to finish by enabling scene activation */
        ASYNC_LOAD_ROUTINE.allowSceneActivation = true;

        /* Wait until the asynchronous operation reports it is done */
        while (!ASYNC_LOAD_ROUTINE.isDone)
        {
            yield return null;
        }
        
        yield return null; // Extra frame to ensure the new scene is visible

        /* Fade out the loading screen after the scene is loaded and visible */
        yield return StartCoroutine(FadeLoadingScreen());

        bIsAsyncLoading = false;
        ATrinityGameManager.DeserializeSettings();
        ATrinityGameManager.SetGameFlowState(EGameFlowState.PLAY);
    }

    private bool IsLoadingComplete()
    {
        return ASYNC_LOAD_ROUTINE.progress >= 0.9f && ASYNC_LOAD_ROUTINE.isDone;
    }
    
    private IEnumerator FadeLoadingScreen()
    {
        float elapsedTime = 0f;
        
        if (CanvasGroup != null)
        {
            // fade out canvas
            while (elapsedTime < FadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                CanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeDuration);
                yield return null;
            }
        }
        else if (LoadingImage != null)
        {
            while (elapsedTime < FadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeDuration);
                LoadingImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
        }
    }
}