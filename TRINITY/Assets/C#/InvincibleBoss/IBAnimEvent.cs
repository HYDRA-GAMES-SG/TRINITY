using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class IBAnimEvent : MonoBehaviour
{
    [SerializeField] ParticleSystem FootStompEffect;
    [SerializeField] ParticleSystem TauntEffect;

    public Transform LeftFoot;
    public Transform RightFoot;
    public Transform Bottom;
    public Transform MouthPos;

    AInvincibleBossController Controller;
    private void Start()
    {
        Controller = GetComponent<AInvincibleBossController>();
    }
    public void SpawnLeftFootStompVFX()
    {
        ParticleSystem stomp = Instantiate(FootStompEffect, transform.position, transform.rotation);
        AAttackCollider projectileController = stomp.GetComponentInChildren<AAttackCollider>();
        projectileController.SetController(Controller);
    }

    public void SpawnRightFootStompVFX()
    {
        ParticleSystem stomp = Instantiate(FootStompEffect, transform.position, transform.rotation);
        AAttackCollider projectileController = stomp.GetComponentInChildren<AAttackCollider>();
        projectileController.SetController(Controller);
    }

    public void SpawnTauntVFX()
    {
        Instantiate(TauntEffect, Bottom.position, Bottom.rotation);
    }
}
