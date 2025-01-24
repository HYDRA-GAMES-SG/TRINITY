using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AInvincibleBossController : IEnemyController
{
    public AInvincibleBossFSM InvincibleBossFSM;

    float InitialSpeed;

    [Header("Vincible Health")] 
    public float VINCIBLE_MAX_HEALTH = 15000f;
    public GameObject Shield;

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

    [Header("GetHitEffect")]
    [SerializeField] float blinkTimer;
    [SerializeField] float blinkDuration = 1.0f;
    [SerializeField] float blinkIntensity = 2.0f;
    [SerializeField]SkinnedMeshRenderer[] skinnedMeshRenderer;
    Material[] materials;


    void Start()
    {
        AI.stoppingDistance = CloseAttack;
        InitialSpeed = AI.speed;

        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        materials = new Material[skinnedMeshRenderer.Length];

        // Cache all materials
        for (int i = 0; i < skinnedMeshRenderer.Length; i++)
        {
            materials[i] = skinnedMeshRenderer[i].material;
        }

        EnemyStatus.Health.OnDamageTaken += StartBlinking;
        
        foreach(IEnemyController ec in ATrinityGameManager.GetEnemyControllers())
        {
            if (ec.Name == "Sentinel")
            {
                ec.EnemyStatus.Health.OnDeath += LowerShield;
            }
        }
    }

    private void LowerShield()
    {
        Shield.SetActive(false);
        EnemyStatus.Health.MAX = VINCIBLE_MAX_HEALTH;
        EnemyStatus.Health.Current = VINCIBLE_MAX_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        CheckCooldown();
        HandleChill();

        if (EnemyStatus.Health.Current <= 0 && !(InvincibleBossFSM.CurrentState is IBDead))
        {
            InvincibleBossFSM.EnqueueTransition<IBDead>();
        }
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD && !(InvincibleBossFSM.CurrentState is IBIdle))
        {
            InvincibleBossFSM.EnqueueTransition<IBIdle>();
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

    public void StartBlinking(float damageAmount)
    {
        blinkTimer = blinkDuration;
        InvokeRepeating(nameof(HandleBlink), 0f, Time.deltaTime);
    }

    private void StopBlinking()
    {
        CancelInvoke(nameof(HandleBlink));

        foreach (var material in materials)
        {
            if (material != null)
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    private void HandleBlink()
    {
        if (blinkTimer <= 0f)
        {
            StopBlinking();
            return;
        }

        blinkTimer -= Time.deltaTime;

        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = lerp * blinkIntensity;

        foreach (var material in materials)
        {
            if (material != null)
            {
                material.EnableKeyword("_EMISSION");
                Color emissionColor = Color.white * intensity;
                material.SetColor("_EmissionColor", emissionColor);
            }
        }
    }
}
