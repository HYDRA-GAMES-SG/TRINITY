using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class APauseMenu : MonoBehaviour
{
    public Image CrossHair;
    public bool bEnableCrosshair = true;
    private float NavigateCooldownTimer = 0f;
    public float NavigateCooldown = .2f;
    public float InputThreshold = .2f;
    
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

    void OnEnable()
    {
        Time.timeScale = 0f;
        ATrinityGameManager.GAME_PAUSED = true;
        ATrinityGameManager.GetInput().OnMovePressed += Navigate;
        
        // Ensure proper initial selection when menu is enabled
        if (MenuElements != null && MenuElements.Length > 0)
        {
            SelectMenuItem(CurrentMenuElementsIndex);
        }

        CrossHair.gameObject.SetActive(false);
    }
    
    

    void OnDisable()
    {
        Time.timeScale = 1f;
        ATrinityGameManager.GAME_PAUSED = false;
        ATrinityGameManager.GetInput().OnMovePressed -= Navigate;
        CrossHair.gameObject.SetActive(ATrinityGameManager.CROSSHAIR_ENABLED);
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
            // Add horizontal navigation logic here if needed
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
}
