using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AGUI : MonoBehaviour
{
    [SerializeField]
    private UHealthComponent HealthComponent;

    [SerializeField]
    private UManaComponent ManaComponent;

    [SerializeField]
    private Slider HealthSlider, ManaSlider;

    [SerializeField]
    private TextMeshProUGUI HealthText, ManaText;

    void Start()
    {
        if (HealthComponent != null)
        {
            HealthComponent.OnHealthModified += UpdateHealthBar;
        }

        if (ManaComponent != null)
        {
            ManaComponent.OnManaModified += UpdateManaBar;
        }
    }

    public void UpdateHealthBar(float HealthPercent)
    {
        if (HealthSlider != null)
            HealthSlider.value = HealthPercent;

        if (HealthText != null)
            HealthText.text = $"{HealthComponent.Current}/{HealthComponent.MAX}%";
    }
    public void UpdateManaBar(float ManaPercent)
    {
        if (ManaSlider != null)
            ManaSlider.value = ManaPercent;

        if (ManaText != null)
            ManaText.text = $"{ManaComponent.Current}/{ManaComponent.MAX}";
    }

    void OnDestroy()
    {
        if (HealthComponent != null)
            HealthComponent.OnHealthModified -= UpdateHealthBar;

        if (ManaComponent != null)
            ManaComponent.OnManaModified -= UpdateManaBar;
    }
}
