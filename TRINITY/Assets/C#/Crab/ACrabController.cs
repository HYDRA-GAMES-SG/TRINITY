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
    [SerializeField] float RoarStunCooldown = 10f;
    [SerializeField] float ComboCooldown = 5f;

    private float JumpSmashTimer = 0f;
    private float RoarStunTimer = 0f;
    private float ComboTimer = 0f;
    private float ChargeMoveFastTimer = 0f;

    [Header("Attack Deal")]
    [SerializeField] float NormalAttack;
    [SerializeField] float ComboAttack;
    [SerializeField] float JumpSmashAttack;
    [SerializeField] float ChargeFastAttack;

    [HideInInspector] public bool CanJumpSmash = false;
    [HideInInspector] public bool CanRoarStun = false;
    [HideInInspector] public bool CanComboAttack = false;
    [HideInInspector] public bool CanCharageMoveFast = false;

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
        //Debug.Log(Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position));
        CheckCooldown();

        if (Health.Percent < 0.5f) //next element phrese
        {

        }
        else if (Health.Percent <= 0f)
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
            RoarStunTimer += Time.deltaTime;
            if (RoarStunTimer >= RoarStunCooldown)
            {
                CanRoarStun = true;
                RoarStunTimer = 0f;
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

    public bool IsActiveState(CrabState state)
    {
        return state is NormalAttack || state is ComboAttack;
    }

    public void OnAnimatorMove()
    {
        if (IsActiveState(CrabFSM.CurrentState))
        {
            float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position);
            if (distanceToTarget > RootMotionNotEnterDistance)
            {
                CrabFSM.CrabController.transform.position += Animator.deltaPosition;
                CrabFSM.CrabController.transform.rotation *= Animator.deltaRotation;
            }
            else
            {
                CrabFSM.CrabController.transform.rotation *= Animator.deltaRotation;
            }
        }
        else if (CrabFSM.CurrentState is JumpSmash || CrabFSM.CurrentState is ChargeFastAttack)
        {
            CrabFSM.CrabController.transform.position += Animator.deltaPosition;
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
            return NormalAttack; // Default attack
        }
    }

    public void ApplyDamage(float damageNumber)
    {
        Health.Modify(-damageNumber);
    }
}
