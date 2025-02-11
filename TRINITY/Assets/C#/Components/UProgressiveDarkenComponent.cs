using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using RenderSettings = UnityEngine.RenderSettings;

public class UProgressiveDarkenComponent : MonoBehaviour
{
    private float TotalHealth;
    private float HealthRemaining = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (IEnemyController iec in ATrinityGameManager.GetEnemyControllers())
        {
            TotalHealth += iec.EnemyStatus.Health.MAX;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HealthRemaining = 0f;
        foreach (IEnemyController iec in ATrinityGameManager.GetEnemyControllers())
        {
            HealthRemaining += iec.EnemyStatus.Health.Current;
        }

        RenderSettings.ambientIntensity = Mathf.Lerp(.1f, 3f, HealthRemaining / TotalHealth);
    }
}
