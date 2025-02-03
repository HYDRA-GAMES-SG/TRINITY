using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACrabAudio : IAudioManager
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlaySmashAudio()
    {
        Play("Smash");
    }

    //void PlayAudio()
    //{
    //    IState currentState = Controller.CrabFSM.CurrentState;

    //    if(currentState is Pursue)
    //    {
    //         //do whatever
    //    }
    //}
}
