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
    public void LightCameraShake(float duration = 0.3f)
    {
        if (!ATrinityGameManager.GetPlayerController().CheckGround().transform)
        {
            return;  //dont send small camera shakes if the player is not on the ground
        }
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCameraFrom(0.05f, duration, transform);
    }

    public void MediumCameraShake(float duration = 0.5f)
    {
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCameraFrom(0.6f, duration, transform);
    }

    public void HeavyCameraShake(float duration = 1.3f) //global
    {
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCamera(1f, duration);
    }
}
