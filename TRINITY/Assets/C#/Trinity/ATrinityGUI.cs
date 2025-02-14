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
    public GameObject Videos;
    public GameObject Crosshair;
    [SerializeField]
    private GameObject HUDCanvas;
    
    [Header("Sliders")]
    [SerializeField] private Image HealthSlider, ManaSlider, ManaBar, DamageSlider;

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

    private Color InitialColor;
    private Color FinalColor;
    public float LerpColorDuration;
    public float PingPongNoManaTime;
    private float PingPongNoManaTimer;

    public AudioClip NoMana;
    public AudioClip SpellNotReady;
    private AudioSource GUISource;
    public bool IsOptionsMenuOpen() => OptionsMenu.activeSelf;
    public bool IsGameOverOpen() => GameOver.activeSelf;
    public bool IsVictoryOpen() => Victory.activeSelf;
    
    public ATrinityMainMenu GetMainMenu() => MainMenu;
    public ATrinityGameOver GetGameOver() => GameOver.GetComponent<ATrinityGameOver>();
    public ATrinityVictory GetVictory() => Victory.GetComponent<ATrinityVictory>();
    public ATrinityOptions GetOptions() => OptionsMenu.GetComponent<ATrinityOptions>();
    public ATrinityVideos GetVideos() => Videos.GetComponent<ATrinityVideos>();
    
    void Awake()
    {     
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
        GUISource = GetComponent<AudioSource>();
        InitialColor = ManaBar.color;
        FinalColor = Color.red;
        HUDCanvas = transform.Find("HUDCanvas").gameObject;
        ATrinityGameManager.GetScore().OnVictory += StartVictory;
        ATrinityGameManager.OnGameFlowStateChanged += BindToEvents;

        ResetGUI();
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

    public void DisplayGameOver()
    {
        GameOver.SetActive(true);
        GameOver.GetComponent<ATrinityGameOver>().Display();
    }

    void Update()
    {
        if (ATrinityGameManager.GetInput().MenuInput)
        {
            ToggleOptions();
        }
        
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }
        
        // Lerp DamageSlider to target values
        if (DamageSlider != null)
        {
            DamageSlider.fillAmount = Mathf.Lerp(DamageSlider.fillAmount, PlayerHealthTarget, PlayerDamageDecayRate * Time.deltaTime);
        }
        PingPongNoManaTimer -= Time.deltaTime;
        if (ManaBar != null && PingPongNoManaTimer > 0)
        {
            ManaBar.color = Color.Lerp(InitialColor, FinalColor, Mathf.PingPong(Time.time, LerpColorDuration) / LerpColorDuration);
        }
        else 
        {
            ManaBar.color = InitialColor;
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


    public void ToggleOptions()
    {
        ATrinityGameManager.GetInput().NullifyInputs();
        OptionsMenu.SetActive(!IsOptionsMenuOpen());

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
    public void ShowNoMana() 
    {
        if (ManaBar != null) 
        {
            GUISource.PlayOneShot(NoMana);
            PingPongNoManaTimer = PingPongNoManaTime;
        }
    }
    public void SpellOnCooldown() 
    {
        if (GUISource != null) 
        {
            GUISource.PlayOneShot(SpellNotReady);
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
        
        GameOver.SetActive(false);
        OptionsMenu.SetActive(false);
        HUDCanvas.SetActive(GetMainMenu().bCanSkipMainMenu);
        
        if (ATrinityGameManager.CurrentScene == "PORTAL")
        {
            GetMainMenu().gameObject.SetActive(!GetMainMenu().bCanSkipMainMenu);
            GetMainMenu().MainMenuCamera.gameObject.SetActive(!GetMainMenu().bCanSkipMainMenu);
            Videos.SetActive(true);
            AMainMenuCamera.OnSwitchToPlayerCamera += EnableCanvas;
            
            if (GetMainMenu().bCanSkipMainMenu)
            {
                GetMainMenu().gameObject.SetActive(false);
                ATrinityGameManager.SetGameFlowState(EGameFlowState.PLAY);
            }
            else
            {
                ATrinityGameManager.SetGameFlowState(EGameFlowState.MAIN_MENU);
            }
        }
        else
        {
            GetMainMenu().MainMenuCamera.gameObject.SetActive(false);
            GetMainMenu().gameObject.SetActive(false);
            ATrinityGameManager.GetGUI().Videos.SetActive(false);
            HUDCanvas.SetActive(true);
        }

        BindToEvents(ATrinityGameManager.GetGameFlowState());
        
        SetupEnemyUI();
    }

    public void BindToEvents(EGameFlowState newGFS)
    {

        UnbindAll();
        
        switch (newGFS)
        {
            case EGameFlowState.MAIN_MENU:
                BindMainMenuEvents(true);
                break;
            case EGameFlowState.PLAY:
                BindPlayEvents(true);
                break;
            case EGameFlowState.PAUSED:
                BindOptionsEvents(true);
                break;
            case EGameFlowState.VICTORY:
                BindVictoryEvents(true);
                break;
            case EGameFlowState.DEAD:
                BindGameOverEvents(true);
                break;
        }
    }

    public void UnbindAll()
    {
        BindMainMenuEvents(false);
        BindPlayEvents(false);
        BindOptionsEvents(false);
        BindVictoryEvents(false);
        BindGameOverEvents(false);
    }
    
    public void BindMainMenuEvents(bool bBind)
    {
        if (bBind)
        {
            //input events
            ATrinityGameManager.GetInput().OnJumpGlidePressed += GetMainMenu().Select;
            ATrinityGameManager.GetInput().OnElementPressed += GetMainMenu().NavigateByElement;
            ATrinityGameManager.GetInput().OnNextElementPressed += GetMainMenu().NavigateForwards;
            ATrinityGameManager.GetInput().OnPreviousElementPressed += GetMainMenu().NavigateBackwards;
            ATrinityGameManager.GetInput().OnElementalPrimaryPressed += GetMainMenu().Select;
            ATrinityGameManager.GetInput().OnForcefieldPressed += GetMainMenu().CloseOptions;
            GetMainMenu().OnMenuElementChanged += ATrinityGameManager.GetGraphics().UpdateStaffAura;
            GetMainMenu().OnMenuElementChanged += ATrinityGameManager.GetGraphics().UpdateMeshColor;
            
            //audio events
            GetMainMenu().OnMainMenuNavigate += ATrinityGameManager.GetAudio().PlayMainMenuNavigate;
            GetMainMenu().OnMainMenuSelection += ATrinityGameManager.GetAudio().PlayMainMenuSelect;
        }
        else
        {
            //input events
            ATrinityGameManager.GetInput().OnJumpGlidePressed -= GetMainMenu().Select;
            ATrinityGameManager.GetInput().OnElementPressed -= GetMainMenu().NavigateByElement;
            ATrinityGameManager.GetInput().OnNextElementPressed -= GetMainMenu().NavigateForwards;
            ATrinityGameManager.GetInput().OnPreviousElementPressed -= GetMainMenu().NavigateBackwards;
            ATrinityGameManager.GetInput().OnElementalPrimaryPressed -= GetMainMenu().Select;
            ATrinityGameManager.GetInput().OnForcefieldPressed -= GetMainMenu().CloseOptions;
            
            //audio events
            GetMainMenu().OnMainMenuNavigate -= ATrinityGameManager.GetAudio().PlayMainMenuNavigate;
            GetMainMenu().OnMainMenuSelection -= ATrinityGameManager.GetAudio().PlayMainMenuSelect;
        }
    }
    
    public void BindPlayEvents(bool bBind)
    {
        if (bBind)
        {
            ATrinityGameManager.GetBrain().OnElementChanged += UpdateSpellImages;
            ATrinityGameManager.GetBrain().OnElementChanged += StartTriangleScaling;   
        }
        else
        {
            ATrinityGameManager.GetBrain().OnElementChanged -= UpdateSpellImages;
            ATrinityGameManager.GetBrain().OnElementChanged -= StartTriangleScaling;
        }
    }
    
    public void BindOptionsEvents(bool bBind)
    {
        if (bBind)
        {
            ATrinityGameManager.GetInput().OnMovePressed += GetOptions().Navigate;
            ATrinityGameManager.GetInput().OnJumpGlidePressed += GetOptions().PressInteractable;
            ATrinityGameManager.GetBrain().OnElementChanged -= UpdateSpellImages;
            ATrinityGameManager.GetBrain().OnElementChanged -= StartTriangleScaling;
        }
        else
        {
            ATrinityGameManager.GetInput().OnMovePressed -= GetOptions().Navigate;
            ATrinityGameManager.GetInput().OnJumpGlidePressed -= GetOptions().PressInteractable;
        }
    }

    public void BindGameOverEvents(bool bBind)
    {
        if (bBind)
        {
            ATrinityGameManager.GetInput().OnJumpGlidePressed += GetGameOver().Restart;
            ATrinityGameManager.GetInput().OnForcefieldPressed += GetGameOver().Return;
        }
        else
        {
            ATrinityGameManager.GetInput().OnJumpGlidePressed -= GetGameOver().Restart;
            ATrinityGameManager.GetInput().OnForcefieldPressed -= GetGameOver().Return;
        }
    }

    public void BindVictoryEvents(bool bBind)
    {
        if (bBind)
        {
            ATrinityGameManager.GetInput().OnMenuPressed += GetVictory().Close;
            ATrinityGameManager.GetInput().OnElementalPrimaryPressed += GetVictory().Close;
            ATrinityGameManager.GetInput().OnJumpGlidePressed += GetVictory().Close;
            ATrinityGameManager.GetInput().OnForcefieldPressed += GetVictory().Close;
        }
        else
        {
            ATrinityGameManager.GetInput().OnMenuPressed -= GetVictory().Close;
            ATrinityGameManager.GetInput().OnElementalPrimaryPressed -= GetVictory().Close;
            ATrinityGameManager.GetInput().OnJumpGlidePressed -= GetVictory().Close;
            ATrinityGameManager.GetInput().OnForcefieldPressed -= GetVictory().Close; 
        }
    }
}