using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UHealthComponent))]
public class ACrabController : MonoBehaviour
{
    public ACrabFSM CrabFSM;
    public NavMeshAgent AI;

    [Header("Cooldown Time")]
    [SerializeField] float JumpSmashCooldown = 20;
    [SerializeField] float ChargeMoveFastCooldown = 15;
    [SerializeField] float RoarIceSprayCooldown = 10f;
    [SerializeField] float ComboCooldown = 5f;

    private float JumpSmashTimer = 0f;
    private float ChargeMoveFastTimer = 0f;
    private float RoarIceSprayTimer = 0f;
    private float ComboTimer = 0f;

    [Header("Attack Deal")]
    [SerializeField] float NormalAttack;
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

    /*[HideInInspector]*/
    public bool bElementPhase = false;

    [Header("The max distance that root motion animation near to target")]
    [SerializeField] float RootMotionNotEnterDistance;

    private UHealthComponent Health;
    private Animator Animator;


    void Start()
    {
        Health = GetComponent<UHealthComponent>();
        Animator = GetComponent<Animator>();
        AGameManager.SetBoss(this.gameObject);
    }

    void Update()
    {
        CheckCooldown();

        if (Health.Percent < 0.5f) //next element phrese
        {
            bElementPhase = true;
            ComboAttack = 0;
        }
        if (Health.Current <= 0f)
        {
            CrabFSM.EnqueueTransition<Death>();
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
    }
    public float GetCurrentAttackDamage()
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

    public float GetParticleAttack()
    {
        if (CrabFSM.CurrentState is JumpSmash)
        {
            Debug.Log("A");
            return JumpSmashFrozenGroundAttack;
        }
        else if (CrabFSM.CurrentState is ComboAttack comboAttack)
        {
            if (comboAttack.AnimKey == "2HitComboClawsAttack_RM")
            {
            Debug.Log("B");
                return IceSlashAttack;
            }
            else if (comboAttack.AnimKey == "2HitComboSmashAttack_RM")
            {
            Debug.Log("C");
                return SmashFrozenGroundAttack;
            }
            return 0;
        }
        else if (CrabFSM.CurrentState is RoarIceSpray)
        {
            Debug.Log("D");
            return IceSprayAttack;
        }
        else
        {
            return 0;
        }
    }

    public bool IsActiveState(CrabState state)
    {
        return state is NormalAttack || state is ComboAttack;
    }


    void OnAnimatorMove()
    {
        if (IsActiveState(CrabFSM.CurrentState))
        {
            float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position);
            if (distanceToTarget > RootMotionNotEnterDistance)
            {
                CrabFSM.CrabController.transform.position += Animator.deltaPosition;
            }
            else
            {
                CrabFSM.CrabController.transform.position -= Animator.deltaPosition * 0.1f;
            }
            CrabFSM.CrabController.transform.rotation *= Animator.deltaRotation;
        }
        else if (CrabFSM.CurrentState is JumpSmash || CrabFSM.CurrentState is ChargeFastAttack)
        {
            CrabFSM.CrabController.transform.position += Animator.deltaPosition;
        }
    }

}
