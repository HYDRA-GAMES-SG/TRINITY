using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ASecondaryLightning : ASpell
{
    [Header("Summoning Properties")]
    public LayerMask GroundLayer;
    public float MaxChannelTime = 2f;
    public float TotemSummonDepth = 5f;
    public float Range = 10f;
    public GameObject SummonVFX;
    [HideInInspector]
    public GameObject SummonVFXObj;

    [Header("Projectile Properties")]
    public float ProjectileDuration = 4.5f;
    public float ProjectileRange = 10f;
    public float ProjectileSpeed;
    public float AggroRange;
    public float ProjectileDamage;
    public GameObject ProjectilePrefab;
    public int ProjectileChargeStacks = 1;

    [Header("Totem Properties")]
    public float AttackFrequency;
    public float TotemDuration;
    public float TotemMaxPitchSpawn = 15f;
    public float MaxSummoningDistance = 5;

    [Header("Enraged Properties")]
    public float EnragedAttackFrequency = 1.2f;
    public float EnragedAttackDamage = 150f;



    [Header("Audio")]
    public AudioClip CastingSFX;
    private AudioSource SFXSource;

    private List<GameObject> Totems;

    private GameObject Totem;
    private float ChannelTime;
    private float CurrentPosition;
    private Vector3 InvokePosition;

    public List<GameObject> GetTotems()
    {
        return Totems;
    }

    public override void Initialize()
    {
        Totems = new List<GameObject>();

        ManaToComplete = ManaCost + (ManaUpkeepCost * MaxChannelTime);
        SFXSource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        Totem = Instantiate(SpellPrefab);
        Totems.Add(Totem);
        Totem.transform.SetParent(this.gameObject.transform);

        InvokePosition = GetGroundPosition();

        if (InvokePosition != Vector3.zero)
        {
            foreach (GameObject totem in Totems)
            {
                float distanceFromTotem = (InvokePosition - totem.transform.position).magnitude;
                if (distanceFromTotem < MaxSummoningDistance)
                {
                    Release();
                    return;
                }
            }
            LightningTotem lightningTotem = Totem.GetComponent<LightningTotem>();
            lightningTotem.Duration = TotemDuration;
            lightningTotem.AttackFrequency = AttackFrequency;
            lightningTotem.EnragedAttackFrequency = EnragedAttackFrequency;
            lightningTotem.InvokePosition = InvokePosition;
            lightningTotem.SummonDepth = TotemSummonDepth;
            lightningTotem.MaxPitchSpawn = TotemMaxPitchSpawn;

            Totem.transform.position = InvokePosition + Vector3.down * TotemSummonDepth;
            Vector3 vfxPos = new(Totem.transform.position.x, Totem.transform.position.y + 4.9f, Totem.transform.position.z);
            SummonVFXObj = Instantiate(SummonVFX, vfxPos, Quaternion.identity);

            ChannelTime = 0f;

            if (CastingSFX != null)
            {
                SFXSource.clip = CastingSFX;
                SFXSource.Play();
            }
        }
        else
        {
            Release();
        }
    }
    public override void CastUpdate()
    {
        // Update the channeling time
        ChannelTime += Time.deltaTime;

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);

        CurrentPosition = Mathf.Lerp(InvokePosition.y - TotemSummonDepth, InvokePosition.y, t);
        Vector3 newPos = new Vector3(InvokePosition.x, CurrentPosition, InvokePosition.z);
        Totem.transform.localPosition = newPos;

        if (ChannelTime >= MaxChannelTime)
        {
            Totem.GetComponent<LightningTotem>().Summoned();

            Release(); //trigger end when max channel time is reached
        }
    }

    public override void CastEnd()
    {
        if (CastingSFX != null)
        {
            SFXSource.Stop();
        }

        if (ChannelTime < MaxChannelTime)
        {
            if (Totem != null)
            {
                if (Totem.GetComponent<LightningTotem>() != null)
                {
                    Totem.GetComponent<LightningTotem>().Unsummon();
                }
            }
        }

        Destroy(SummonVFXObj);
    }

    private Vector3 GetGroundPosition()
    {
        Ray ray = ATrinityGameManager.GetCamera().Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, Range, GroundLayer, QueryTriggerInteraction.Ignore))
        {
            // make sure the hit point is within range and on valid ground
            if (Vector3.Distance(ATrinityGameManager.GetSpells().CastPoint.position, hit.point) <= Range)
            {
                return hit.point + Vector3.up * .1f;
            }
        }
        else
        {
            //if we don't get a valid ground hit we just find ground at the max range in the forward vector
            Vector3 searchOrigin = ATrinityGameManager.GetSpells().CastPoint.position + ATrinityGameManager.GetPlayerController().Forward * Range;

            if (Physics.Raycast(searchOrigin, Vector3.down, out RaycastHit groundHit, Range * 4f, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return groundHit.point + Vector3.up * .1f;
            }
        }

        return Vector3.zero;
    }
}
