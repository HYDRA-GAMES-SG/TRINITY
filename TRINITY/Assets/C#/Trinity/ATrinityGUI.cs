using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class ATrinityGUI : MonoBehaviour
{

    static private ATrinityGUI Instance;

    [Header("Duration")] public float FadeInDuration = 1f;
    
    [Header("References")]
    public AEnemyHealthBar[] EnemyHealthBars = new AEnemyHealthBar[3];

    [SerializeField]
    private ATrinityMainMenu MainMenu;
    public GameObject OptionsMenu;
    public GameObject GameOver;
    public GameObject Victory;
    public GameObject Tutorials;
    public GameObject Crosshair;
    [SerializeField]
    private GameObject HUDCanvas;
    
    [Header("Sliders")]
    [SerializeField] private Image HealthSlider, ManaSlider, DamageSlider;

    [Header("UI Objects")] 
    public GameObject TriangleRotater;
    public GameObject TriangleScaler;
    public GameObject[] CurrentSpellImages = new GameObject[3];

    public Image[] CooldownFills = new Image[5];
    public Sprite[] ElementImages = new Sprite[3];
    
    public Sprite[] FireSpellImages = new Sprite[3];
    public Sprite[] ColdSpellImages = new Sprite[3];
    public Sprite[] LightningSpellImages = new Sprite[3];
    public float PlayerDamageDecayRate = 3f;
    
    [Header("Triangle Animation")]
    public float TriangleRotationRate = 300f;
    public float TriangleScaleDuration = .3f;
    public float TriangleStartScale = .78f;
    public float TriangleFinalScale = .45f;
    
    private float PlayerHealthTarget;
    private Coroutine TriangleScaleCoro;

    void Awake()
    {
        ATrinityGameManager.SetGUI(this);
        
        List<ATrinityGUI> CurrentInstances = FindObjectsOfType<ATrinityGUI>().ToList();
        
        if (CurrentInstances.Count() > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        ATrinityGameManager.SetGUI(this);

    }
    
    void Start()
    {
        HUDCanvas = transform.Find("HUDCanvas").gameObject;
        
        if (ATrinityGameManager.GetPlayerController() != null)
        {
            ATrinityGameManager.GetPlayerController().HealthComponent.OnHealthModified += UpdateHealthBar;
            ATrinityGameManager.GetPlayerController().HealthComponent.OnDeath += DisplayGameOver;
        }

        if (ATrinityGameManager.GetSpells() != null)
        {
            ATrinityGameManager.GetSpells().ManaComponent.OnManaModified += UpdateManaBar;
        }
        

        if (ATrinityGameManager.GetBrain() != null)
        {
            ATrinityGameManager.GetBrain().OnElementChanged += UpdateSpellImages;
            ATrinityGameManager.GetBrain().OnElementChanged += StartTriangleScaling;
        }

        ATrinityGameManager.GetInput().OnMenuPressed += TogglePause;

        ATrinityGameManager.GetScore().OnVictory += StartVictory;

        SetupEnemyUI();
        // SceneManager.activeSceneChanged += SetupEnemyUI;
        // EditorSceneManager.activeSceneChangedInEditMode += SetupEnemyUI;
        // EditorSceneManager.activeSceneChanged += SetupEnemyUI;
    }


    private void StartVictory(ETrinityScore score)
    {
        Victory.SetActive(true);
        Victory.GetComponent<ATrinityVictory>().ScoreText.text = ATrinityScore.GetScoreString(score);
    }
    
    private void UpdateSpellImages(ETrinityElement newElement)
    {
        Sprite[] spellImages = new Sprite [3];
        
        switch (newElement)
        {
            case ETrinityElement.ETE_Fire:
                spellImages = FireSpellImages;
                break;
            case ETrinityElement.ETE_Cold:
                spellImages = ColdSpellImages;
                break;
            case ETrinityElement.ETE_Lightning:
                spellImages = LightningSpellImages;
                break;
            default:
                break;
        }

        for (int i = 0; i < spellImages.Length; i++)
        {          
            CurrentSpellImages[i].GetComponent<Image>().sprite = spellImages[i];
        }
    }

    private void StartTriangleScaling(ETrinityElement newElement)
    {
        if (TriangleScaleCoro != null)
        {
            StopCoroutine(TriangleScaleCoro);
        }
        
        StartCoroutine(ShrinkTriangle());
    }

    private void DisplayGameOver()
    {
        GameOver.GetComponent<ATrinityGameOver>().Display();
    }

    void Update()
    {
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.MAIN_MENU)
        {
            return;
        }
        // Lerp DamageSlider to target values
        if (DamageSlider != null)
        {
            DamageSlider.fillAmount = Mathf.Lerp(DamageSlider.fillAmount, PlayerHealthTarget, PlayerDamageDecayRate * Time.deltaTime);
        }

        UpdateCooldowns();
        HandleTriangleRotation();
    }

    private void UpdateCooldowns()
    {
        CooldownFills[0].fillAmount = ATrinityGameManager.GetSpells().Blink.GetCooldownNormalized();
        CooldownFills[1].fillAmount = ATrinityGameManager.GetSpells().Forcefield.GetCooldownNormalized();
        
        switch (ATrinityGameManager.GetBrain().GetElement())
        {
            case ETrinityElement.ETE_Cold:
                CooldownFills[2].fillAmount = ATrinityGameManager.GetSpells().UtilityCold.GetCooldownNormalized();
                CooldownFills[3].fillAmount = ATrinityGameManager.GetSpells().PrimaryCold.GetCooldownNormalized();
                CooldownFills[4].fillAmount = ATrinityGameManager.GetSpells().SecondaryCold.GetCooldownNormalized();
                break;
            case ETrinityElement.ETE_Fire:
                CooldownFills[2].fillAmount = ATrinityGameManager.GetSpells().UtilityFire.GetCooldownNormalized();
                CooldownFills[3].fillAmount = ATrinityGameManager.GetSpells().PrimaryFire.GetCooldownNormalized();
                CooldownFills[4].fillAmount = ATrinityGameManager.GetSpells().SecondaryFire.GetCooldownNormalized();
                break;
            case ETrinityElement.ETE_Lightning:
                CooldownFills[2].fillAmount = ATrinityGameManager.GetSpells().UtilityLightning.GetCooldownNormalized();
                CooldownFills[3].fillAmount = ATrinityGameManager.GetSpells().PrimaryLightning.GetCooldownNormalized();
                CooldownFills[4].fillAmount = ATrinityGameManager.GetSpells().SecondaryLightning.GetCooldownNormalized();
                break;
        }
    }

    private void TogglePause()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.DEAD 
            || ATrinityGameManager.GetGameFlowState() != EGameFlowState.PAUSED 
            || ATrinityGameManager.GetGameFlowState() != EGameFlowState.MAIN_MENU)
        {
            OptionsMenu.SetActive(!OptionsMenu.activeSelf);
        }
    }
    
    public void SetupEnemyUI()
    {
        for (int i = 0; i < EnemyHealthBars.Length; i++)
        {
            EnemyHealthBars[i].gameObject.SetActive(false);
        }
        
        for (int i = 0; i < ATrinityGameManager.GetEnemyControllers().Count; i++)
        {
            EnemyHealthBars[i].gameObject.SetActive(true);
            EnemyHealthBars[i].SetEnemyController(ATrinityGameManager.GetEnemyControllers()[i]);
        }
    }

    
    public void UpdateHealthBar(float healthPercent)
    {
        if (HealthSlider != null)
        {
            HealthSlider.fillAmount = healthPercent;
            PlayerHealthTarget = HealthSlider.fillAmount; // Record the current value
        }
    }

    public void UpdateManaBar(float manaPercent)
    {
        if (ManaSlider != null)
        {
            ManaSlider.fillAmount = manaPercent;
        }
    }

    public void EnableCanvas()
    {
        if (HUDCanvas.activeSelf)
        {
            return;
        }
        
        HUDCanvas.SetActive(true);
        StartCoroutine(FadeInGUI());
    }

    private IEnumerator FadeInGUI()
    {
        float fadeTime = 0f;
        while (fadeTime < FadeInDuration)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0,1, fadeTime / FadeInDuration);
            HUDCanvas.GetComponent<CanvasGroup>().alpha = alpha;

            yield return null;
        }
    }
    
    private void HandleTriangleRotation()
    {
        float targetRotation = ATrinityGameManager.GetBrain().GetElement() switch
        {
            ETrinityElement.ETE_Fire => 0f,
            ETrinityElement.ETE_Cold => 120f,
            ETrinityElement.ETE_Lightning => 240f,
            _ => 0f
        };

        float currentRotation = TriangleRotater.transform.rotation.eulerAngles.z;
        float rotationDifference = targetRotation - currentRotation;

        if (rotationDifference < 60f)
        {
            if (TriangleScaleCoro != null)
            {
                StopCoroutine(TriangleScaleCoro);
            }
            
            TriangleScaleCoro = StartCoroutine(GrowTriangle());
        }
        
        // normalize the difference to be between -180 and 180 degrees
        if (rotationDifference > 180f)
        {
            rotationDifference -= 360f;
        }
        else if (rotationDifference < -180f)
        {
            rotationDifference += 360f;
        }

        float step = TriangleRotationRate * Time.deltaTime;
    
        // chatgpt fix: only rotate if we're not very close to target (to avoid jitter)
        if (Mathf.Abs(rotationDifference) > 0.1f)
        {
            float newRotation = currentRotation + Mathf.Clamp(rotationDifference, -step, step);
            TriangleRotater.transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
        }
    }
    
    void OnDestroy()
    {
        if (ATrinityGameManager.GetPlayerController() != null)
            ATrinityGameManager.GetPlayerController().HealthComponent.OnHealthModified -= UpdateHealthBar;

        if (ATrinityGameManager.GetSpells() != null)
            ATrinityGameManager.GetSpells().ManaComponent.OnManaModified -= UpdateManaBar;
    }

    IEnumerator ShrinkTriangle()
    {
        float startScale = TriangleScaler.transform.localScale.x;
        float growDuration = 0f;
        
        while (growDuration < TriangleScaleDuration)
        {
            growDuration += Time.deltaTime;
            float scale = Mathf.Lerp(startScale, TriangleFinalScale, growDuration / TriangleScaleDuration);
            TriangleScaler.transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    IEnumerator GrowTriangle()
    {
        float startScale = TriangleScaler.transform.localScale.x;
        float growDuration = 0f;
        while (growDuration < TriangleScaleDuration)
        {
            growDuration += Time.deltaTime;
            float scale = Mathf.Lerp(startScale, TriangleStartScale, growDuration / TriangleScaleDuration);
            TriangleScaler.transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    public void ResetGUI()
    {
        AMainMenuCamera.OnSwitchToPlayerCamera -= EnableCanvas;
        
        if (ATrinityGameManager.CurrentScene == "PORTAL")
        {
            GetMainMenu().gameObject.SetActive(true);
            GetMainMenu().MainMenuCamera.enabled = true;
            GetMainMenu().Initialize();
            Tutorials.SetActive(true);
            HUDCanvas.SetActive(false);
            AMainMenuCamera.OnSwitchToPlayerCamera += EnableCanvas;
        }
        else
        {
            GetMainMenu().MainMenuCamera.enabled = false;
            GetMainMenu().gameObject.SetActive(false);
            ATrinityGameManager.GetGUI().Tutorials.SetActive(false);
            HUDCanvas.SetActive(true);
        }

        SetupEnemyUI();
    }

    public ATrinityMainMenu GetMainMenu()
    {
        return MainMenu;
    }
}