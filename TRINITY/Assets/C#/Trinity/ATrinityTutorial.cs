using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ATrinityTutorial : MonoBehaviour
{
    public List<GameObject> TutorialParents;
    public float DisplayDuration = 5f;
    public float FadeDuration = 1.0f;
    
    private ATrinityInput InputReference;
    private Image[] VideoBackgroundImages;
    private GameObject CurrentTutorialParent;
    private VideoPlayer CurrentVideoPlayer;
    private TextMeshProUGUI[] CurrentTextElements;
    private float FadeTimer;
    private float DisplayTimer;
    private bool bIsFading;
    private bool bIsDisplaying;

    void Awake()
    {
        VideoBackgroundImages = transform.Find("Background").GetComponentsInChildren<Image>();
    }
    
    void Start()
    {
        
        if (SceneManager.GetActiveScene().name != "PORTAL")
        {
            Destroy(this.gameObject);
            return;
        }
        
        InputReference = ATrinityGameManager.GetInput();
        BindToInputEvents(true);
    }

    void Update()
    {
        if (bIsDisplaying)
        {
            DisplayTimer += Time.deltaTime;
            if (DisplayTimer >= DisplayDuration)
            {
                bIsDisplaying = false;
                bIsFading = true;
                FadeTimer = 0f;
            }
        }
        
        if (bIsFading)
        {
            FadeTimer += Time.deltaTime;
            float alpha = 1 - (FadeTimer / FadeDuration);
            
            if (alpha <= 0)
            {
                ClearTutorial();
                bIsFading = false;
                return;
            }

            if (VideoBackgroundImages != null)
            {
                foreach (Image img in VideoBackgroundImages)
                {
                    Color color = img.color;
                    color.a = alpha;
                    img.color = color;
                }
            }
            
            // Fade VideoPlayers
            if (CurrentVideoPlayer != null)
            {
                CurrentVideoPlayer.targetCameraAlpha = alpha;
            }

            // Fade TextMeshPro elements
            if (CurrentTextElements != null)
            {
                foreach (var textElement in CurrentTextElements)
                {
                    if (textElement != null)
                    {
                        Color color = textElement.color;
                        color.a = alpha;
                        textElement.color = color;
                    }
                }
            }
        }
    }

    private void HandleDisplayTime()
    {
        if (CurrentTutorialParent != null)
        {
            CurrentVideoPlayer = CurrentTutorialParent.GetComponentInChildren<VideoPlayer>();
            CurrentTextElements = CurrentTutorialParent.GetComponentsInChildren<TextMeshProUGUI>();
            DisplayTimer = 0f;
            bIsDisplaying = true;
            bIsFading = false;
            
            
            if (CurrentTutorialParent != null)
            {
                if (VideoBackgroundImages != null)
                {
                    foreach (Image img in VideoBackgroundImages)
                    {
                        Color color = img.color;
                        color.a = 1f;
                        img.color = color;
                    }
                }
                
                // Reset VideoPlayer alpha
                if (CurrentVideoPlayer != null)
                {
                    CurrentVideoPlayer.targetCameraAlpha = 1;
                }

                // Reset TextMeshPro alphas
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
    }

    private void ClearTutorial()
    {
        if (CurrentTutorialParent != null)
        {
            if (VideoBackgroundImages != null)
            {
                foreach (Image img in VideoBackgroundImages)
                {
                    Color color = img.color;
                    color.a = 0f;
                    img.color = color;
                }
            }
            
            // Reset VideoPlayer alpha
            if (CurrentVideoPlayer != null)
            {
                CurrentVideoPlayer.targetCameraAlpha = 0;
            }

            // Reset TextMeshPro alphas
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
            bIsDisplaying = false;
            bIsFading = false;
        }
    }

    private void OnDestroy()
    {
        BindToInputEvents(false);
    }
    
    private void DisplayElementTutorial()
    {
        ClearTutorial();
        switch (ATrinityGameManager.GetBrain().GetElement())
        {
            case ETrinityElement.ETE_Fire:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "ElementFireParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Cold:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "ElementColdParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Lightning:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "ElementLightningParent");
                CurrentTutorialParent.SetActive(true);
                break;
            default:
                break;
        }
        HandleDisplayTime();
    }
    
    private void DisplayElementTutorial(ETrinityElement newElement)
    {
        ClearTutorial();
        switch (ATrinityGameManager.GetBrain().GetElement())
        {
            case ETrinityElement.ETE_Fire:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "ElementFireParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Cold:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "ElementColdParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Lightning:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "ElementLightningParent");
                CurrentTutorialParent.SetActive(true);
                break;
            default:
                break;
        }
        HandleDisplayTime();
    }
    
    private void DisplayPrimaryTutorial()
    {
        ClearTutorial();
        switch (ATrinityGameManager.GetBrain().GetElement())
        {
            case ETrinityElement.ETE_Fire:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "PrimaryFireParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Cold:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "PrimaryColdParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Lightning:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "PrimaryLightningParent");
                CurrentTutorialParent.SetActive(true);
                break;
            default:
                break;
        }
        HandleDisplayTime();
    }
    
    private void DisplaySecondaryTutorial()
    {
        ClearTutorial();
        switch (ATrinityGameManager.GetBrain().GetElement())
        {
            case ETrinityElement.ETE_Fire:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "SecondaryFireParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Cold:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "SecondaryColdParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Lightning:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "SecondaryLightningParent");
                CurrentTutorialParent.SetActive(true);
                break;
            default:
                break;
        }
        HandleDisplayTime();
    }
    
    private void DisplayUtilityTutorial()
    {
        ClearTutorial();
        switch (ATrinityGameManager.GetBrain().GetElement())
        {
            case ETrinityElement.ETE_Fire:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "UtilityFireParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Cold:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "UtilityColdParent");
                CurrentTutorialParent.SetActive(true);
                break;
            case ETrinityElement.ETE_Lightning:
                CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "UtilityLightningParent");
                CurrentTutorialParent.SetActive(true);
                break;
            default:
                break;
        }
        HandleDisplayTime();
    }

    private void DisplayBlinkTutorial()
    {
        ClearTutorial();
        CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "BlinkParent");
        CurrentTutorialParent.SetActive(true);
        HandleDisplayTime();
    }

    private void DisplayForcefieldTutorial()
    {
        ClearTutorial();
        CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "ForcefieldParent");
        CurrentTutorialParent.SetActive(true);
        HandleDisplayTime();
    }

    private void DisplayJumpTutorial()
    {
        ClearTutorial();
        CurrentTutorialParent = TutorialParents.Find(transformObj => transformObj.name == "JumpParent");
        CurrentTutorialParent.SetActive(true);
        HandleDisplayTime();
    }
    
    void BindToInputEvents(bool bBind)
    {
        if (bBind)
        {
            InputReference.OnElementalPrimaryPressed += DisplayPrimaryTutorial;
            InputReference.OnElementalSecondaryPressed += DisplaySecondaryTutorial;
            InputReference.OnElementalUtiltiyPressed += DisplayUtilityTutorial;
            
            InputReference.OnElementPressed += DisplayElementTutorial;
            InputReference.OnNextElementPressed += DisplayElementTutorial;
            InputReference.OnPreviousElementPressed += DisplayElementTutorial;
            
            InputReference.OnBlinkPressed += DisplayBlinkTutorial;
            InputReference.OnForcefieldPressed += DisplayForcefieldTutorial;
            
            InputReference.OnJumpGlidePressed += DisplayJumpTutorial;
        }
        else
        {
            InputReference.OnElementalPrimaryPressed -= DisplayPrimaryTutorial;
            InputReference.OnElementalSecondaryPressed -= DisplaySecondaryTutorial;
            InputReference.OnElementalUtiltiyPressed -= DisplayUtilityTutorial;
            
            InputReference.OnElementPressed -= DisplayElementTutorial;
            InputReference.OnNextElementPressed -= DisplayElementTutorial;
            InputReference.OnPreviousElementPressed -= DisplayElementTutorial;
            
            InputReference.OnBlinkPressed -= DisplayBlinkTutorial;
            InputReference.OnForcefieldPressed -= DisplayForcefieldTutorial;
            
            InputReference.OnJumpGlidePressed -= DisplayJumpTutorial;
        }
    }
}