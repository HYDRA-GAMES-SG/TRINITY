using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFlyingBossController : IEnemyController
{
    public AFlyingBossFSM FlyingBossFSM;
    public Rigidbody rb;
    public Transform InvincibleBoss;
    public ParticleSystem ElectricShot;
    public ParticleSystem ElectricCharge;

    [Header("Distance Check")]
    public float CloseAttackRange = 13f;
    public float LongAttackRange = 30f;
    public float HoverRange = 50f;

    [Header("Custom Hide Range")]
    public float HoverXAxis = 10f;
    public float HoverYAxis = 10f;
    public float HoverZAxis = 3f;

    [Header("Cooldown Time")]
    [SerializeField] float ElectricChargeAttack = 10f;
    [SerializeField] float SpikeAttack = 10f;


    float TimerSpikeAttack = 0f;
    float TimerElectricChargeAttack = 0f;

    public bool bCanSpikeAttacked = false;
    public bool bCanElectricChareAttacked = false;

    [Header("Attack Damage")]
    [SerializeField] float SpikeAttackDMG;
    [SerializeField] float ElectricChareDMG;

    public UHealthComponent Health;

    void Start()
    {
        Health = GetComponent<UHealthComponent>();
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        CheckCoolDown();

        if (Health.Current <= 0)
        {
            FlyingBossFSM.EnqueueTransition<FBDie>();
        }
        print(FlyingBossFSM.CurrentState);
    }
    private void CheckCoolDown()
    {
        if (bCanSpikeAttacked)
        {
            TimerSpikeAttack += Time.deltaTime;
            if (TimerSpikeAttack >= SpikeAttack)
            {
                bCanSpikeAttacked = true;
                TimerSpikeAttack = 0f;
            }
        }
        if (bCanElectricChareAttacked)
        {
            TimerElectricChargeAttack += Time.deltaTime;
            if (TimerElectricChargeAttack >= ElectricChargeAttack)
            {
                bCanElectricChareAttacked = true;
                TimerElectricChargeAttack = 0f;
            }
        }
    }
    public float GetCurrentAttackDamage()
    {
        if (FlyingBossFSM.CurrentState is FBAttack)
        {
            return SpikeAttackDMG;
        }
        return 0;
    }
    public void RotateTowardTarget(Vector3 directionToTarget)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
    }
    public float CalculateDistance()
    {
        Vector3 PlayerPos = new Vector3(FlyingBossFSM.PlayerController.transform.position.x, 0, FlyingBossFSM.PlayerController.transform.position.z);
        Vector3 IBPos = new Vector3(transform.position.x, 0, transform.position.z);

        float distanceToTarget = Vector3.Distance(PlayerPos, IBPos);
        return distanceToTarget;
    }
}
