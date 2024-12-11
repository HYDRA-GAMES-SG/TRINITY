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
    public ATrinitySpells TrinitySpells;

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
    }

    void Update()
    {

    }
    private void SetupBossUI(GameObject bossGameObject)
    {
        print(bossGameObject.name);
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
        print($"Health value : {healthPercent}");
        BossHealthSlider.value = healthPercent;
    }


    public void UpdateHealthBar(float HealthPercent)
    {
        if (HealthSlider != null)
        {
            HealthSlider.value = HealthPercent;
            print("Health value changed");
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
            print("Mana value changed");
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