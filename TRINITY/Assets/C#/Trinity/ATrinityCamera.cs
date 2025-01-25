using System;
using System.Collections;
using System.Diagnostics;
using ThirdPersonCamera;
using Unity.VisualScripting;
using UnityEngine;

public class ATrinityCamera : MonoBehaviour
{
    private CameraController CameraController;
    private OverTheShoulder OverTheShoulderCameraComponent;
    public UCameraShakeComponent CameraShakeComponent;
    public Camera Camera;
    public GameObject LookAtObject;
    public float BlinkCameraLerpTime = 1f;
    public float BlinkCameraDistance = 5f;

    private float OriginalCameraDistance; // To store the original value of ReleaseDistance
    private float CurrentLerpTime;         // To track lerp progress
    private bool bBlinkLerp;// To track if a blink event is active
    private bool bBulletTimeLerp; //to track if a bullet time event is active

    public float BulletTimeDuration = .3f;
    public float BulletTimeDistance = 5f;
    public float BulletTimeScale = .5f;

    private string PostProcessingLayerName = "PP_Default";
    
    void Start()
    {
        CameraShakeComponent = LookAtObject.GetComponent<UCameraShakeComponent>();
        
        ATrinityGameManager.SetCamera(this);
        
        Cursor.visible = false;
        Camera = GetComponent<Camera>();

        CameraController = GetComponent<CameraController>();
        OverTheShoulderCameraComponent = GetComponent<OverTheShoulder>();
        if (OverTheShoulderCameraComponent != null)
        {
            OriginalCameraDistance = OverTheShoulderCameraComponent.ReleaseDistance;
        }
        
        {
            OriginalCameraDistance = OverTheShoulderCameraComponent.ReleaseDistance;
        }

        foreach(IEnemyController enemyController in ATrinityGameManager.GetEnemyControllers())
        {
            enemyController.OnBulletTime += HandleBulletTime;
        }
        
        ABlink.BlinkCamera += HandleBlink;

        SwitchPostProcessing("PP_Default");
    }

    private void HandleBulletTime()
    {
        if (Time.timeScale != 1.0f || bBulletTimeLerp)
        {
            return;
        }
        
        bBulletTimeLerp = true;
        // Start coroutine to return to normal time
        StartCoroutine(BulletTimeLerpRoutine());
    }

    private IEnumerator BulletTimeLerpRoutine()
    {
        // First lerp down to slow motion
        float startTimeScale = Time.timeScale;
        float originalFixedDeltaTime = Time.fixedDeltaTime;

        float elapsedTime = 0f;
    
        while (elapsedTime < BulletTimeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime since we're modifying time scale
            float t = elapsedTime / BulletTimeDuration;
        
            Time.timeScale = Mathf.Lerp(startTimeScale, BulletTimeScale, t);
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            yield return null;
        }
    
        // Then lerp back to normal speed
        elapsedTime = 0f;
        while (elapsedTime < BulletTimeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / BulletTimeDuration;
        
            Time.timeScale = Mathf.Lerp(BulletTimeScale, 1f, t);
            yield return null;
        }
    
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        bBulletTimeLerp = false;
    }

    private void HandleBlink()
    {
        if (OverTheShoulderCameraComponent != null)
        {
            OverTheShoulderCameraComponent.ReleaseDistance = BlinkCameraDistance;
            CurrentLerpTime = 0f; // Reset lerp time
            bBlinkLerp = true;    // Start lerp process
        }
    }

    private void OnDestroy()
    {
        ABlink.OnBlink -= HandleBlink;
        
        foreach(IEnemyController enemyController in ATrinityGameManager.GetEnemyControllers())
        {
            enemyController.OnBulletTime -= HandleBulletTime;
        }
    }

    void Update()
    {
        if (bBlinkLerp && OverTheShoulderCameraComponent != null)
        {
            // Increment the lerp time
            CurrentLerpTime += Time.deltaTime;
            float t = CurrentLerpTime / BlinkCameraLerpTime;

            // Lerp the ReleaseDistance value back to its original value
            OverTheShoulderCameraComponent.ReleaseDistance = Mathf.Lerp(BlinkCameraDistance, OriginalCameraDistance, t);

            // Stop lerping once the time is exceeded
            if (CurrentLerpTime >= BlinkCameraLerpTime)
            {
                OverTheShoulderCameraComponent.ReleaseDistance = OriginalCameraDistance;
                bBlinkLerp = false;
            }
        }
        
        if (ATrinityGameManager.GetSpells().SecondaryCold.IceCubeInstance.GetComponent<IceCube>().Mesh.enabled)
        {
            if (IsPointInBoxCollider(transform.position, ATrinityGameManager.GetSpells().SecondaryCold.IceCubeTrigger, .08f))
            {
                SwitchPostProcessing("PP_IceCube");
                return;
            }
            else
            {
                SwitchPostProcessing("PP_Default");
            }
        }
        else
        {
            SwitchPostProcessing("PP_Default");
        }

    }

    void LateUpdate()
    {
        // Additional LateUpdate logic if needed
    }
    
    public bool IsPointInBoxCollider(Vector3 point, BoxCollider boxCollider, float sizeOffset = 0f) 
    {
        point = boxCollider.transform.InverseTransformPoint(point);
        
        Vector3 bounds = new Vector3(
            (boxCollider.size.x + sizeOffset) * 0.5f,
            (boxCollider.size.y + sizeOffset) * 0.5f,
            (boxCollider.size.z + sizeOffset) * 0.5f
        );
    
        return Mathf.Abs(point.x) <= bounds.x && 
               Mathf.Abs(point.y) <= bounds.y && 
               Mathf.Abs(point.z) <= bounds.z;
    }

    void SwitchPostProcessing(string postProcessingChildObjectName)
    {
        if (postProcessingChildObjectName == PostProcessingLayerName)
        {
            return;
        }
        
        if (transform.Find(postProcessingChildObjectName))
        {
            transform.Find(postProcessingChildObjectName).gameObject.SetActive(true);
            transform.Find(PostProcessingLayerName).gameObject.SetActive(false);
            PostProcessingLayerName = postProcessingChildObjectName;
        }
    }
}
