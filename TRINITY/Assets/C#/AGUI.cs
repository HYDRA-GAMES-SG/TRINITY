using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AGUI : MonoBehaviour
{
    [SerializeField] private UHealthComponent BossHealth;

    [SerializeField] private Slider HealthSlider, ManaSlider, BossHealthSlider;

    [SerializeField]
    private TextMeshProUGUI HealthText, ManaText;

    private ATrinityCharacter PlayerCharacter;

    void Start()
    {
        if(PlayerCharacter != null)
        {
            PlayerCharacter.HealthComponent.OnHealthModified += UpdateHealthBar;
        }

        if (PlayerCharacter != null)
        {
            PlayerCharacter.ManaComponent.OnManaModified += UpdateManaBar;
        }
    }

    void Update()
    {
        HandleBossHealth();

    }

    private void HandleBossHealth()
    {
        if (AGameManager.Boss != null)
        {
            if (BossHealth == null)
            {
                BossHealth = AGameManager.Boss.GetComponent<UHealthComponent>();
                
                if (!BossHealthSlider.gameObject.activeSelf)
                {
                    BossHealthSlider.gameObject.SetActive(true);
                }
            }
            
            BossHealthSlider.value = BossHealth.Percent;
        }
        else
        {
            if (BossHealth != null)
            {
                BossHealthSlider.gameObject.SetActive(false);
                BossHealth = null;
            }
        }
    }

    public void UpdateHealthBar(float HealthPercent)
    {
        if (HealthSlider != null)
            HealthSlider.value = HealthPercent;

        if (HealthText != null)
            HealthText.text = $"{PlayerCharacter.HealthComponent.Current}/{PlayerCharacter.HealthComponent.MAX}%";
    }
    public void UpdateManaBar(float ManaPercent)
    {
        if (ManaSlider != null)
            ManaSlider.value = ManaPercent;

        if (ManaText != null)
            ManaText.text = $"{PlayerCharacter.ManaComponent.Current}/{PlayerCharacter.ManaComponent.MAX}";
    }

    void OnDestroy()
    {
        if (PlayerCharacter != null)
            PlayerCharacter.HealthComponent.OnHealthModified -= UpdateHealthBar;

        if (PlayerCharacter != null)
            PlayerCharacter.ManaComponent.OnManaModified -= UpdateManaBar;
    }
}