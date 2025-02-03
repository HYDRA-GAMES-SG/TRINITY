using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AMainMenuCamera : MonoBehaviour
{
    public float FadeMainMenuDuration = 1.5f;
    public float AnimateCameraDuration = 3f;
    public GameObject PlayerCamera;
    public static System.Action OnSwitchToPlayerCamera;
    public GameObject MainMenuParentObject;
    public CanvasGroup MainMenuCanvas;
    
    private GameObject MainMenuCamera;
    
    
    // Start is called before the first frame update
    void Start()
    {
        PlayerCamera.SetActive(true);
        PlayerCamera.GetComponent<ATrinityCamera>().Camera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Animate()
    {
        StartCoroutine(AnimateCoroutine());
        StartCoroutine(FadeMainMenu());
    }

    private IEnumerator FadeMainMenu()
    {
        float fadeTime = 0f;
        while (fadeTime < FadeMainMenuDuration)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, fadeTime / FadeMainMenuDuration);
            MainMenuCanvas.alpha = alpha;

            yield return null;
        }

        MainMenuCanvas.gameObject.SetActive(false);
    }
    
    private IEnumerator AnimateCoroutine()
    {
        yield return new WaitForSeconds(3f);
        
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        while (Vector3.Distance(transform.position, PlayerCamera.transform.position) > .01f 
               && Mathf.Abs(Quaternion.Dot(transform.rotation, PlayerCamera.transform.rotation)) > .01f)
        {
            float t = elapsedTime / AnimateCameraDuration;
        
            // Use smooth interpolation (smoothstep)
            float smoothT = t * t * (3f - 2f * t);
        
            // Interpolate position and rotation
            transform.position = Vector3.Lerp(startPosition, PlayerCamera.transform.position, smoothT);
            transform.rotation = Quaternion.Slerp(startRotation, PlayerCamera.transform.rotation, smoothT);
        
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure final position and rotation are exact
        transform.position = PlayerCamera.transform.position;
        transform.rotation = PlayerCamera.transform.rotation;
        
        this.GetComponent<Camera>().enabled = false;
        PlayerCamera.GetComponent<Camera>().enabled = true;
        OnSwitchToPlayerCamera?.Invoke();
        ATrinityGameManager.SetGameFlowState(EGameFlowState.PLAY);
        Destroy(MainMenuParentObject);
        
    }
}
