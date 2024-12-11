using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AGUI : MonoBehaviour
{

    [SerializeField] private Slider HealthSlider, ManaSlider, BossHealthSlider;

    [SerializeField]
    private TextMeshProUGUI HealthText, ManaText;
    public ATrinityController TrinityController;
    public ATrinityBrain TrinityBrain;
    public ATrinitySpells TrinitySpells;
    
    
    [Header("UI Objects")]
    public Image FrameBackground;
    public GameObject CurrentElementImage;
    public GameObject[] ElementImages = new GameObject[3];
    public string FireHexademicalCode = "#FF0000";
    public string ColdHexademicalCode = "#00CDFF";
    public string LightningHexademicalCode = "#FFC400";

    void Start()
    {
        if (TrinityController != null)
        {
            TrinityController.HealthComponent.OnHealthModified += UpdateHealthBar;
        }

        
        if (TrinitySpells != null)
        {
            TrinitySpells.ManaComponent.OnManaModified += UpdateManaBar;
        }
        
        ATrinityBrain.OnBossSet += SetupBossUI;
        
        CurrentElementImage = ElementImages[(int)TrinityBrain.GetElement()];
        
        if (TrinityBrain != null)
        {
            TrinityBrain.OnElementChanged += UpdateElement;
        }

    }

    void Update()
    {

    }
    
    public void UpdateElement(ETrinityElement newElement)
    {
        CurrentElementImage.SetActive(false); //turn off previous image
        
        CurrentElementImage = ElementImages[(int)newElement];
        CurrentElementImage.SetActive(true);

        
        switch ((int)newElement)
        {

            case 0:
            {
                SetColorByHexademical(FireHexademicalCode);
                break;
            }
            case 1:
            {
                SetColorByHexademical(ColdHexademicalCode);
                break;
            }
            case 2:
            {
                SetColorByHexademical(LightningHexademicalCode);
                break;
            }
        }
    }
    
    public void SetColorByHexademical(string hexCode) 
    {
        Color newColor;
        if (ColorUtility.TryParseHtmlString(hexCode, out newColor))
        {
            FrameBackground.color = newColor;
            Color frameBackgroundColor = FrameBackground.color;
            frameBackgroundColor.a = (40f / 255f);
            FrameBackground.color = frameBackgroundColor;
        }
    }
    
    

    private void SetupBossUI(GameObject bossGameObject)
    {
        bossGameObject.GetComponent<UEnemyStatus>().Health.OnHealthModified += UpdateBossHealthBar;
        bossGameObject.GetComponent<UEnemyStatus>().Health.OnDeath += HandleBossDeath;
        BossHealthSlider.gameObject.SetActive(true);
        BossHealthSlider.value = 100f;
    }

    private void HandleBossDeath()
    {
        BossHealthSlider.value = 0f;
        BossHealthSlider.gameObject.SetActive(false);

    }
    private void UpdateBossHealthBar(float healthPercent)
    {
        BossHealthSlider.value = healthPercent;
    }


    public void UpdateHealthBar(float HealthPercent)
    {
        if (HealthSlider != null)
        {
            HealthSlider.value = HealthPercent;
        }
        if (HealthText != null) 
        {
            HealthText.text = $"{TrinityController.HealthComponent.Current}/{TrinityController.HealthComponent.MAX}HP";
        }
    }
    public void UpdateManaBar(float ManaPercent)
    {
        if (ManaSlider != null) 
        {   
            ManaSlider.value = ManaPercent;
        }

        if (ManaText != null) 
        {
            ManaText.text = $"{TrinitySpells.ManaComponent.Current}/{TrinitySpells.ManaComponent.MAX}";
        }      
    }

    void OnDestroy()
    {
        if (TrinityController != null)
            TrinityController.HealthComponent.OnHealthModified -= UpdateHealthBar;

        if (TrinitySpells != null)
            TrinitySpells.ManaComponent.OnManaModified -= UpdateManaBar;

        ATrinityBrain.OnBossSet -= SetupBossUI;
    }
}