using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AEnemyHealthBar : MonoBehaviour
{
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
        DamageText.text = "";
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
        
        Destroy(this);
    }
}
