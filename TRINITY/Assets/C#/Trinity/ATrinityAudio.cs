using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityAudio : IAudioManager
{
    // Start is called before the first frame update
    void Start()
    {        
        ATrinityGameManager.SetSFX(this);

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
