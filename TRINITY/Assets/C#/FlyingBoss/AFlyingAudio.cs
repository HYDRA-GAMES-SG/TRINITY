using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFlyingAudio : IAudioManager
{
    private void Awake()
    {
        base.Awake();
    }
    public void PlayCharge()
    {
        Play("AttackCharge");
    }
    public void PlayRelease()
    {
        Play("AttackRelease");
    }
}
