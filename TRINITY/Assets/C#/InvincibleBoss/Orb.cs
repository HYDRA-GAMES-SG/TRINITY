using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] float targetSize = 1f;
    [SerializeField] float duration = 5f;
    [SerializeField] ParticleSystem AOE;
    [SerializeField] LayerMask CollideMask;
    private void Start()
    {
        StartCoroutine(ScaleOverTime(targetSize, duration));
    }

    private IEnumerator ScaleOverTime(float target, float time)
    {
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.one * target;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((CollideMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            ParticleSystem spawnedAOE = Instantiate(AOE, transform.position, Quaternion.identity);
            spawnedAOE.Play();
            Destroy(gameObject);
        }
    }
}
