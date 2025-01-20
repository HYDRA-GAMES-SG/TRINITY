using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class ElectricBoom : MonoBehaviour
{
    [SerializeField] ParticleSystem Thunder;
    [SerializeField] ParticleSystem ThunderExplosion;
    [SerializeField] ParticleSystem Indicator;

    [SerializeField] float CallThunder;
    [SerializeField] float CallThunderExplosion;

    public float Damage;
    public IEnemyController Controller;

    float ThunderTimer;
    float ThunderExplosionTimer;

    bool bThunder = false;
    bool bThunderExplosion = false;

    private void Start()
    {
        Destroy(Thunder.gameObject, 1.5f);
        Destroy(gameObject, 3f);
    }
    void Update()
    {
        ThunderTimer += Time.deltaTime;
        if (ThunderTimer >= CallThunder && !bThunder)
        {
            bThunder = true;
            Thunder.gameObject.SetActive(true);
        }
        if (bThunder)
        {
            ThunderExplosionTimer += Time.deltaTime;
            if (ThunderExplosionTimer >= CallThunderExplosion && !bThunderExplosion)
            {
                bThunderExplosion = true;
                ThunderExplosion.gameObject.SetActive(true);
            }
        }
    }

    public void GetControllerDamage(IEnemyController controller, float damage)
    {
        Controller = controller;
        Damage = damage;
    }

}
