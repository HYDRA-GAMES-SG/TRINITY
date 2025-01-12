using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFlyingBossController : IEnemyController
{
    public AFlyingBossFSM FlyingBossFSM;
    public Transform InvincibleBoss;

    public ParticleSystem ElectricShot;
    public GameObject GOElectricShot;
    public ParticleSystem ElectricCharge;
    public GameObject GOElectricCharge;

    [Header("Distance Check")]
    public float CloseAttackRange = 13f;
    public float LongAttackRange = 30f;
    public float HoverRange = 50f;
    public float InvincibleBossRange = 2f;

    [Header("Custom Hide Range")]
    public float HoverXAxis = 10f;
    public float HoverYAxis = 10f;
    public float HoverZAxis = 3f;

    [Header("Cooldown Time")]
    [SerializeField] float ElectricChargeAttackCD = 10f;
    [SerializeField] float SpikeAttackCD = 10f;
    [SerializeField] float RandomFlyCD = 2f;


    float TimerSpikeAttack = 0f;
    float TimerElectricChargeAttack = 0f;
    float TimerRandomFly = 0f;

    public bool bCanRandomFly = true;
    public bool bCanSpikeAttack = false;
    public bool bCanElectricChargeAttack = false;

    public bool bGOElectricShotSpawned = false;
    public bool bGOElectricChargeSpawned = false;

    [Header("Attack Damage")]
    [SerializeField] float ElectricBombDMG;

    public UHealthComponent Health;

    void Start()
    {
        Health = GetComponent<UHealthComponent>();
    }
    void Update()
    {
        CheckCoolDown();

        if (Health.Current <= 0)
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
}
