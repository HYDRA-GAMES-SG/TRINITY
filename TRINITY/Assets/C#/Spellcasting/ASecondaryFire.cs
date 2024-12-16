using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASecondaryFire : ASpell
{
    public bool ENABLE_DEBUG = false;

    // Flameblast Properties
    [Header("Properties")] 
    public LayerMask GroundLayer;
    public float Range = 10f;
    public float MaxChannelTime = 2f;
    public float MaxSize = 4f;
    public float MinSize = .1f;
    public float MaxHeight = .5f;

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
        if (ENABLE_DEBUG) { Debug.Log("Initialized"); }

        RunePrefab = SpellPrefab;
        if (Rune == null)
        {
            Rune = Instantiate(RunePrefab, SpellsReference.CastPoint.position, Quaternion.identity);
            Rune.transform.SetParent(this.gameObject.transform);
            Rune.SetActive(false); // Ensure the rune is initially inactive
        }
        SFX = GetComponent<AudioSource>();
        
    }

    public override void CastStart()
    {
        if (ENABLE_DEBUG) { Debug.Log("Cast Start"); }

        if (Rune == null) return;

        Vector3 invokePosition = GetGroundPosition();
        
        if (invokePosition != Vector3.zero)
        {
            Rune.transform.position = invokePosition;
            Rune.transform.localScale = Vector3.one * MinSize; 
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
        if (ENABLE_DEBUG) { Debug.Log("Cast Update"); }

        if (Rune == null || !Rune.activeSelf) return;

        // Update the channeling time
        ChannelTime += Time.deltaTime;

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);
        float newSize = Mathf.Lerp(MinSize, MaxSize, t);
        Rune.transform.localScale = new Vector3(newSize, newSize > MaxHeight ? MaxHeight : newSize, newSize);
        
        if (ChannelTime >= MaxChannelTime)
        {
            Release(); //trigger end when max channel time is reached
        }
    }

    public override void CastEnd()
    {
        if (ENABLE_DEBUG) { Debug.Log("Cast End"); }

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

        // Spawn the glyph and pillar at the rune's location
        if (GlyphPrefab != null)
        {
            Glyph = Instantiate(GlyphPrefab, Rune.transform.position, Quaternion.identity);
            Glyph.transform.SetParent(this.gameObject.transform);
            Vector3 newScale = new Vector3(Rune.transform.localScale.x * .5f, 1f, Rune.transform.localScale.x *.5f);
            Glyph.transform.localScale = newScale;
            Destroy(Glyph, 6f);
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

    private Vector3 GetGroundPosition()
    {
        Ray ray = SpellsReference.CameraReference.Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
        if (Physics.Raycast(ray, out RaycastHit hit, Range, GroundLayer))
        {
            // Ensure the hit point is within range and on valid ground
            if (Vector3.Distance(SpellsReference.CastPoint.position, hit.point) <= Range)
            {
                return hit.point + Vector3.up * .1f;
            }
        }
        else
        {
            //if we don't get a valid ground hit we just find ground at the max range in the forward vector
            Vector3 searchOrigin = SpellsReference.CastPoint.position + Controller.Forward * Range;
            
            if (Physics.Raycast(searchOrigin, Vector3.down, out RaycastHit groundHit, Range * 2f, GroundLayer))
            {
                return groundHit.point + Vector3.up * .1f;
            }
        }
        
        return Vector3.zero;
    }
}