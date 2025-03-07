using System.Collections;
using System.Collections.Generic;
using System.Net;
using ThirdPersonCamera;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ATrinityOptions : MonoBehaviour
{
    public static System.Action OnOptionsMenuButton;
    public static System.Action OnOptionsMenuSlider;
    public static System.Action OnOptionsMenuToggle;
    public static System.Action OnOptionsMenuNavigate;

    public Toggle CrossHairToggle;
    public Slider GamepadSensitivity;
    public Slider MouseSensitivity;
    public Slider MasterVolume;
    public Button QuitButton;
    public GameObject TutorialButton;

    public Image CrossHair;
    public bool bEnableCrosshair = true;
    private float NavigateCooldownTimer = 0f;
    public float NavigateCooldown = .2f;
    public float InputThreshold = 0f;

    public AMainMenuGate MainMenuGate;

    [Header("Menu Items")]
    public Selectable[] MenuElements;  // Array of UI elements (buttons, etc.)
    private int CurrentMenuElementsIndex = 0;


    void Start()
    {
        // Set initial selection
        if (MenuElements != null && MenuElements.Length > 0)
        {
            SelectMenuItem(0);
        }
    }

    public void PressInteractable()
    {
        if (NavigateCooldownTimer > 0f)
        {
            return;
        }

        if (MenuElements[CurrentMenuElementsIndex] is Toggle)
        {
            Toggle currentToggle = MenuElements[CurrentMenuElementsIndex] as Toggle;

            currentToggle.isOn = !currentToggle.isOn;
            NavigateCooldownTimer = NavigateCooldown;
            OnOptionsMenuToggle?.Invoke();
            ATrinityGameManager.SerializeSettings(MakeGameSettings());
            ATrinityGameManager.DeserializeSettings();
            CrossHair.gameObject.SetActive(false);
        }

        if (MenuElements[CurrentMenuElementsIndex] is Button)
        {
            Button currentButton = MenuElements[CurrentMenuElementsIndex] as Button;

            currentButton.onClick?.Invoke();
            NavigateCooldownTimer = NavigateCooldown;
            OnOptionsMenuButton?.Invoke();
            ATrinityGameManager.SerializeSettings(MakeGameSettings());
            ATrinityGameManager.DeserializeSettings();

        }
    }

    void OnEnable()
    {

        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.MAIN_MENU)
        {
            Time.timeScale = 0f;
            ATrinityGameManager.SetGameFlowState(EGameFlowState.PAUSED);
        }

        // Ensure proper initial selection when menu is enabled
        if (MenuElements != null && MenuElements.Length > 0)
        {
            SelectMenuItem(CurrentMenuElementsIndex);
        }

        if (ATrinityGameManager.CurrentScene == "PORTAL" && QuitButton != null)
        {
            QuitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit";
        }
        else if (QuitButton != null)
        {
            QuitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit To Mage's Gate";
        }

        if (CrossHair)
        {
            CrossHair.gameObject.SetActive(false);
        }

        NavigateCooldownTimer = 0f;

        BindAudioEvents(true);
    }
    private FGameSettings MakeGameSettings()
    {
        FGameSettings newSettings = new FGameSettings(
            CrossHairToggle.isOn,
            GamepadSensitivity.value,
            MouseSensitivity.value,
            MasterVolume.value
        );

        return newSettings;
    }


    void OnDisable()
    {

        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.MAIN_MENU)
        {
            Time.timeScale = 1f;
            ATrinityGameManager.SetGameFlowState(EGameFlowState.PLAY);
        }

        CrossHair.gameObject.SetActive(ATrinityGameManager.CROSSHAIR_ENABLED);
        
        BindAudioEvents(false);
        ATrinityGameManager.GetInput().NullifyInputs();
    }
    
    public void SkipTutorial()
    {
        ATrinityGameManager.GetPlayerController().ResetPlayer();
        
        if (MainMenuGate != null)
        {
            MainMenuGate.Open();
        }
        
        TutorialButton.SetActive(false);
        
        ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu = true;
        
        ATrinityGameManager.GetPlayerController().ResetPlayer();
        
        OnReturnToGameClicked();
    }
    
    public void Navigate()
    {
        if (NavigateCooldownTimer > 0f)
        {
            return;
        }

        Vector2 moveInput = ATrinityGameManager.GetInput().MoveInput;

        // Only process input if it exceeds the threshold
        if (Mathf.Abs(moveInput.y) >= InputThreshold)
        {
            int newIndex = CurrentMenuElementsIndex;

            if (moveInput.y > 0) // Up
            {
                newIndex--;
                if (newIndex < 0)
                {
                    if (ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu)
                    {
                        newIndex = MenuElements.Length - 2;
                    }
                    else
                    {
                        newIndex = MenuElements.Length - 1; // Wrap to bottom
                    }
                }
            }
            else if (moveInput.y < 0) // Down
            {
                newIndex++;
                if (newIndex >= MenuElements.Length || newIndex == 5 && ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu)
                {
                    newIndex = 0; // Wrap to top
                }
            }

            if (newIndex != CurrentMenuElementsIndex)
            {
                SelectMenuItem(newIndex);
                NavigateCooldownTimer = NavigateCooldown;
                OnOptionsMenuNavigate?.Invoke();

            }
        }

        // Optional: Handle horizontal navigation if needed
        else if (Mathf.Abs(moveInput.x) >= InputThreshold)
        {
            if (MenuElements[CurrentMenuElementsIndex] is Slider)
            {
                Slider currentSlider = MenuElements[CurrentMenuElementsIndex] as Slider;
                if (moveInput.x > 0)
                {
                    currentSlider.value += .1f;
                    ATrinityGameManager.SerializeSettings(MakeGameSettings());
                    ATrinityGameManager.DeserializeSettings();
                }
                else
                {
                    currentSlider.value -= .1f;
                    ATrinityGameManager.SerializeSettings(MakeGameSettings());
                    ATrinityGameManager.DeserializeSettings();
                }

                OnOptionsMenuSlider?.Invoke();
            }
            // Add horizontal Snavigation logic here if needed
            NavigateCooldownTimer = NavigateCooldown;
        }
    }

    private void SelectMenuItem(int index)
    {
        // Deselect current item
        if (CurrentMenuElementsIndex >= 0 && CurrentMenuElementsIndex < MenuElements.Length)
        {
            MenuElements[CurrentMenuElementsIndex].OnDeselect(null);
        }

        // Select new item
        CurrentMenuElementsIndex = index;
        MenuElements[CurrentMenuElementsIndex].Select();
        MenuElements[CurrentMenuElementsIndex].OnSelect(null);
    }

    void Update()
    {
        NavigateCooldownTimer -= Time.unscaledDeltaTime;
        if (ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu)
        {
            TutorialButton.SetActive(false);
        }
    }

    public void OnCrossHairToggle(bool bToggle)
    {
        PlayerPrefs.SetInt("bCrossHairEnabled", bToggle ? 1 : 0);
        ATrinityGameManager.CROSSHAIR_ENABLED = PlayerPrefs.GetInt("bCrossHairEnabled") > 0;
    }

    public void OnReturnToGameClicked()
    {
        this.gameObject.SetActive(false);
    }

    public void OnQuitClicked()
    {
        BindAudioEvents(false);
        if (ATrinityGameManager.CurrentScene != "PORTAL")
        {
            ATrinityGameManager.LoadScene("PORTAL");
        }
        else
        {
            Application.Quit();
        }
    }

    public void BindAudioEvents(bool bBind)
    {
        if (bBind)
        {
            //audio events
            ATrinityOptions.OnOptionsMenuSlider += ATrinityGameManager.GetAudio().PlayOptionsMenuSlider;
            ATrinityOptions.OnOptionsMenuToggle += ATrinityGameManager.GetAudio().PlayOptionsMenuToggle;
            ATrinityOptions.OnOptionsMenuButton += ATrinityGameManager.GetAudio().PlayOptionsMenuButton;
            ATrinityOptions.OnOptionsMenuNavigate += ATrinityGameManager.GetAudio().PlayOptionsMenuNavigate;
        }
        else
        {
            //audio events
            OnOptionsMenuSlider -= ATrinityGameManager.GetAudio().PlayOptionsMenuSlider;
            OnOptionsMenuToggle -= ATrinityGameManager.GetAudio().PlayOptionsMenuToggle;
            OnOptionsMenuButton -= ATrinityGameManager.GetAudio().PlayOptionsMenuButton;
            OnOptionsMenuNavigate -= ATrinityGameManager.GetAudio().PlayOptionsMenuNavigate;
        }
    }
}
