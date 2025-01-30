using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ATrinityOptions : MonoBehaviour
{
    public Toggle CrossHairToggle;
    public Slider GamepadSensitivity;
    public Slider MouseSensitivity;
    public Slider MasterVolume;
    
    public Image CrossHair;
    public bool bEnableCrosshair = true;
    private float NavigateCooldownTimer = 0f;
    public float NavigateCooldown = .2f;
    public float InputThreshold = 0f;
    
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

    private void PressInteractable()
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
        }
    }

    void OnEnable()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.MAIN_MENU)
        {
            Time.timeScale = 0f;
            ATrinityGameManager.SetGameFlowState(EGameFlowState.PAUSED);
        }

        //ATrinityGameManager.GetInput().OnMovePressed += Navigate;
        
        // Ensure proper initial selection when menu is enabled
        if (MenuElements != null && MenuElements.Length > 0)
        {
            SelectMenuItem(CurrentMenuElementsIndex);
        }

        if (CrossHair)
        {
            CrossHair.gameObject.SetActive(false);
        }
            
        NavigateCooldownTimer = 0f;
        
        ATrinityGameManager.GetInput().OnMovePressed += Navigate;
        ATrinityGameManager.GetInput().OnJumpGlidePressed += PressInteractable;
    }
    
    

    void OnDisable()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.MAIN_MENU)
        {
            Time.timeScale = 1f;
            ATrinityGameManager.SetGameFlowState(EGameFlowState.PLAY);
        }
        //ATrinityGameManager.GetInput().OnMovePressed -= Navigate;
        
        ATrinityGameManager.GetInput().OnMovePressed -= Navigate;
        ATrinityGameManager.GetInput().OnJumpGlidePressed -= PressInteractable;

        FGameSettings newSettings = new FGameSettings(
            CrossHairToggle.isOn,
            GamepadSensitivity.value,
            MouseSensitivity.value,
            MasterVolume.value
        );

        ATrinityGameManager.SerializeSettings(newSettings);

        if (CrossHair)
        {
            CrossHair.gameObject.SetActive(ATrinityGameManager.CROSSHAIR_ENABLED);
        }
    }

    private void Navigate()
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
                    newIndex = MenuElements.Length - 1; // Wrap to bottom
                }
            }
            else if (moveInput.y < 0) // Down
            {
                newIndex++;
                if (newIndex >= MenuElements.Length)
                {
                    newIndex = 0; // Wrap to top
                }
            }

            if (newIndex != CurrentMenuElementsIndex)
            {
                SelectMenuItem(newIndex);
                NavigateCooldownTimer = NavigateCooldown;
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
                }
                else
                {
                    currentSlider.value -= .1f;
                }
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
    }

    public void OnCrossHairToggle(bool bToggle)
    {
        ATrinityGameManager.CROSSHAIR_ENABLED = bToggle;
    }

    public void OnReturnToGameClicked()
    {
        this.gameObject.SetActive(false);
    }

    public void OnQuitClicked()
    {
        if (SceneManager.GetActiveScene().name != "PORTAL")
        {
            SceneManager.LoadScene("PORTAL");
        }
        else
        {
            Application.Quit();
        }
    }
}
