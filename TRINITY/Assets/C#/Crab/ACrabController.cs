using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UHealthComponent))]
public class ACrabController : MonoBehaviour
{
    public ACrabFSM CrabFSM;
    public NavMeshAgent AI;

    [SerializeField] float JumpSmashCooldown = 10f;
    [SerializeField] float RoarStunCooldown = 10f;
    [SerializeField] float ComboCooldown = 5f;

    private float JumpSmashTimer = 0f;
    private float RoarStunTimer = 0f;
    private float ComboTimer = 0f;

    public bool CanJumpSmash = false;
    public bool CanRoarStun = false;
    public bool CanComboAttack = false;
    //[HideInInspector]

    private UHealthComponent Health;

    void Start()
    {
        Health = GetComponent<UHealthComponent>();

        AGameManager.SetBoss(this.gameObject);
    }

    void Update()
    {
        CheckCooldown();
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
}
