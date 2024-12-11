using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMinionTest : MonoBehaviour
{
    public float JumpTimer;
    public float AttackTimer;

    public float JumpCooldown;
    public float AttackCooldown;

    public bool CanAttack;
    public bool CanJump;

    public Animator MinionController;

    const string JUMP = "Jump";
    const string GETHITFRONT = "GetHitFront";
    const string CLAWSATTACKLEFT = "ClawsAttackLeft";
    string _currentstate;
    // Start is called before the first frame update
    void Start()
    {
        MinionController = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AttackTimer -= Time.deltaTime;
        JumpTimer -= Time.deltaTime;
        if (JumpTimer < 0)
        {
            ChangeAnimationState(JUMP);
            JumpTimer = JumpCooldown;
        }
        else 
        {
            CanJump = false;
            if (AttackTimer < 0)
            {
                ChangeAnimationState(CLAWSATTACKLEFT);
                AttackTimer = AttackCooldown;
            }
            else
            {
                CanAttack = false;
            }
        }

        if (!CanJump && !CanAttack) 
        {
            ChangeAnimationState(GETHITFRONT);
        }
    }
    private void ChangeAnimationState(string newState)
    {
        if (newState == _currentstate)
        {
            return;
        }
        MinionController.Play(newState);

       _currentstate = newState;
    }
    bool IsAnimationPlaying(Animator animator, string stateName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            return true;
        }

        else
        {
            return false;
        }
    }
}
