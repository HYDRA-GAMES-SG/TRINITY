using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AInvincibleBossController : IEnemyController
{
    public AInvincibleBossFSM InvincibleBossFSM;
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
    [SerializeField] float ShotShockDMG;
    [SerializeField] float ThrowDMG;


    public UHealthComponent Health;

    void Start()
    {
        Health = GetComponent<UHealthComponent>();
        AI.stoppingDistance = CloseAttack;
    }

    // Update is called once per frame
    void Update()
    {
        CheckCooldown();

        if (Health.Current <= 0)
        {
            InvincibleBossFSM.EnqueueTransition<IBDizzy>();
        }
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
        if (InvincibleBossFSM.CurrentState is IBHandAttack)
        {
            return ShotShockDMG;
        }
        return 0;
    }

    public void RotateTowardTarget(Vector3 directionToTarget , float rotateSpeed)
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
    