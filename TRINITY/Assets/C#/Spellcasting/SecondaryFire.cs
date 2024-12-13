using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SecondaryFire : ASpell
{
    public bool ENABLE_DEBUG = false;

    // Flameblast Properties
    [Header("Properties")]
    public float Range = 10f;
    public float MaxChannelTime = 2f;
    public float MaxSize = 4f;
    public float MinSize = .1f;

    [Header("Resources")]
    private GameObject RunePrefab;
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

    public override void Initialize()
    {
        RunePrefab = SpellPrefab;
        if (Rune == null)
        {
            Rune = Instantiate(RunePrefab, Spells.CastPoint.position, Quaternion.identity);
            Rune.SetActive(false); // Ensure the rune is initially inactive
        }
        SFX = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        if (Rune == null) return;

        // Position the rune at a valid point in front of the caster within the specified range
        Vector3 position = GetTargetPositionInRange();
        
        if (position != Vector3.zero)
        {
            Rune.transform.position = position;
            Rune.transform.localScale = Vector3.one * MinSize; // Start with the minimum size
            Rune.SetActive(true);
            ChannelTime = 0f;

            if (Channeling != null)
            {
                SFX.clip = Channeling;
                SFX.Play();
            }
        }
    }

    public override void CastUpdate()
    {
        if (Rune == null || !Rune.activeSelf) return;

        // Update the channeling time
        ChannelTime += Time.deltaTime;

        // Calculate the new size based on channeling time
        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);
        float newSize = Mathf.Lerp(MinSize, MaxSize, t);
        Rune.transform.localScale = Vector3.one * newSize;

        if (ChannelTime >= MaxChannelTime)
        {
            CastEnd(); // Automatically trigger end when max channel time is reached
        }
    }

    public override void CastEnd()
    {
        if (Rune == null || !Rune.activeSelf) return;

        Rune.SetActive(false); // Disable the rune

        // Spawn the glyph and pillar at the rune's location
        if (GlyphPrefab != null)
        {
            Glyph = Instantiate(GlyphPrefab, Rune.transform.position, Quaternion.identity);
        }

        if (PillarPrefab != null)
        {
            Pillar = Instantiate(PillarPrefab, Rune.transform.position, Quaternion.identity);
        }

        if (Explosion != null)
        {
            SFX.PlayOneShot(Explosion); 
        }
    }

    private Vector3 GetTargetPositionInRange()
    {
        Ray ray = Spells.CameraRef.Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            // Ensure the hit point is within range and on valid ground
            if (Vector3.Distance(Spells.CastPoint.position, hit.point) <= Range)
            {
                return hit.point;
            }
        }
        return Vector3.zero; // Return zero vector if no valid point is found
    }
}