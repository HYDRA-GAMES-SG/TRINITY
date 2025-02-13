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

    public AFlyingAudio FlyingAudio;


    void Start()
    {
        FlyingAudio = GetComponent<AFlyingAudio>();
    }
    void Update()
    {
        CheckCoolDown();

        if (EnemyStatus.Health.Current <= 0 && !(FlyingBossFSM.CurrentState is FBDie))
        {
            FlyingBossFSM.EnqueueTransition<FBDie>();
        }
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD && !(FlyingBossFSM.CurrentState is FBHover))
        {
            FlyingBossFSM.EnqueueTransition<FBHover>();
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
        return ElectricBombDMG;
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
    //public void StartBlinking(float damageAmount)
    //{
    //    blinkTimer = blinkDuration;
    //    InvokeRepeating(nameof(HandleBlink), 0f, Time.deltaTime);
    //}

    //private void StopBlinking()
    //{
    //    CancelInvoke(nameof(HandleBlink)); // Stop the blinking effect

    //    if (material != null)
    //    {
    //        material.SetColor("_EmissionColor", Color.black);
    //    }
    //}

    //private void HandleBlink()
    //{
    //    if (blinkTimer <= 0f)
    //    {
    //        StopBlinking();
    //        return;
    //    }

    //    blinkTimer -= Time.deltaTime;

    //    float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
    //    float intensity = lerp * blinkIntensity;

    //    if (material != null)
    //    {
    //        material.EnableKeyword("_EMISSION");
    //        Color emissionColor = Color.white * intensity;
    //        material.SetColor("_EmissionColor", emissionColor);
    //    }
    //}
}
