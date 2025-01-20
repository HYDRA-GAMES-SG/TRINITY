using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASecondaryCold : ASpell
{
    [Header("Ice Cube Properties")]
    public LayerMask GroundLayer;
    public float Range;
    public int InitialChillStacks;
    public float MaxScale;
    public float MinScale;
    public float MaxChannelTime;
    public float Duration;
    public float IceCreationPercentThreshold;

    
    [Header("Audio")]
    public AudioClip CastingSFX;

    private AudioSource SFXSource;
    private GameObject IceCube;
    private float CurrentScale;
    private float ChannelTime;

    public override void Initialize()
    {
        if (IceCube == null)
        {
            IceCube = Instantiate(SpellPrefab);
            IceCube.transform.SetParent(this.gameObject.transform);
            IceCube.SetActive(false);
        }
        
         SFXSource = GetComponent<AudioSource>();
    }
    
    public override void CastStart()
    {
        if (IceCube == null)
        {
            return;
        }

        
        Vector3 invokePosition = GetGroundPosition();
        
        CurrentScale = MinScale;
        
        if (invokePosition != Vector3.zero)
        {
            IceCube iceCube = IceCube.GetComponent<IceCube>();
            iceCube.Duration = Duration;
            iceCube.Mesh.enabled = false;
            iceCube.Reset();
            IceCube.transform.position = invokePosition;
            IceCube.transform.localScale = Vector3.one * MinScale;
            IceCube.SetActive(true);
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
        if (IceCube == null || !IceCube.activeSelf)
        {
            return;
        }

        // Update the channeling time
        ChannelTime += Time.deltaTime;

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);
        CurrentScale = Mathf.Lerp(MinScale, MaxScale, t);
        IceCube.transform.localScale = CurrentScale * Vector3.one;

        if (ChannelTime / MaxChannelTime >= IceCreationPercentThreshold)
        {
            IceCube.GetComponent<IceCube>().Mesh.enabled = true;
        }
        
        if (ChannelTime >= MaxChannelTime)
        {
            Release(); //trigger end when max channel time is reached
        }
    }
    public override void CastEnd()
    {
        if (CastingSFX != null)
        {
            SFXSource.Stop();
        }

        if (ChannelTime / MaxChannelTime < IceCreationPercentThreshold)
        {
            IceCube.GetComponent<IceCube>().Melt();
        }
        
        if (IceCube == null || !IceCube.activeSelf)
        {
            return; //do nothing if rune does not exist since no valid placement found
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
