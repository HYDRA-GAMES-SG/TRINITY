using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASecondaryFire : ASpell
{
    [Header("Debug")]
    public bool ENABLE_DEBUG = false;

    [Header("Damage and Ailments")] 
    public EAilmentType AilmentType;
    public int StacksPerRadius;
    public float DamagePerStack = 50f;

    [Header("Zone Properties")] 
    public float ZoneDuration = 10f;
    
    // Flameblast Properties
    [Header("Properties")] 
    public LayerMask GroundLayer;
    public float Range = 10f;
    public float MaxChannelTime = 2f;
    public float MaxRadius = 4f;
    public float MinRadius = .1f;

    [Header("Resources")]
    private GameObject RunePrefab;
    public float MaxRuneParticleHeight = .5f;
    public GameObject GlyphPrefab;
    public GameObject PillarPrefab;
    public AudioClip Channeling;
    public AudioClip Explosion;
    
    
    // Private Variables
    private GameObject Rune;
    private GameObject Glyph;
    private GameObject Pillar;
    private float ChannelTime = 0f;

    // Cached Components
    private AudioSource SFX;
    
    [HideInInspector]
    public float CurrentRadius = 1f;

    public override void Initialize()
    {
        if (ENABLE_DEBUG) { Debug.Log("Initialized"); }

        RunePrefab = SpellPrefab;
        if (Rune == null)
        {
            Rune = Instantiate(RunePrefab, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            Rune.transform.SetParent(this.gameObject.transform);
            Rune.SetActive(false); // Ensure the rune is initially inactive
        }
        SFX = GetComponent<AudioSource>();
        
    }

    public override void CastStart()
    {
        if (Glyph != null)
        {
            Destroy(Glyph);
        }
        
        if (Rune == null) return;

        Vector3 invokePosition = GetGroundPosition();
        CurrentRadius = MinRadius;
        
        if (invokePosition != Vector3.zero)
        {
            Rune.transform.position = invokePosition;
            Rune.transform.localScale = Vector3.one * MinRadius; 
            Rune.SetActive(true);
            ChannelTime = 0f;

            if (Channeling != null)
            {
                SFX.clip = Channeling;
                SFX.Play();
            }
        }
        else
        {
            Release();
        }
    }

    public override void CastUpdate()
    {
        if (Rune == null || !Rune.activeSelf)
        {
            return;
        }

        // Update the channeling time
        ChannelTime += Time.deltaTime;

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);
        CurrentRadius = Mathf.Lerp(MinRadius, MaxRadius, t);
        Rune.transform.localScale = new Vector3(CurrentRadius, CurrentRadius > MaxRuneParticleHeight ? MaxRuneParticleHeight : CurrentRadius, CurrentRadius);
        
        Light runeLight = Rune.transform.Find("Light").gameObject.GetComponent<Light>();
        runeLight.range = Mathf.Lerp(3f, 15f, t);
        runeLight.intensity = Mathf.Lerp(20f, 400f, t);
        
        if (ChannelTime >= MaxChannelTime)
        {
            Release(); //trigger end when max channel time is reached
        }
    }

    public override void CastEnd()
    {

        if (Channeling != null)
        {
            SFX.Stop();
        }

        if (!Rune.activeSelf)
        {
            return; //do nothing if rune does not exist since no valid placement found
        }

        if (Rune == null || !Rune.activeSelf) return;

        Rune.SetActive(false); // Disable the rune


        Ray ray = new Ray(Rune.transform.position, Vector3.up);
        Physics.SphereCast(ray, CurrentRadius, out RaycastHit hitInfo, 5f);

        HitBox hitBox;
        if (hitInfo.collider != null)
        {
            //print("Spherecast collider not null)");
            hitInfo.collider.TryGetComponent<HitBox>(out hitBox);

            if (hitBox != null)
            {
                //print("Hitbox exists");

                hitBox.EnemyStatus.Ailments.ModifyStack(AilmentType, Mathf.RoundToInt(StacksPerRadius * CurrentRadius));
                hitBox.EnemyStatus.Health.Modify(-DamagePerStack * CurrentRadius);
                
                if (ENABLE_DEBUG)
                {
                    Debug.Log($"Applying : {Mathf.RoundToInt(StacksPerRadius * CurrentRadius)} of {AilmentType}");
                }
            }
        }
        
        // spawn the glyph and pillar at the rune's location
        if (GlyphPrefab != null)
        {
            Vector3 glyphPos = Rune.transform.position + Vector3.up * .15f;
            Glyph = Instantiate(GlyphPrefab, glyphPos, Quaternion.identity);
            Glyph.transform.SetParent(this.gameObject.transform);
            Vector3 newScale = new Vector3(Rune.transform.localScale.x * .5f, 1f, Rune.transform.localScale.x *.5f);
            Glyph.transform.localScale = newScale;
            //Destroy(Glyph, 6f);
        }

        if (PillarPrefab != null)
        {
            Pillar = Instantiate(PillarPrefab, Rune.transform.position, Quaternion.identity);
            Pillar.transform.SetParent(this.gameObject.transform);
            Pillar.transform.localScale = Rune.transform.localScale.x / 4f  * Vector3.one;
            Pillar.GetComponent<ParticleSystem>().Play();
            Destroy(Pillar, 7f);
        }

        if (Explosion != null)
        {
            SFX.PlayOneShot(Explosion); 
        }
    }

    private void OnDrawGizmos()
    {
        // if (!Rune.activeSelf)
        // {
        //     Gizmos.DrawSphere(Rune.transform.position, CurrentRadius);
        // }
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