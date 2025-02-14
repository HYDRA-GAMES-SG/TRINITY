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
    public AudioClip[] CastingSFX;

    private AudioSource SFXSource;
    [HideInInspector]
    public GameObject IceCubeInstance;
    private float CurrentScale;
    private float ChannelTime;

    [HideInInspector]
    public BoxCollider IceCubeTrigger;

    public override void Initialize()
    {
        if (IceCubeInstance == null)
        {
            IceCubeInstance = Instantiate(SpellPrefab);
            IceCubeInstance.transform.SetParent(this.gameObject.transform);
            IceCubeInstance.SetActive(false);
            IceCubeTrigger = IceCubeInstance.transform.Find("SpellTrigger").GetComponent<BoxCollider>();
        }

        ManaToComplete = ManaCost + (ManaUpkeepCost * (MaxChannelTime * IceCreationPercentThreshold));
        SFXSource = GetComponent<AudioSource>();
    }

    public override void CastStart()
    {
        if (IceCubeInstance == null)
        {
            return;
        }


        Vector3 invokePosition = GetGroundPosition();

        CurrentScale = MinScale;

        if (invokePosition != Vector3.zero)
        {
            IceCube iceCube = IceCubeInstance.GetComponent<IceCube>();
            iceCube.Duration = Duration;
            iceCube.Mesh.enabled = false;
            iceCube.Reset();
            IceCubeInstance.transform.position = invokePosition;
            IceCubeInstance.transform.localScale = Vector3.one * MinScale;
            IceCubeInstance.transform.Find("GlowWide").gameObject.SetActive(true);
            IceCubeInstance.SetActive(true);
            ChannelTime = 0f;

            if (CastingSFX != null)
            {
                int rng = Random.Range(0, CastingSFX.Length);
                SFXSource.clip = CastingSFX[rng];
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
        if (IceCubeInstance == null || !IceCubeInstance.activeSelf)
        {
            return;
        }

        // Update the channeling time
        ChannelTime += Time.deltaTime;

        float t = Mathf.Clamp01(ChannelTime / MaxChannelTime);
        CurrentScale = Mathf.Lerp(MinScale, MaxScale, t);
        IceCubeInstance.transform.localScale = CurrentScale * Vector3.one;

        if (ChannelTime / MaxChannelTime >= IceCreationPercentThreshold)
        {
            IceCubeInstance.GetComponent<IceCube>().Mesh.enabled = true;
            IceCubeInstance.transform.Find("GlowWide").gameObject.SetActive(false);
        }

        if (ChannelTime >= MaxChannelTime)
        {
            Release(); //trigger end when max channel time is reached
        }
    }
    public override void CastEnd()
    {

        if (ChannelTime / MaxChannelTime < IceCreationPercentThreshold && CastingSFX != null)
        {
            IceCubeInstance.GetComponent<IceCube>().Melt();
            SFXSource.Stop();
        }

        if (IceCubeInstance == null || !IceCubeInstance.activeSelf)
        {
            return; //do nothing if rune does not exist since no valid placement found
        }
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

            if (Physics.Raycast(searchOrigin, Vector3.down, out RaycastHit groundHit, Range * 2f, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return groundHit.point + Vector3.up * .1f;
            }
        }

        return Vector3.zero;
    }
}
