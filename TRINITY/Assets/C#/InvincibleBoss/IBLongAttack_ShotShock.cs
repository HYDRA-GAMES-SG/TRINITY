using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBLongAttack_ShotShock : InvincibleBossState
{
    [SerializeField]
    string AnimKey = "SpitterShot2";

    [SerializeField] Transform ShockBluePos;
    [SerializeField] ParticleSystem ShockBlue;
    [SerializeField] IBShock ShockTrail;
    [SerializeField] float ShockDelay;
    [SerializeField] float ShockSpeed;

    [SerializeField] Transform Indicator;
    [SerializeField] Vector3 targetScale;
    [SerializeField] Vector3 initialScale;
    [SerializeField] float scaleSpeed = 30f; // Z scale change per second
    [SerializeField] bool isScaling = false;

    float timer = 0;
    bool hasShot = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.bCanShotShock && fromState is IBPursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKey);

        ShockBlue.Play();

        initialScale = Indicator.localScale;
        isScaling = false;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (InvincibleBossFSM.PlayerController.transform.position - InvincibleBossFSM.InvincibleBossController.transform.position).normalized;
        InvincibleBossFSM.InvincibleBossController.RotateTowardTarget(faceDirection, RotateSpeed);

        string layerName = GetType().Name;
        int layerIndex = InvincibleBossFSM.InvincibleBossController.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = InvincibleBossFSM.InvincibleBossController.Animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            InvincibleBossFSM.EnqueueTransition<IBPursue>();
        }


        Vector3 playerBasePos = InvincibleBossFSM.PlayerController.transform.position;
        float playerMidHeight = InvincibleBossFSM.PlayerController.GetComponent<CapsuleCollider>().height / 2f;
        Vector3 playerMidPos = new Vector3(playerBasePos.x, playerBasePos.y + playerMidHeight, playerBasePos.z);
        Vector3 ShockBlueDirection = (playerMidPos - ShockBluePos.position).normalized;
        ShockBluePos.transform.rotation = Quaternion.LookRotation(ShockBlueDirection, Vector3.up);

        timer += Time.fixedDeltaTime;
        if (timer >=  2 && !isScaling)
        {
            isScaling = true;
            Indicator.gameObject.SetActive(true);
        }
        if (isScaling)
        {
            Indicator.localScale = Vector3.Lerp(Indicator.localScale, targetScale, Time.deltaTime / (1f / scaleSpeed));

            if (Mathf.Abs(Indicator.localScale.z - targetScale.z) < 0.01f)
            {
                Indicator.localScale = targetScale;
            }
        }
        if (timer >= ShockDelay && !hasShot)
        {
            Indicator.gameObject.SetActive(false);
            hasShot = true;
            IBShock shock = Instantiate(ShockTrail, ShockBluePos.transform.position, ShockBluePos.transform.rotation);
            shock.GetController(InvincibleBossFSM.InvincibleBossController);
            Rigidbody rb = shock.GetComponent<Rigidbody>();
            rb.velocity = shock.transform.forward * ShockSpeed;
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        timer = 0;
        hasShot = false;
        ShockBlue.Stop();
        InvincibleBossFSM.InvincibleBossController.bCanShotShock = false;
        Indicator.localScale = initialScale;
        Indicator.gameObject.SetActive(false);
        isScaling = false;

    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
