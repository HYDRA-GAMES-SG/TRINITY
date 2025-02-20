using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ATrinityMainMenu : MonoBehaviour
{
    public event Action OnMainMenuExit;
    public event Action OnMainMenuNavigate;
    public event Action OnMainMenuSelection;
    public event Action<ETrinityElement> OnMenuElementChanged;

    public AMainMenuGate Gate;
    public Image TitleText;
    public float TitleFadeInTime;
    public AMainMenuCamera MainMenuCamera;
    public float RotationSpeed = 240f;
    public RectTransform ElementTriangle;
    public GameObject OptionsMenu;
    public GameObject MainMenuGUI;
    public List<TextMeshProUGUI> TriangleTexts;
    public GameObject ArrowParent;

    private bool bStartingGame = false;
    private bool bRotating = false;
    private bool bOptionsMenu = false;
    private EMainMenu MainMenuSelection;
    private Color InitialColor;
    
    public bool bCanSkipMainMenu = false;
    
    public bool IsOptionsMenuOpen() => OptionsMenu.activeSelf;

    // Start is called before the first frame update
    void Start()
    {
        TriangleTexts = new List<TextMeshProUGUI>();
        TriangleTexts = ElementTriangle.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        InitialColor = TriangleTexts[0].color;

        Color newColor = TitleText.color;
        newColor.a = 0f;
        TitleText.color = newColor;
    }

    void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        MainMenuSelection = EMainMenu.EMM_Start;
        bOptionsMenu = false;
        bRotating = false;
        bStartingGame = false;
    }
    

    public void Update()
    {
        if (TitleText.color.a < 1f)
        {
            Color newColor = TitleText.color;
            newColor.a += Time.deltaTime / TitleFadeInTime;
            newColor.a = Mathf.Clamp01(newColor.a);
            TitleText.color = newColor;
        }
    
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
            OnMenuElementChanged?.Invoke((ETrinityElement)MainMenuSelection);
        }
    }

    public void Select()
    {
        if (bOptionsMenu || bRotating || bStartingGame)
        {
            //handle options menu
            return;
        }

        OnMainMenuSelection?.Invoke();

        switch (MainMenuSelection)
        {
            case EMainMenu.EMM_Start:
                if (!bStartingGame)
                {
                    bStartingGame = true;
                    MainMenuCamera.Animate();
                    OnMainMenuExit?.Invoke();
                }

                break;
            case EMainMenu.EMM_Options:
                if (!bOptionsMenu)
                {
                    HideTriangleText();
                    OptionsMenu.SetActive(true);
                    bOptionsMenu = true;
                    ATrinityGameManager.GetInput().OnForcefieldPressed += CloseOptions;
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

        OnMainMenuNavigate?.Invoke();

        EMainMenu[] values = (EMainMenu[])Enum.GetValues(typeof(EMainMenu));
        int index = Array.IndexOf(values, MainMenuSelection);
        index = (index + 1) % values.Length;
        MainMenuSelection = values[index];
        HideArrows();
    }

    private void HideArrows()
    {
        ArrowParent.SetActive(false);
    }

    public void NavigateBackwards()
    {
        if (bOptionsMenu || bRotating)
        {
            return;
        }

        OnMainMenuNavigate?.Invoke();

        EMainMenu[] values = (EMainMenu[])Enum.GetValues(typeof(EMainMenu));
        int index = Array.IndexOf(values, MainMenuSelection);
        index = (index - 1 + values.Length) % values.Length;
        MainMenuSelection = values[index];
        HideArrows();
        
    }


    public void NavigateByElement(ETrinityElement newElement)
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

        OnMainMenuNavigate?.Invoke();
        
        HideArrows();
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
        ShowTriangleText();
        OptionsMenu.SetActive(false);
        bOptionsMenu = false;
        ATrinityGameManager.GetInput().OnForcefieldPressed -= CloseOptions;
    }

    public void HideTriangleText()
    {
        foreach (TextMeshProUGUI txt in TriangleTexts)
        {
            txt.alpha = 0f;
        }
    }

    public void ShowTriangleText()
    {
        
        foreach (TextMeshProUGUI txt in TriangleTexts)
        {
            txt.alpha = 1f;
        }
    }

    
}
