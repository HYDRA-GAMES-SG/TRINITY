using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGate : MonoBehaviour
{
    public float OpenDuration = 3f;
    [HideInInspector]
    public float OpenTimer = 0f;
    
    [HideInInspector]
    public bool bShouldOpen = false;

    private bool bAudioPlayed = false;

    private Vector3 OpenPosition;
    private Vector3 ClosedPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        ClosedPosition = transform.position;
        OpenPosition = ClosedPosition + Vector3.up * 10.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (bShouldOpen)
        {
            OpenTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(ClosedPosition, OpenPosition, OpenTimer / OpenDuration);
            AudioSource audioSource = GetComponent<AudioSource>();
            
            if (!audioSource.isPlaying && !bAudioPlayed)
            {
                audioSource.Play();
                bAudioPlayed = true;
            }
        }
    }

    public void Open()
    {
        bShouldOpen = true;
    }
}
    