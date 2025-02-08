using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ThirdPersonCamera;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ATrinityCamera : MonoBehaviour
{
    private CameraController CameraController;
    private OverTheShoulder OverTheShoulderCameraComponent;
    [HideInInspector]
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
    private MotionBlur PP_BlinkBlur;
    private ChromaticAberration PP_BlinkCA;
    private LensDistortion PP_BlinkWarp;


    private ScriptableRendererFeature GlideLines;
    
    void Start()
    {
        CameraShakeComponent = LookAtObject.GetComponent<UCameraShakeComponent>();
        
        transform.Find("PP_Blink").gameObject.GetComponent<Volume>().profile.TryGet(out PP_BlinkBlur);
        transform.Find("PP_Blink").gameObject.GetComponent<Volume>().profile.TryGet(out PP_BlinkCA);
        transform.Find("PP_Blink").gameObject.GetComponent<Volume>().profile.TryGet(out PP_BlinkWarp);

        PP_BlinkWarp.intensity.overrideState = true;
        PP_BlinkWarp.intensity.value = 0f;
        

        PP_BlinkCA.intensity.overrideState = true;
        PP_BlinkCA.intensity.value = 0f;
        
        PP_BlinkBlur.intensity.overrideState = true;
        PP_BlinkBlur.intensity.value = 0f;
        
        
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
        ScriptableRenderer renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
        PropertyInfo property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
        List<ScriptableRendererFeature> RenderPipelineFeatures = property.GetValue(renderer) as List<ScriptableRendererFeature>;
        
        foreach (ScriptableRendererFeature feature in RenderPipelineFeatures)
        {
            if (feature.name == "GlideLines")
            {
                GlideLines = feature;
            }
        }



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
            
            PP_BlinkBlur.intensity.value = Mathf.Lerp(.6f, .1f, t);
            PP_BlinkCA.intensity.value = Mathf.Lerp(.25f, 0f, t);
            PP_BlinkWarp.intensity.value = Mathf.Lerp(.60f, 0f, t);

            // Stop lerping once the time is exceeded
            if (CurrentLerpTime >= BlinkCameraLerpTime)
            {
                OverTheShoulderCameraComponent.ReleaseDistance = OriginalCameraDistance;
                bBlinkLerp = false;
                PP_BlinkBlur.intensity.value = 0.0f;
                PP_BlinkCA.intensity.value = 0.0f;
                PP_BlinkWarp.intensity.value = 0.0f;
            }
        }


        if (ATrinityGameManager.GetPlayerFSM().CurrentState is NormalMovement state)
        {
            if (state.GetMovementState() == ETrinityMovement.ETM_Gliding)
            {
                GlideLines.SetActive(true);
            }
            else
            {
                GlideLines.SetActive(false);
            }
        }

        HandlePostProcessing();
        

    }

    private void HandlePostProcessing()
    {
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PAUSED)
        {
            SwitchPostProcessing("PP_PAUSE");
            return;
        }
        
        if (ATrinityGameManager.GetSpells().SecondaryCold.IceCubeInstance.GetComponent<IceCube>().Mesh.enabled)
        {
            if (IsPointInBoxCollider(transform.position, ATrinityGameManager.GetSpells().SecondaryCold.IceCubeTrigger, .08f))
            {
                SwitchPostProcessing("PP_IceCube");
                return;
            }
            
        }
        else if (ATrinityGameManager.GetSpells().UtilityCold.bActive)
        {
            //SwitchPostProcessing("PP_IceCube");
            return;
        }
        
        SwitchPostProcessing("PP_Default");
        
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
