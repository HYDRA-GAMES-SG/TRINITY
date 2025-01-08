using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AFinalMonsterController : IEnemyController
{
    public AFinalMonsterBossFSM FinalMonsterFSM;

    [Header("Particle Effect Prefabs")]
    public GameObject BlackHolePrefab;
    public GameObject SnowFallPrefab;
    public GameObject PilarFrostPrefab;
    public GameObject FrostAuraPrefab;
    public GameObject FrostRayPrefab;
    public GameObject WaveFrostPrefab;
    public GameObject WaveFrostCastPrefab;
    public GameObject FrozenPulsePrefab;
    public GameObject OrbFrostPrefab;

    private Dictionary
        <string, GameObject>
        instantiatedParticles = new Dictionary
        <string, GameObject>();

    [Header("Distance Check")]
    public float CloseAttackRange = 13f;
    public float LongAttackRange = 30f;

    [Header("Particle Effect")]
    public ParticleSystem BlackHole;
    public ParticleSystem SnowFall;
    public ParticleSystem PilarFrost;
    public ParticleSystem FrostAura;
    public ParticleSystem FrostRay;
    public ParticleSystem WaveFrost;
    public ParticleSystem WaveFrostCast;
    public ParticleSystem FrozenPulse;
    public ParticleSystem OrbFrost;

    [Header("Long Range Attack Cooldown Time")]
    public float InvokeSnowFallCD = 5f;
    public float InvokeSpikeCD = 3f;
    public float FrostRayCD = 5f;
    public float FrostWaveCD = 2f;

    [Header("Long Range Attack Boolean")]
    public bool bInvokeSnowFall;
    public bool bInvokeSpike;
    public bool bFrostRay;
    public bool bFrostWave;

    [Header("Close Range Attack Cooldown Time")]
    public float HeavyAttackCD = 3f;
    public float DashCD = 3f;
    public float HitTheGroundCD = 5f;

    [Header("Close Range Attack Boolean")]
    public bool bHeavyAttack;
    public bool bDash;
    public bool bHitTheGround;

    public UHealthComponent Health;
    public Rigidbody rb;
    public bool bPhase2;

    private float TimerInvokeSnowFall = 0f;
    private float TimerInvokeSpike = 0f;
    private float TimerFrostRay = 0f;
    private float TimerFrostWave = 0f;
    private float TimerHeavyAttack = 0f;
    private float TimerDash = 0f;
    private float TimerHitTheGround = 0f;


    void Start()
    {
        Health = GetComponent<UHealthComponent>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckCoolDown();
        if (Health.Current <= (Health.MAX / 2))
        {
            bPhase2 = true;
        }
        if (Health.Current <= 0)
        {
            FinalMonsterFSM.EnqueueTransition<FMBDie>();
        }
    }
    void CheckCoolDown()
    {
        if (!bInvokeSnowFall)
        {
            TimerInvokeSnowFall += Time.deltaTime;
            if (TimerInvokeSnowFall >= InvokeSnowFallCD)
            {
                bInvokeSnowFall = true;
                TimerInvokeSnowFall = 0f;
            }
        }

        if (!bInvokeSpike)
        {
            TimerInvokeSpike += Time.deltaTime;
            if (TimerInvokeSpike >= InvokeSpikeCD)
            {
                bInvokeSpike = true;
                TimerInvokeSpike = 0f;
            }
        }

        if (!bFrostRay)
        {
            TimerFrostRay += Time.deltaTime;
            if (TimerFrostRay >= FrostRayCD)
            {
                bFrostRay = true;
                TimerFrostRay = 0f;
            }
        }

        if (!bFrostWave)
        {
            TimerFrostWave += Time.deltaTime;
            if (TimerFrostWave >= FrostWaveCD)
            {
                bFrostWave = true;
                TimerFrostWave = 0f;
            }
        }

        if (!bHeavyAttack)
        {
            TimerHeavyAttack += Time.deltaTime;
            if (TimerHeavyAttack >= HeavyAttackCD)
            {
                bHeavyAttack = true;
                TimerHeavyAttack = 0f;
            }
        }

        if (!bDash)
        {
            TimerDash += Time.deltaTime;
            if (TimerDash >= DashCD)
            {
                bDash = true;
                TimerDash = 0f;
            }
        }

        if (!bHitTheGround)
        {
            TimerHitTheGround += Time.deltaTime;
            if (TimerHitTheGround >= HitTheGroundCD)
            {
                bHitTheGround = true;
                TimerHitTheGround = 0f;
            }
        }
    }
    public void PlayParticleSystem(string particleKey, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!instantiatedParticles.ContainsKey(particleKey))
        {
            instantiatedParticles[particleKey] = Instantiate(prefab, position, rotation);
        }
        else
        {
            instantiatedParticles[particleKey].transform.position = position;
            instantiatedParticles[particleKey].transform.rotation = rotation;
        }

        instantiatedParticles[particleKey].GetComponent<ParticleSystem>().Play();
    }
    public void RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
    public float CalculateGroundDistance()
    {
        Vector3 PlayerPos = new Vector3(FinalMonsterFSM.PlayerController.transform.position.x, 0, FinalMonsterFSM.PlayerController.transform.position.z);
        Vector3 IBPos = new Vector3(transform.position.x, 0, transform.position.z);

        float distanceToTarget = Vector3.Distance(PlayerPos, IBPos);
        return distanceToTarget;
    }
}
