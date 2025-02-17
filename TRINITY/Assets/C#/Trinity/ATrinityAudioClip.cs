using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityAudioClip : MonoBehaviour
{
    public AudioClip Audio;
    public string Name;
    public float Volume = 1f;
    public float Pitch = 1f;
    public EAudioGroup MixerGroup = EAudioGroup.EAG_SFX;
    public float Audio3DMaxDistance = 100;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
