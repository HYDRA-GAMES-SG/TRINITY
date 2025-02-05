using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UFullScreenEffect : MonoBehaviour
{

    //we have one timer
    //we have a threshold float .5f would be fade out starts at the half way point

    private bool bEffectActive => EffectTimer < EffectDuration;
    private float FadeOutTime => FadeOutThreshold * EffectDuration;

    private float EffectTimer;
    public float EffectDuration;
    public float FadeOutThreshold = .66f;

    [Header("References")]
    [SerializeField] private ScriptableRendererFeature FullScreenDamage;
    [SerializeField] private Material Material;

    [Header("Intensity Stats")]
    [SerializeField] private float VoronoiIntensityStat = 2.5f;
    [SerializeField] private float VignetteIntensityStat = 1.25f;


    private string VoronoiIntensity = "_VoronoiIntensity";
    private string VignetteIntensity = "_VignetteIntensity";

    private ATrinityController ATrinityController;

    // Start is called before the first frame update
    private void Awake()
    {

    }
    void Start()
    {
        ATrinityGameManager.GetPlayerController().OnHit += StartEffect;
        FullScreenDamage.SetActive(false);
        EffectTimer = EffectDuration;
    }

    public void StartEffect(FHitInfo hitInfo)
    {
        EffectTimer = 0f;
        FullScreenDamage.SetActive(true);
        Material.SetFloat(VoronoiIntensity, VoronoiIntensityStat);
        Material.SetFloat(VignetteIntensity, VignetteIntensityStat);
    }

    // Update is called once per frame
    void Update()
    {
        if (bEffectActive)
        {
            EffectTimer += Time.deltaTime;

            if ((EffectTimer / EffectDuration) > FadeOutThreshold)
            {
                float lerpedVoronoi = Mathf.Lerp(VoronoiIntensityStat, 0f, ((EffectTimer - FadeOutTime) / EffectDuration));
                float lerpedVignette = Mathf.Lerp(VignetteIntensityStat, 0f, ((EffectTimer - FadeOutTime) / EffectDuration));

                Material.SetFloat(VoronoiIntensity, lerpedVoronoi);
                Material.SetFloat(VignetteIntensity, lerpedVignette);
            }
        }
        else if (!bEffectActive) 
        {
            FullScreenDamage.SetActive(false);
        }
    }
}
