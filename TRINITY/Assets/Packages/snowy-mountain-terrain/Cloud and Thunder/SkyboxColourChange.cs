using UnityEditor;
using UnityEngine;

public class SkyboxColourChange : MonoBehaviour
{
    [SerializeField] float transitionDuration = 10f;
    [SerializeField] float reverseTransitionDuration = 5f;
    [SerializeField] Color initialColor = Color.white;
    [SerializeField] Color targetColor = new Color(0.482f, 0.482f, 0.482f);

    public UHealthComponent FlyingBossHealth;

    float startTime;

    void Start()
    {
        if (RenderSettings.skybox == null)
        {
            return;
        }

        startTime = Time.time;
        RenderSettings.skybox.SetColor("_Tint", initialColor);
    }

    void Update()
    {
        if (FlyingBossHealth.bDead)
        {
            TransitionBackToInitialColor();
        }
        else
        {
            TransitionToTargetColor();
        }
    }
    void TransitionToTargetColor()
    {
        float elapsedTime = Time.time - startTime;
        float t = Mathf.Clamp01(elapsedTime / transitionDuration);

        Color currentColor = Color.Lerp(initialColor, targetColor, t);
        Debug.Log($"Transitioning to target color: t = {t}, currentColor = {currentColor}");

        RenderSettings.skybox.SetColor("_Tint", currentColor);
        DynamicGI.UpdateEnvironment();
    }

    void TransitionBackToInitialColor()
    {
        float elapsedTime = Time.time - startTime;
        float t = Mathf.Clamp01(elapsedTime / reverseTransitionDuration);

        Color currentColor = Color.Lerp(targetColor, initialColor, t);
        Debug.Log($"Transitioning back to initial color: t = {t}, currentColor = {currentColor}");

        RenderSettings.skybox.SetColor("_Tint", currentColor);
        DynamicGI.UpdateEnvironment();
    }
}