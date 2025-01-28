using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(ATrinityGameManager))]
[RequireComponent(typeof(ATrinityInput))]
public class ATrinityMainMenu : MonoBehaviour
{
    public float RotationSpeed = 240f;
    public RectTransform ElementTriangle;
    private EMainMenu MainMenuSelection;
    private bool bOptionsMenu = false;
    public GameObject OptionsMenu;
    private bool bRotating = false;
    
    // Start is called before the first frame update
    void Start()
    {
        ATrinityGameManager.SetGameFlowState(EGameFlowState.MAIN_MENU);
        MainMenuSelection = EMainMenu.EMM_Start;
        ATrinityGameManager.GetInput().OnElementPressed += NavigateByElement;
        ATrinityGameManager.GetInput().OnJumpGlidePressed += Select;
        ATrinityGameManager.GetInput().OnNextElementPressed += NavigateForwards;
        ATrinityGameManager.GetInput().OnPreviousElementPressed += NavigateBackwards;
        bOptionsMenu = false;
        bRotating = false;
    }

    public void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        float targetRotation = MainMenuSelection switch
        {
            EMainMenu.EMM_Start => 0f,
            EMainMenu.EMM_Options => 120f,
            EMainMenu.EMM_Quit => 240f,
            _ => 0f
        };

        float currentRotation = ElementTriangle.rotation.eulerAngles.z;
        float rotationDifference = targetRotation - currentRotation;
    
        // normalize the difference to be between -180 and 180 degrees
        if (rotationDifference > 180f)
        {
            rotationDifference -= 360f;
        }
        else if (rotationDifference < -180f)
        {
            rotationDifference += 360f;
        }

        float step = RotationSpeed * Time.deltaTime;
    
        // chatgpt fix: only rotate if we're not very close to target (to avoid jitter)
        if (Mathf.Abs(rotationDifference) > 0.1f)
        {
            float newRotation = currentRotation + Mathf.Clamp(rotationDifference, -step, step);
            ElementTriangle.rotation = Quaternion.Euler(0f, 0f, newRotation);
            bRotating = true;
        }
        else
        {
            bRotating = false;
        }
    }
    
    private void Select()
    {
        if (bOptionsMenu || bRotating)
        {
            //handle options menu
            return;
        }
        
        switch (MainMenuSelection)
        {
            case EMainMenu.EMM_Start:
                SceneManager.LoadScene("PORTAL");
                break;
            case EMainMenu.EMM_Options:
                if (!bOptionsMenu)
                {
                    OptionsMenu.SetActive(true);
                    bOptionsMenu = true;
                    ATrinityGameManager.GetInput().OnMovePressed += NavigateOptions;
                }
                break;
            case EMainMenu.EMM_Quit:
                Application.Quit();
                break;
            
            
        }
    }

    public void NavigateForwards()
    {
        if (bOptionsMenu || bRotating)
        {
            return;
        }
        
        EMainMenu[] values = (EMainMenu[])Enum.GetValues(typeof(EMainMenu));
        int index = Array.IndexOf(values, MainMenuSelection);
        index = (index + 1) % values.Length;
        MainMenuSelection = values[index];
    }

    public void NavigateBackwards()
    {
        if (bOptionsMenu || bRotating)
        {
            return;
        }
    
        EMainMenu[] values = (EMainMenu[])Enum.GetValues(typeof(EMainMenu));
        int index = Array.IndexOf(values, MainMenuSelection);
        index = (index - 1 + values.Length) % values.Length;
        MainMenuSelection = values[index]; 
    }

    
    private void NavigateByElement(ETrinityElement newElement)
    {
        if (bOptionsMenu || bRotating)
        {
            return;
        }
        
        switch (newElement)
        {
            case ETrinityElement.ETE_Fire:
                MainMenuSelection = EMainMenu.EMM_Start;
                break;
            case ETrinityElement.ETE_Cold:
                MainMenuSelection = EMainMenu.EMM_Quit;
                break;
            case ETrinityElement.ETE_Lightning:
                MainMenuSelection = EMainMenu.EMM_Options;
                break;
        }
    }

    private void NavigateOptions()
    {
        if (!bOptionsMenu)
        {
            return;
        }
    }

    public void CloseOptions()
    {
        OptionsMenu.SetActive(false);
        bOptionsMenu = false;
        ATrinityGameManager.GetInput().OnMovePressed -= NavigateOptions;

    }

    public void OnDestroy()
    {
        ATrinityGameManager.GetInput().OnJumpGlidePressed -= Select;
        ATrinityGameManager.GetInput().OnElementPressed -= NavigateByElement;
        ATrinityGameManager.GetInput().OnNextElementPressed -= NavigateForwards;
        ATrinityGameManager.GetInput().OnPreviousElementPressed -= NavigateBackwards;
    }
}
