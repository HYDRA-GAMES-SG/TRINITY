using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ATrinityVideos : MonoBehaviour
{
    public List<GameObject> TutorialParents; // list of tutorial parents
    
    private Image[] VideoBackgroundImages;
    private GameObject CurrentTutorialParent;
    public GameObject RenderTextureCube;
    public Texture2D[] TutorialRenderTextures;
    [HideInInspector] 
    public VideoPlayer CurrentVideoPlayer;
    private TextMeshProUGUI[] CurrentTextElements;
    
    void Awake()
    {
        VideoBackgroundImages = transform.Find("Background").GetComponentsInChildren<Image>();
    }
    
    public void PlayTutorial(int tutorialVideoIndex)
    {
        if (CurrentTutorialParent != null)
        {
            StopTutorial();
        }
        CurrentTutorialParent = TutorialParents[tutorialVideoIndex];

        if (RenderTextureCube != null)
        {
            RenderTextureCube.GetComponent<MeshRenderer>().material.mainTexture = TutorialRenderTextures[tutorialVideoIndex];
        }
        else
        {
            RenderTextureCube = FindObjectOfType<USpellPickupComponent>().gameObject;
            
        }
        
        if (CurrentTutorialParent != null)
        {
            CurrentTutorialParent.SetActive(true);
            CurrentVideoPlayer = CurrentTutorialParent.GetComponentInChildren<VideoPlayer>();
            if (CurrentVideoPlayer != null)
            {
                CurrentVideoPlayer.isLooping = true;
                CurrentVideoPlayer.targetCameraAlpha = 1;
                CurrentVideoPlayer.Play();
            }
            CurrentTextElements = CurrentTutorialParent.GetComponentsInChildren<TextMeshProUGUI>();

            if (VideoBackgroundImages != null)
            {
                foreach (Image img in VideoBackgroundImages)
                {
                    Color color = img.color;
                    color.a = 1f;
                    img.color = color;
                }
            }
            if (CurrentTextElements != null)
            {
                foreach (var textElement in CurrentTextElements)
                {
                    if (textElement != null)
                    {
                        var color = textElement.color;
                        color.a = 1;
                        textElement.color = color;
                    }
                }
            }
        }
    }
    
    public void StopTutorial()
    {
        if (CurrentTutorialParent != null)
        {
            if (CurrentVideoPlayer != null)
            {
                CurrentVideoPlayer.Stop();
                CurrentVideoPlayer.targetCameraAlpha = 0;
            }
            if (VideoBackgroundImages != null)
            {
                foreach (Image img in VideoBackgroundImages)
                {
                    Color color = img.color;
                    color.a = 0f;
                    img.color = color;
                }
            }
            if (CurrentTextElements != null)
            {
                foreach (var textElement in CurrentTextElements)
                {
                    if (textElement != null)
                    {
                        var color = textElement.color;
                        color.a = 0;
                        textElement.color = color;
                    }
                }
            }
            CurrentTutorialParent.SetActive(false);
            CurrentTutorialParent = null;
            CurrentVideoPlayer = null;
            CurrentTextElements = null;
        }
    }
}