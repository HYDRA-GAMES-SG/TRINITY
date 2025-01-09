using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AEnemyHealthBar : MonoBehaviour
{
    public IEnemyController EnemyController;
    public TextMeshProUGUI EnemyName;
    public Slider HealthBar;    
    public Slider DamageBar;

    public float HealthTarget;
    
    // Start is called before the first frame update
    void Start()
    {
        EnemyController.EnemyStatus.Health.OnHealthModified += UpdateEnemyHealthBar;
        EnemyController.EnemyStatus.Health.OnDeath  += OnEnemyDeath;
    }

    // Update is called once per frame
    void Update()
    {
       DamageBar.value = Mathf.Lerp(DamageBar.value, HealthTarget, Time.deltaTime);
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
