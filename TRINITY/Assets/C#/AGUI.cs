using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AGUI : MonoBehaviour
{
    [SerializeField] private Slider HealthSlider, ManaSlider, BossHealthSlider, DamageSlider, BossDamageSlider;

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

    private float BossHealthTarget;
    private float PlayerHealthTarget;

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
        // Lerp DamageSlider and BossDamageSlider to target values
        if (DamageSlider != null)
        {
            DamageSlider.value = Mathf.Lerp(DamageSlider.value, PlayerHealthTarget, Time.deltaTime);
        }

        if (BossDamageSlider != null && BossHealthSlider.gameObject.activeSelf)
        {
            BossDamageSlider.value = Mathf.Lerp(BossDamageSlider.value, BossHealthTarget, Time.deltaTime);
        }
    }

    public void UpdateElement(ETrinityElement newElement)
    {
        CurrentElementImage.SetActive(false); // Turn off previous image

        CurrentElementImage = ElementImages[(int)newElement];
        CurrentElementImage.SetActive(true);

        switch ((int)newElement)
        {
            case 0:
                SetColorByHexademical(FireHexademicalCode);
                break;
            case 1:
                SetColorByHexademical(ColdHexademicalCode);
                break;
            case 2:
                SetColorByHexademical(LightningHexademicalCode);
                break;
        }
    }

    public void SetColorByHexademical(string hexCode)
    {
        if (ColorUtility.TryParseHtmlString(hexCode, out Color newColor))
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
        BossDamageSlider.value = 100f; // Initialize BossDamageSlider
    }

    private void HandleBossDeath()
    {
        BossHealthSlider.value = 0f;
        BossDamageSlider.value = 0f;
        BossHealthSlider.gameObject.SetActive(false);
        BossDamageSlider.gameObject.SetActive(false);
    }

    private void UpdateBossHealthBar(float healthPercent)
    {
        BossHealthTarget = BossHealthSlider.value; // Record the current value
        BossHealthSlider.value = healthPercent;
    }

    public void UpdateHealthBar(float healthPercent)
    {
        if (HealthSlider != null)
        {
            PlayerHealthTarget = HealthSlider.value; // Record the current value
            HealthSlider.value = healthPercent;
        }
        if (HealthText != null)
        {
            HealthText.text = $"{TrinityController.HealthComponent.Current}/{TrinityController.HealthComponent.MAX}HP";
        }
    }

    public void UpdateManaBar(float manaPercent)
    {
        if (ManaSlider != null)
        {
            ManaSlider.value = manaPercent;
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