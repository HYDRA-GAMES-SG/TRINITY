using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBAnimEvent : MonoBehaviour
{
    [SerializeField] ParticleSystem FootStompEffect;
    [SerializeField] ParticleSystem TauntEffect;

    public Transform LeftFoot;
    public Transform RightFoot;
    public Transform Bottom;
    public Transform MouthPos;

    public void SpawnLeftFootStompVFX()
    {
        Instantiate(FootStompEffect, transform.position, transform.rotation);
    }

    public void SpawnRightFootStompVFX()
    {
        Instantiate(FootStompEffect, transform.position, transform.rotation);
    }

    public void SpawnTauntVFX()
    {
        Instantiate(TauntEffect, Bottom.position, Bottom.rotation);
    }
}
