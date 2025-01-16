using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AInvincibleBossController : IEnemyController
{
    public AInvincibleBossFSM InvincibleBossFSM;

    float InitialSpeed;

    [Header("Distance Check")]
    public float CloseAttack = 13f;
    public float LongAttack = 30f;

    //----------------------------------------------------------------------------------

    [Header("Cooldown Time")]
    [SerializeField] float ShotShockCd = 10f;
    [SerializeField] float TauntCd = 20f;
    [SerializeField] float ThrowCd = 30f;

    float TimerShotShock = 0;
    float TimerTaunt = 0;
    float TimerThrow = 0;

    public bool bCanShotShock = false;
    public bool bCanTaunt = false;
    public bool bCanThrow = false;

    //----------------------------------------------------------------------------------

    [Header("Attack Damage")]
    [SerializeField] float SwingHandDMG;
    [SerializeField] float SmashDMG;
    [SerializeField] float StompDMG;
    [SerializeField] float ShotShockDMG;
    [SerializeField] float ThrowOrbDMG;
    public float OrbExplosionDMG;

    [HideInInspector]
    public UHealthComponent Health;
    [HideInInspector]
    public bool bIsDead = false;

    void Start()
    {
        Health = GetComponent<UHealthComponent>();
        AI.stoppingDistance = CloseAttack;
        InitialSpeed = AI.speed;
    }

    // Update is called once per frame
    void Update()
    {
        CheckCooldown();
        HandleChill();

        if (Health.Current <= 0 && !bIsDead)
        {
            bIsDead = true;
            InvincibleBossFSM.EnqueueTransition<IBDizzy>();
        }
    }
    private void HandleChill()
    {
        AI.speed = InitialSpeed * EnemyStatus.Ailments.ChillSpeedModifier;
        InvincibleBossFSM.Animator.speed = EnemyStatus.Ailments.ChillSpeedModifier;
    }

    private void OnAnimatorMove()
    {
        if (InvincibleBossFSM.CurrentState is IBHandAttack)
        {
            if (CalculateGroundDistance() > AI.stoppingDistance)
            {
                transform.position += Animator.deltaPosition;
            }
            else
            {
                transform.position -= Animator.deltaPosition * 0.1f;
            }
            transform.rotation *= Animator.deltaRotation;
        }
    }

    private void CheckCooldown()
    {
        if (!bCanShotShock)
        {
            TimerShotShock += Time.deltaTime;
            if (TimerShotShock >= ShotShockCd)
            {
                bCanShotShock = true;
                TimerShotShock = 0f;
            }
        }
        if (!bCanTaunt)
        {
            TimerTaunt += Time.deltaTime;
            if (TimerTaunt >= TauntCd)
            {
                bCanTaunt = true;
                TimerTaunt = 0f;
            }
        }
        if (!bCanThrow)
        {
            TimerThrow += Time.deltaTime;
            if (TimerThrow >= ThrowCd)
            {
                bCanThrow = true;
                TimerThrow = 0f;
            }
        }
    }

    public override float GetCurrentAttackDamage()
    {
        if (InvincibleBossFSM.CurrentState is IBHandAttack handAtk)
        {
            if (handAtk.AnimKey == handAtk.AnimKeyTriggerATK[0])
            {
                return SwingHandDMG;
            }
            else if (handAtk.AnimKey == handAtk.AnimKeyTriggerATK[1])
            {
                return SmashDMG;
            }
        }
        else if (InvincibleBossFSM.CurrentState is IBFootAttack)
        {
            return StompDMG;
        }
        else if (InvincibleBossFSM.CurrentState is IBLongAttack_ShotShock)
        {
            return ShotShockDMG;
        }
        else if (InvincibleBossFSM.CurrentState is IBLongAttack_ThrowRock)
        {
            return ThrowOrbDMG;
        }
        return NormalAttack;
    }

    public void RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
    public float CalculateGroundDistance()
    {
        Vector3 PlayerPos = new Vector3(InvincibleBossFSM.PlayerController.transform.position.x, 0, InvincibleBossFSM.PlayerController.transform.position.z);
        Vector3 IBPos = new Vector3(transform.position.x, 0, transform.position.z);

        float distanceToTarget = Vector3.Distance(PlayerPos, IBPos);
        return distanceToTarget;
    }
}
