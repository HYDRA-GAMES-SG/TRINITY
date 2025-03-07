using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMainMenuGate : MonoBehaviour
{
    public float CloseDuration = 3f;
    [HideInInspector]
    public float CloseTimer = 0f;
    
    [HideInInspector]
    public bool bShouldClose = false;

    public bool bShouldOpen = false;

    private bool bAudioPlayed = false;

    private Vector3 ClosedPos;
    private Vector3 OpenPos;
    
    // Start is called before the first frame update
    void Start()
    {
        OpenPos = transform.position;
        ClosedPos = OpenPos + Vector3.down * 6f;
    }

    // Update is called once per frame
    void Update()
    {
        if (bShouldClose)
        {
            CloseTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(OpenPos, ClosedPos, CloseTimer / CloseDuration);
            AudioSource audioSource = GetComponent<AudioSource>();
            
            if (!audioSource.isPlaying && !bAudioPlayed)
            {
                audioSource.Play();
                bAudioPlayed = true;
            }
        }

        if (bShouldOpen)
        {
            CloseTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(ClosedPos, OpenPos, CloseTimer / CloseDuration);
            AudioSource audioSource = GetComponent<AudioSource>();
            
            if (!audioSource.isPlaying && !bAudioPlayed)
            {
                audioSource.Play();
                bAudioPlayed = true;
            }
        }
    }

    public void Close()
    {
        bShouldClose = true;
        bShouldOpen = false;
    }

    public void Open()
    {
        bShouldClose = false;
        bShouldOpen = true;
        CloseTimer = 0f;
    }
}