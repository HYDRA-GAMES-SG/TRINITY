using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class LightningBolt : AProjectile
{    
    public static APrimaryLightning PrimaryLightning;
    [HideInInspector]
    private Vector3 Direction;
    private Quaternion BaseRotation;
    public float Damage;
    public float Duration;
    public float Speed;
    [HideInInspector]
    public Rigidbody RB;

    public AudioSource LightningSource;

    [Header("VFX Prefabs")]
    public GameObject ExplosionVFX;

    private SphereCollider Collider;

    private Light[] PointLights;

    private System.Random RNG;
    // Start is called before the first frame update
    void Start()
    {
        BaseRotation = Quaternion.Euler(-90, 0, 0);
        RB = GetComponent<Rigidbody>();
        LightningSource = GetComponent<AudioSource>();
        PrimaryLightning = ATrinityGameManager.GetSpells().PrimaryLightning;
        this.transform.SetParent(PrimaryLightning.transform);
        Direction = ATrinityGameManager.GetSpells().CastDirection;
        Collider = GetComponent<SphereCollider>();
        RB.velocity = Direction * Speed;
        PointLights = GetComponentsInChildren<Light>();
        RNG = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(RB.velocity, Vector3.up) * BaseRotation;
        
        if (!Collider.enabled)
        {
            Collider.enabled = true;
        }

        foreach (Light light in PointLights)
        {
            light.intensity = 50f + (float)RNG.NextDouble() * 150;
        }
    }
    public override void Despawn()
    {
        GameObject vfx = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            UEnemyColliderComponent enemyCollider = collision.gameObject.GetComponent<UEnemyColliderComponent>();

            if (!enemyCollider)
            {
                return;
            }

            enemyCollider.EnemyController.TriggerGetHit();


            UEnemyStatusComponent enemyStatus = enemyCollider.EnemyStatus;
            FDamageInstance damageSource = new FDamageInstance(Damage, PrimaryLightning.AilmentType, PrimaryLightning.StacksApplied);
            enemyStatus += damageSource;
            print($"Damage Taken : {Damage}, Ailment type and stacks : {PrimaryLightning.AilmentType} + {PrimaryLightning.StacksApplied}");
        }

        Despawn();
    }

}
