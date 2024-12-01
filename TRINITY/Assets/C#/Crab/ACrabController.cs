using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UHealthComponent))]
public class ACrabController : MonoBehaviour
{
    public ACrabFSM CrabFSM;
    public NavMeshAgent AI;

    [Header("Cooldown Time")]
    [SerializeField] float JumpSmashCooldown = 10f;
    [SerializeField] float RoarStunCooldown = 10f;
    [SerializeField] float ComboCooldown = 5f;

    private float JumpSmashTimer = 0f;
    private float RoarStunTimer = 0f;
    private float ComboTimer = 0f;

    [HideInInspector] public bool CanJumpSmash = false;
    [HideInInspector] public bool CanRoarStun = false;
    [HideInInspector] public bool CanComboAttack = false;
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
    }
}
