using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFlyingBossController : IEnemyController
{
    public AFlyingBossFSM FlyingBossFSM;
    public Transform InvincibleBoss;


    [Header("Distance Check")]
   
    public float HoverRange = 50f;

    [Header("Custom Hide Range")]
    public float HoverXAxis = 10f;
    public float HoverYAxis = 10f;
    public float HoverZAxis = 3f;

    [Header("Cooldown Time")]
    [SerializeField] float ElectricChargeAttackCD = 10f;
    [SerializeField] float RandomFlyCD = 2f;


    float TimerElectricChargeAttack = 0f;
    float TimerRandomFly = 0f;

    public bool bCanRandomFly = true;
    public bool bCanElectricChargeAttack = false;

    [Header("Attack Damage")]
    [SerializeField] float ElectricBombDMG;

    [Header("GetHitEffect")]
    [SerializeField] float blinkTimer;
    [SerializeField] float blinkDuration = 1.0f;
    [SerializeField] float blinkIntensity = 2.0f;
    SkinnedMeshRenderer[] skinnedMeshRenderer;
    Material[] materials;
    void Start()
    {
        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        materials = new Material[skinnedMeshRenderer.Length];

        // Cache all materials
        for (int i = 0; i < skinnedMeshRenderer.Length; i++)
        {
            materials[i] = skinnedMeshRenderer[i].material;
        }
        EnemyStatus.Health.OnDamageTaken += StartBlinking;
    }
    void Update()
    {
        CheckCoolDown();

        if (EnemyStatus.Health.Current <= 0)
        {
            FlyingBossFSM.EnqueueTransition<FBDie>();
        }
    }
    private void CheckCoolDown()
    {
        if (!bCanRandomFly)
        {
            TimerRandomFly += Time.deltaTime;
            if (TimerRandomFly >= RandomFlyCD)
            {
                bCanRandomFly = true;
                TimerRandomFly = 0f;
            }
        }
        //if (!bCanSpikeAttack)
        //{
        //    TimerSpikeAttack += Time.deltaTime;
        //    if (TimerSpikeAttack >= SpikeAttackCD)
        //    {
        //        bCanSpikeAttack = true;
        //        TimerSpikeAttack = 0f;
        //    }
        //}
        if (!bCanElectricChargeAttack)
        {
            TimerElectricChargeAttack += Time.deltaTime;
            if (TimerElectricChargeAttack >= ElectricChargeAttackCD)
            {
                bCanElectricChargeAttack = true;
                TimerElectricChargeAttack = 0f;
            }
        }
    }
    public override float GetCurrentAttackDamage()
    {
        if (FlyingBossFSM.CurrentState is FBAttack)
        {
            return ElectricBombDMG;
        }
        return NormalAttack;
    }
    public void RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
    public float CalculateDistance()
    {
        Vector3 PlayerPos = new Vector3(FlyingBossFSM.PlayerController.transform.position.x, 0, FlyingBossFSM.PlayerController.transform.position.z);
        Vector3 FlyBossPos = new Vector3(transform.position.x, 0, transform.position.z);

        float distanceToTarget = Vector3.Distance(PlayerPos, FlyBossPos);
        return distanceToTarget;
    }
    public void StartBlinking(float damageAmount)
    {
        blinkTimer = blinkDuration;
        InvokeRepeating(nameof(HandleBlink), 0f, Time.deltaTime);
    }

    private void StopBlinking()
    {
        CancelInvoke(nameof(HandleBlink));

        foreach (var material in materials)
        {
            if (material != null)
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    private void HandleBlink()
    {
        if (blinkTimer <= 0f)
        {
            StopBlinking();
            return;
        }

        blinkTimer -= Time.deltaTime;

        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = lerp * blinkIntensity;

        foreach (var material in materials)
        {
            if (material != null)
            {
                Color emissionColor = Color.white * intensity;
                material.SetColor("_EmissionColor", emissionColor);

                // Enable emission in the material
                DynamicGI.SetEmissive(skinnedMeshRenderer[0], emissionColor);
            }
        }
    }
}
