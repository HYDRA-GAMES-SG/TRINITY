using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class ATrinityGUI : MonoBehaviour
{
    public AEnemyHealthBar[] EnemyHealthBars = new AEnemyHealthBar[3];
    public GameObject OptionsMenu;
    public GameObject GameOver;
    
    [SerializeField] private Image HealthSlider, ManaSlider, DamageSlider;
    
    [Header("UI Objects")]
    public GameObject CurrentElementImage;
    public GameObject[] CurrentSpellImages = new GameObject[3];

    public Image[] CooldownFills = new Image[5];
    public Sprite[] ElementImages = new Sprite[3];
    
    public Sprite[] FireSpellImages = new Sprite[3];
    public Sprite[] ColdSpellImages = new Sprite[3];
    public Sprite[] LightningSpellImages = new Sprite[3];
    
    private float PlayerHealthTarget;

    public float PlayerDamageDecayRate = 3f;

    void Start()
    {
        if (ATrinityGameManager.GetPlayerController() != null)
        {
            ATrinityGameManager.GetPlayerController().HealthComponent.OnHealthModified += UpdateHealthBar;
            ATrinityGameManager.GetPlayerController().HealthComponent.OnDeath += DisplayGameOver;
        }

        if (ATrinityGameManager.GetSpells() != null)
        {
            ATrinityGameManager.GetSpells().ManaComponent.OnManaModified += UpdateManaBar;
        }
        
        CurrentElementImage.GetComponent<Image>().sprite = ElementImages[(int)ATrinityGameManager.GetBrain().GetElement()];

        if (ATrinityGameManager.GetBrain() != null)
        {
            ATrinityGameManager.GetBrain().OnElementChanged += UpdateElementUI;
        }

        ATrinityGameManager.GetInput().OnMenuPressed += TogglePause;
        
        SetupEnemyUI();
    }

    private void DisplayGameOver()
    {
        GameOver.GetComponent<ATrinityGameOver>().Display();
    }

    void Update()
    {
        // Lerp DamageSlider to target values
        if (DamageSlider != null)
        {
            DamageSlider.fillAmount = Mathf.Lerp(DamageSlider.fillAmount, PlayerHealthTarget, PlayerDamageDecayRate * Time.deltaTime);
        }

        UpdateCooldowns();
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

    public void UpdateElementUI(ETrinityElement newElement)
    {
        CurrentElementImage.GetComponent<Image>().sprite = ElementImages[(int)newElement];

        for (int i = 0; i < CurrentSpellImages.Length; i++)
        {
            switch (newElement)
            {
                case ETrinityElement.ETE_Cold:
                    CurrentSpellImages[i].GetComponent<Image>().sprite = ColdSpellImages[i];
                    break;
                case ETrinityElement.ETE_Lightning:
                    CurrentSpellImages[i].GetComponent<Image>().sprite = LightningSpellImages[i];
                    break;
                case ETrinityElement.ETE_Fire:
                    CurrentSpellImages[i].GetComponent<Image>().sprite = FireSpellImages[i];
                    break;
                default:
                    break;
                
            }
        }
    }

    private void TogglePause()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.DEAD || ATrinityGameManager.GetGameFlowState() != EGameFlowState.MAIN_MENU)
        {
            OptionsMenu.SetActive(!OptionsMenu.activeSelf);
        }
    }
    
    private void SetupEnemyUI()
    {
        for (int i = 0; i < ATrinityGameManager.GetEnemyControllers().Count; i++)
        {
            EnemyHealthBars[i].SetEnemyController(ATrinityGameManager.GetEnemyControllers()[i]);
            EnemyHealthBars[i].EnemyName.text = ATrinityGameManager.GetEnemyControllers()[i].Name;
            EnemyHealthBars[i].gameObject.SetActive(true);
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

    void OnDestroy()
    {
        if (ATrinityGameManager.GetPlayerController() != null)
            ATrinityGameManager.GetPlayerController().HealthComponent.OnHealthModified -= UpdateHealthBar;

        if (ATrinityGameManager.GetSpells() != null)
            ATrinityGameManager.GetSpells().ManaComponent.OnManaModified -= UpdateManaBar;
    }
}