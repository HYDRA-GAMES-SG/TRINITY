using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMainMenuCamera : MonoBehaviour
{
    public float AnimateDuration = 3f;
    public GameObject PlayerCamera;

    public static System.Action OnSwitchToPlayerCamera;
    // Start is called before the first frame update
    void Start()
    {
        PlayerCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Animate()
    {
        StartCoroutine(AnimateCoroutine());
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
            float t = elapsedTime / AnimateDuration;
        
            // Use smooth interpolation (smoothstep)
            float smoothT = t * t * (3f - 2f * t);
        
            // Interpolate position and rotation
            transform.position = Vector3.Lerp(startPosition, PlayerCamera.transform.position, smoothT);
            transform.rotation = Quaternion.Slerp(startRotation, PlayerCamera.transform.rotation, smoothT);
        
            elapsedTime += Time.deltaTime;

            print(smoothT);
            yield return null;
        }

        // Ensure final position and rotation are exact
        transform.position = PlayerCamera.transform.position;
        transform.rotation = PlayerCamera.transform.rotation;
        
        this.GetComponent<Camera>().enabled = false;
        PlayerCamera.SetActive(true);
        OnSwitchToPlayerCamera?.Invoke();
    }
}
