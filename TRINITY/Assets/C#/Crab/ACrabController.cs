using UnityEngine;
using UnityEngine.AI;

public class ACrabController : IEnemyController
{
    public ACrabFSM CrabFSM;

    [Header("Transforms")]
    [SerializeField]
    public Transform LeftClaw;
    [SerializeField]
    public Transform RightClaw;

    [Header("Cooldown Time")]
    [SerializeField] float JumpSmashCooldown = 20;
    [SerializeField] float ChargeMoveFastCooldown = 15;
    [SerializeField] float RoarIceSprayCooldown = 10f;
    [SerializeField] float ComboCooldown = 5f;
    [SerializeField] float GetHitCooldown = 1f;
    [SerializeField] private float NavSpeed = 7f;
    [SerializeField] float dotThreshold = 0.95f;

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

    public bool CanJumpSmash = false;
    public bool CanRoarStun = false;
    public bool CanComboAttack = false;
    public bool CanCharageMoveFast = false;
    public bool CanGetHit = true;

    /*[HideInInspector] */
    public bool bElementPhase = false;
    [HideInInspector] public bool bCanChill = true;

    [Header("The max distance that root motion animation near to target")]
    [SerializeField] float RootMotionNotEnterDistance;

    //[Header("GetHitEffect")]
    //[SerializeField] float blinkTimer;
    //[SerializeField] float blinkDuration = 1.0f;
    //[SerializeField] float blinkIntensity = 2.0f;
    //SkinnedMeshRenderer skinnedMeshRenderer;
    //Material material;
    private void Start()
    {
        AI.speed = NavSpeed;

        //skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        //material = skinnedMeshRenderer.material;

        //EnemyStatus.Health.OnDamageTaken += StartBlinking;
    }
    void Update()
    {
        //Debug.Log(Animator.speed);
        CheckCooldown();

        HandleChill();

        if (EnemyStatus.Health.Current < EnemyStatus.Health.MAX / 2 && !bElementPhase) //next element phrese
        {
            CrabFSM.EnqueueTransition<IcePhaseRoar>();
            ComboAttack = 0;
        }
        if (EnemyStatus.Health.Current <= 0f && !(CrabFSM.CurrentState is Death))
        {
            CrabFSM.EnqueueTransition<Death>();
        }
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD && !(CrabFSM.CurrentState is Idle))
        {
            CrabFSM.EnqueueTransition<Idle>();
        }
    }

    private void HandleChill()
    {
        if (bCanChill)
        {
            AI.speed = NavSpeed * EnemyStatus.Ailments.ChillSpeedModifier;
            CrabFSM.Animator.speed = EnemyStatus.Ailments.ChillSpeedModifier;
        }
        else
        {
            //if (!(CrabFSM.CurrentState is ChargeFastAttack))
            {
                CrabFSM.Animator.speed = 1;
            }
        }
    }

    public override float GetParticleAttack()
    {
        if (CrabFSM.CurrentState is JumpSmash)
        {
            return JumpSmashFrozenGroundAttack;
        }
        else if (CrabFSM.CurrentState is ComboAttack comboAttack)
        {
            float attackDMG;
            switch (comboAttack.AnimKey)
            {
                case "2HitComboClawsAttack_RM_End":
                    attackDMG = IceSlashAttack;
                    break;

                case "2HitComboSmashAttack_RM_End":
                    attackDMG = SmashFrozenGroundAttack;
                    break;
                default:
                    Debug.LogError("No damage");
                    attackDMG = 0;
                    break;
            }
            return attackDMG;
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
        float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, transform.position);
        if (CrabFSM.CurrentState is NormalAttack || CrabFSM.CurrentState is ComboAttack)
        {
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
        else if (CrabFSM.CurrentState is JumpSmash)
        {
            if (distanceToTarget > AI.stoppingDistance)
            {
                transform.position += Animator.deltaPosition;
            }
            else
            {
                transform.position -= Animator.deltaPosition * 0.3f;
            }
        }
        else if (CrabFSM.CurrentState is ChargeFastAttack)
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

    public bool FacingTarget()
    {
        Vector3 directionToTarget = (CrabFSM.PlayerController.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, directionToTarget);
        return dot > dotThreshold;
    }
}
