using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASecondaryLightning : ASpell
{
    [Header("Summoning Properties")]
    public LayerMask GroundLayer;
    public float MaxChannelTime = 2f;
    public float TotemSummonDepth = 5f;
    public float Range = 10f;

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


    
    [Header("Audio")]
    public AudioClip CastingSFX;
    private AudioSource SFXSource;

    private List<GameObject> Totems;
    
    private GameObject Totem;
    private float ChannelTime;
    private float CurrentPosition;
    private Vector3 InvokePosition;
    
    public override void Initialize()
    {
        Totems = new List<GameObject>();
        
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
            LightningTotem lightningTotem = Totem.GetComponent<LightningTotem>();
            lightningTotem.Duration = TotemDuration;
            lightningTotem.AttackFrequency = AttackFrequency;
            lightningTotem.InvokePosition = InvokePosition;
            lightningTotem.SummonDepth = TotemSummonDepth;
            lightningTotem.MaxPitchSpawn = TotemMaxPitchSpawn;
            
            Totem.transform.position = InvokePosition + Vector3.down * TotemSummonDepth;
            
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
            Totem.GetComponent<LightningTotem>().bSummoned = true;
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
            Totem.GetComponent<LightningTotem>().Unsummon();
        }
        
    }
    
    private Vector3 GetGroundPosition()
    {
        Ray ray = ATrinityGameManager.GetCamera().Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
        if (Physics.Raycast(ray, out RaycastHit hit, Range, GroundLayer))
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
            
            if (Physics.Raycast(searchOrigin, Vector3.down, out RaycastHit groundHit, Range * 2f, GroundLayer))
            {
                return groundHit.point + Vector3.up * .1f;
            }
        }
        
        return Vector3.zero;
    }
}
