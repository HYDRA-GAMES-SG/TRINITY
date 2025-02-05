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
    public TextMeshProUGUI EnemyName;
    public Image HealthBar;    
    public Image DamageBar;


    public float DamageTextUpdateFrequency = 1f;

    private float DamageTextUpdateTimer = 1f;

    private float HealthTarget;
    private IEnemyController EnemyController;

    public float GetHealthTarget()
    {
        return HealthTarget;
    }

    public IEnemyController GetController()
    {
        return EnemyController;
    }
    
    public void SetEnemyController(IEnemyController enemyController)
    {
        EnemyController = enemyController;
        EnemyController.EnemyStatus.Health.OnHealthModified += UpdateEnemyHealthBar;
        EnemyController.EnemyStatus.Health.OnDamageTaken += UpdateDamageText;
        EnemyController.EnemyStatus.Health.OnDeath  += OnEnemyDeath;
        EnemyController.EnemyStatus.Ailments.OnChargeModified += UpdateChargeStacks;
        EnemyController.EnemyStatus.Ailments.OnChillModified += UpdateChillStacks;
        EnemyController.EnemyStatus.Ailments.OnIgniteModified += UpdateIgniteStacks;
        DamageText.text = "";
        UpdateChillStacks(EnemyController.EnemyStatus.Ailments);
        UpdateIgniteStacks(EnemyController.EnemyStatus.Ailments);
        UpdateChargeStacks(EnemyController.EnemyStatus.Ailments);
        HealthTarget = 1f;
        DamageBar.fillAmount = 1f;
        HealthBar.fillAmount = 1f;
        EnemyName.text = EnemyController.Name;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        
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
       DamageBar.fillAmount = Mathf.Lerp(DamageBar.fillAmount, HealthTarget, Time.deltaTime);
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
        HealthTarget = HealthBar.fillAmount;
        HealthBar.fillAmount = healthPercent;
    }

    private void OnEnemyDeath()
    {
        HealthBar.fillAmount = 0f;
        DamageBar.fillAmount = 0f;
        EnemyController.EnemyStatus.Health.OnHealthModified -= UpdateEnemyHealthBar;
        EnemyController.EnemyStatus.Health.OnDeath  -= OnEnemyDeath;
        EnemyController.EnemyStatus.Health.OnDamageTaken -= UpdateDamageText;
        EnemyController.EnemyStatus.Ailments.OnChargeModified -= UpdateChargeStacks;
        EnemyController.EnemyStatus.Ailments.OnChillModified -= UpdateChillStacks;
        EnemyController.EnemyStatus.Ailments.OnIgniteModified -= UpdateIgniteStacks;

        this.gameObject.SetActive(false);
    }
}
