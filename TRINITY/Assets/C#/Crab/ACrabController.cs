using UnityEngine;
using UnityEngine.AI;

public class ACrabController : IEnemyController
{
    public ACrabFSM CrabFSM;

    [Header("Cooldown Time")]
    [SerializeField] float JumpSmashCooldown = 20;
    [SerializeField] float ChargeMoveFastCooldown = 15;
    [SerializeField] float RoarIceSprayCooldown = 10f;
    [SerializeField] float ComboCooldown = 5f;
    [SerializeField] float GetHitCooldown = 1f;
    [SerializeField] private float NavSpeed = 7f;

    private float JumpSmashTimer = 0f;
    private float ChargeMoveFastTimer = 0f;
    private float RoarIceSprayTimer = 0f;
    private float ComboTimer = 0f;
    private float GetHitTimer = 0f;

    [Header("Attack Deal")]
    [SerializeField] float ComboAttack;
    [SerializeField] float JumpSmashAttack;
    [SerializeField] float ChargeFastAttack;

    [Header("Particle Attack Deal")]
    [SerializeField] float IceSprayAttack;
    [SerializeField] float IceSlashAttack;
    [SerializeField] float SmashFrozenGroundAttack;
    [SerializeField] float JumpSmashFrozenGroundAttack;

    [HideInInspector] public bool CanJumpSmash = false;
    [HideInInspector] public bool CanRoarStun = false;
    [HideInInspector] public bool CanComboAttack = false;
    [HideInInspector] public bool CanCharageMoveFast = false;
    [HideInInspector] public bool CanGetHit = false;


    [HideInInspector] public bool bElementPhase = false;

    [Header("The max distance that root motion animation near to target")]
    [SerializeField] float RootMotionNotEnterDistance;

    private void Start()
    {
        AI.speed = NavSpeed;
    }

    void Update()
    {
        CheckCooldown();

        HandleChill();

        if (EnemyStatus.Health.Current < EnemyStatus.Health.MAX / 2 && !bElementPhase) //next element phrese
        {
            CrabFSM.EnqueueTransition<IcePhaseRoar>();
            ComboAttack = 0;
        }
        if (EnemyStatus.Health.Current <= 0f)
        {
            CrabFSM.EnqueueTransition<Death>();
        }
    }

    private void HandleChill()
    {
        AI.speed = NavSpeed * EnemyStatus.Ailments.ChillSpeedModifier;
        CrabFSM.Animator.speed = EnemyStatus.Ailments.ChillSpeedModifier;
    }

    public override float GetParticleAttack()
    {
        if (CrabFSM.CurrentState is JumpSmash)
        {
            return JumpSmashFrozenGroundAttack;
        }
        else if (CrabFSM.CurrentState is ComboAttack comboAttack)
        {
            if (comboAttack.AnimKey == "2HitComboClawsAttack_RM")
            {
                return IceSlashAttack;
            }
            else if (comboAttack.AnimKey == "2HitComboSmashAttack_RM")
            {
                return SmashFrozenGroundAttack;
            }
            return 0;
        }
        else if (CrabFSM.CurrentState is RoarIceSpray)
        {
            return IceSprayAttack;
        }
        else
        {

            return 0;
        }
    }

    private void CheckCooldown()
    {
        if (!CanJumpSmash)
        {
            JumpSmashTimer += Time.deltaTime;
            if (JumpSmashTimer >= JumpSmashCooldown)
            {
                CanJumpSmash = true;
                JumpSmashTimer = 0f;
            }
        }
        if (!CanRoarStun)
        {
            RoarIceSprayTimer += Time.deltaTime;
            if (RoarIceSprayTimer >= RoarIceSprayCooldown)
            {
                CanRoarStun = true;
                RoarIceSprayTimer = 0f;
            }
        }
        if (!CanComboAttack)
        {
            ComboTimer += Time.deltaTime;
            if (ComboTimer >= ComboCooldown)
            {
                CanComboAttack = true;
                ComboTimer = 0f;
            }
        }
        if (!CanCharageMoveFast)
        {
            ChargeMoveFastTimer += Time.deltaTime;
            if (ChargeMoveFastTimer >= ChargeMoveFastCooldown)
            {
                CanCharageMoveFast = true;
                ChargeMoveFastTimer = 0f;
            }
        }
        if (!CanGetHit)
        {
            GetHitTimer += Time.deltaTime;
            if (GetHitTimer >= GetHitCooldown)
            {
                CanGetHit = true;
                GetHitTimer = 0f;
            }
        }
    }

    public override float GetCurrentAttackDamage()
    {
        if (CrabFSM.CurrentState is JumpSmash)
        {
            return JumpSmashAttack;
        }
        else if (CrabFSM.CurrentState is ComboAttack)
        {
            return ComboAttack;
        }
        else if (CrabFSM.CurrentState is ChargeFastAttack)
        {
            return ChargeFastAttack;
        }
        else
        {
            return NormalAttack;
        }
    }

    void OnAnimatorMove()
    {
        if (CrabFSM.CurrentState is NormalAttack || CrabFSM.CurrentState is ComboAttack)
        {
            float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, transform.position);
            if (distanceToTarget > AI.stoppingDistance)
            {
                transform.position += Animator.deltaPosition;
            }
            else
            {
                transform.position -= Animator.deltaPosition * 0.3f;
            }
            transform.rotation *= Animator.deltaRotation;
        }
        else if (CrabFSM.CurrentState is JumpSmash || CrabFSM.CurrentState is ChargeFastAttack)
        {
            transform.position += Animator.deltaPosition;
        }
    }

    public override void TriggerGetHit()
    {
        CrabFSM.EnqueueTransition<GetHit>();
    }

    public void RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
    public float CalculateGroundDistance()
    {
        Vector3 PlayerPos = new Vector3(CrabFSM.PlayerController.transform.position.x, 0, CrabFSM.PlayerController.transform.position.z);
        Vector3 IBPos = new Vector3(transform.position.x, 0, transform.position.z);

        float distanceToTarget = Vector3.Distance(PlayerPos, IBPos);
        return distanceToTarget;
    }
}
