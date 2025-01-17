using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AEnemyHealthBar : MonoBehaviour
{
    public TextMeshProUGUI ChillText;
    public TextMeshProUGUI ChargeText;
    public TextMeshProUGUI IgniteText;
    public GameObject ChillImage;
    public GameObject ChargeImage;
    public GameObject IgniteImage;
    
    public TextMeshProUGUI DamageText;
    public IEnemyController EnemyController;
    public TextMeshProUGUI EnemyName;
    public Slider HealthBar;    
    public Slider DamageBar;

    public float HealthTarget;

    public float DamageTextUpdateFrequency = 1f;

    private float DamageTextUpdateTimer = 1f;
    
    // Start is called before the first frame update
    void Start()
    {
        EnemyController.EnemyStatus.Health.OnHealthModified += UpdateEnemyHealthBar;
        EnemyController.EnemyStatus.Health.OnDamageTaken += UpdateDamageText;
        EnemyController.EnemyStatus.Health.OnDeath  += OnEnemyDeath;
        EnemyController.EnemyStatus.Ailments.OnChargeModified += UpdateChargeStacks;
        EnemyController.EnemyStatus.Ailments.OnChillModified += UpdateChillStacks;
        EnemyController.EnemyStatus.Ailments.OnIgniteModified += UpdateIgniteStacks;
        DamageText.text = "";
    }

    private void UpdateChillStacks(UAilmentComponent ailmentComponent)
    {
        if (ailmentComponent.AilmentKeys[EAilmentType.EAT_Chill].Stacks > 0)
        {
            ChillText.text = ailmentComponent.AilmentKeys[EAilmentType.EAT_Chill].Stacks.ToString();
            ChillImage.SetActive(true);
        }
        else
        {
            ChillText.text = "";
            ChillImage.SetActive(false);
        }
    }
    private void UpdateChargeStacks(UAilmentComponent ailmentComponent)
    {
        if (ailmentComponent.AilmentKeys[EAilmentType.EAT_Charge].Stacks > 0)
        {
            ChargeText.text = ailmentComponent.AilmentKeys[EAilmentType.EAT_Charge].Stacks.ToString();
            ChargeImage.SetActive(true);
        }
        else
        {
            ChargeText.text = "";
            ChargeImage.SetActive(false);
        }
    }
    private void UpdateIgniteStacks(UAilmentComponent ailmentComponent)
    {
        if (ailmentComponent.AilmentKeys[EAilmentType.EAT_Ignite].Stacks > 0)
        {
            IgniteText.text = ailmentComponent.AilmentKeys[EAilmentType.EAT_Ignite].Stacks.ToString();
            IgniteImage.SetActive(true);
        }
        else
        {
            IgniteText.text = "";
            IgniteImage.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
       DamageBar.value = Mathf.Lerp(DamageBar.value, HealthTarget, Time.deltaTime);
       DamageTextUpdateTimer -= Time.deltaTime;
       
       if (DamageTextUpdateTimer <= 0f && DamageText.text != "")
       {
           DamageText.text = "";
       }

    }

    private void UpdateDamageText(float damageTaken)
    {
        // if (DamageText.text != "")
        // {
        //     if (damageTaken * 10 <= int.Parse(DamageText.text))
        //     {
        //         return;
        //     }    
        // }

        if (damageTaken < 5f)
        {
            return;
        }
        
        DamageText.text = Mathf.FloorToInt(damageTaken).ToString();
        DamageTextUpdateTimer = DamageTextUpdateFrequency;    
    }
    
    private void UpdateEnemyHealthBar(float healthPercent)
    {
        HealthTarget = HealthBar.value;
        HealthBar.value = healthPercent;
    }

    private void OnEnemyDeath()
    {
        HealthBar.value = 0f;
        DamageBar.value = 0f;
        gameObject.SetActive(false);
        EnemyController.EnemyStatus.Health.OnHealthModified -= UpdateEnemyHealthBar;
        EnemyController.EnemyStatus.Health.OnDeath  -= OnEnemyDeath;
        EnemyController.EnemyStatus.Health.OnDamageTaken -= UpdateDamageText;
        EnemyController.EnemyStatus.Ailments.OnChargeModified -= UpdateChargeStacks;
        EnemyController.EnemyStatus.Ailments.OnChillModified -= UpdateChillStacks;
        EnemyController.EnemyStatus.Ailments.OnIgniteModified -= UpdateIgniteStacks;
        
        Destroy(this);
    }
}
