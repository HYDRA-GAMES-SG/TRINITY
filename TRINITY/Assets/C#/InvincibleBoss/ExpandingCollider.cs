using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandingCollider : MonoBehaviour
{
    public SphereCollider sphereCollider; 
    public float expansionDuration = 1f;
    private float initialRadius = 2f;
    private float targetRadius = 19f;

    void Start()
    {
        if (sphereCollider == null)
        {
            sphereCollider = GetComponent<SphereCollider>();
        }

        sphereCollider.radius = initialRadius;
        sphereCollider.enabled = false;

        ActivateCollider();
    }

    public void ActivateCollider()
    {
        sphereCollider.enabled = true;
        StartCoroutine(ExpandCollider());
    }

    private IEnumerator ExpandCollider()
    {
        float elapsedTime = 0f;

        while (elapsedTime < expansionDuration)
        {
            sphereCollider.radius = Mathf.Lerp(initialRadius, targetRadius, elapsedTime / expansionDuration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        sphereCollider.radius = targetRadius;

        sphereCollider.enabled = false;
    }
}
