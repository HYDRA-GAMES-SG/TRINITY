using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AMainMenuCamera : MonoBehaviour
{
    public float FadeMainMenuDuration = 1.5f;
    public float AnimateCameraDuration = 3f;
    public static System.Action OnSwitchToPlayerCamera;
    public GameObject MainMenuParentObject;
    public CanvasGroup MainMenuCanvas;
    public CanvasGroup CreditsCanvas;
    public AudioSource MainMenuMusic;
    public GameObject SceneBGM;
    
    private GameObject MainMenuCamera;
    private ATrinityCamera TrinityCamera;

    public void HandleMenuMusic()
    {
        if (ATrinityGameManager.CurrentScene == "PORTAL" && ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu)
        {
            MainMenuMusic.volume = 0f;
            MainMenuMusic.Stop();
            SceneBGM.SetActive(true);
        }
        if (ATrinityGameManager.CurrentScene == "CrabBossDungeon")
        {
            ATrinityGameManager.OnSceneChanged -= HandleMenuMusic;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {

        ATrinityGameManager.OnSceneChanged += HandleMenuMusic;
        
        if (!TrinityCamera)
        {
            ATrinityCamera[] allTrinityCameras = Resources.FindObjectsOfTypeAll<ATrinityCamera>();
            foreach (ATrinityCamera cam in allTrinityCameras)
            {
                TrinityCamera = cam;
                break;
            }

            if (TrinityCamera != null)
            {
                TrinityCamera.gameObject.SetActive(true);
            }
        }
        
        if (!MainMenuParentObject.GetComponent<ATrinityMainMenu>().bCanSkipMainMenu)
        {
            TrinityCamera.Camera.enabled = false;
        }
        else
        {
            TrinityCamera.Camera.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Animate()
    {
        StartCoroutine(AnimateCoroutine());
        StartCoroutine(FadeMainMenu());
        StartCoroutine(FadeCredits());
    }

    private IEnumerator FadeCredits()
    {
        float fadeTime = 0f;
        while (fadeTime < FadeMainMenuDuration)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, fadeTime / FadeMainMenuDuration);
            CreditsCanvas.alpha = alpha;

            yield return null;
        }

        CreditsCanvas.gameObject.SetActive(false);
    }
    
    private IEnumerator FadeMainMenu()
    {
        float fadeTime = 0f;
        while (fadeTime < FadeMainMenuDuration)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, fadeTime / FadeMainMenuDuration);
            MainMenuMusic.volume = Mathf.Lerp(.1f, 0, fadeTime / FadeMainMenuDuration);
            MainMenuCanvas.alpha = alpha;

            yield return null;
        }

        MainMenuCanvas.gameObject.SetActive(false);
        MainMenuMusic.gameObject.SetActive(false);
        SceneBGM.SetActive(true);
        if (ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu)
        {
            ATrinityGameManager.GetGUI().GetMainMenu().Gate.Close();
        }
    }
    
    private IEnumerator AnimateCoroutine()
    {
        yield return new WaitForSeconds(3f);
        
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        while (Vector3.Distance(transform.position, TrinityCamera.transform.position) > .01f 
               && Mathf.Abs(Quaternion.Dot(transform.rotation, TrinityCamera.transform.rotation)) > .01f)
        {
            float t = elapsedTime / AnimateCameraDuration;
        
            // Use smooth interpolation (smoothstep)
            float smoothT = t * t * (3f - 2f * t);
        
            // Interpolate position and rotation
            transform.position = Vector3.Lerp(startPosition, TrinityCamera.transform.position, smoothT);
            transform.rotation = Quaternion.Slerp(startRotation, TrinityCamera.transform.rotation, smoothT);
        
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure final position and rotation are exact
        transform.position = TrinityCamera.transform.position;
        transform.rotation = TrinityCamera.transform.rotation;
        
        this.GetComponent<Camera>().enabled = false;
        TrinityCamera.Camera.enabled = true;
        OnSwitchToPlayerCamera?.Invoke();
        ATrinityGameManager.SetGameFlowState(EGameFlowState.PLAY);
    }
}
