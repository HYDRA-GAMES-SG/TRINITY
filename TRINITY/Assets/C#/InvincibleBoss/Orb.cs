using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Orb : MonoBehaviour
{
    [SerializeField] float targetSize = 1f;
    [SerializeField] float duration = 5f;
    [SerializeField] ParticleSystem AOE;
    [SerializeField] LayerMask CollideMask;

    AInvincibleBossController IBController;
    float Damage;

    private void Start()
    {
        StartCoroutine(ScaleOverTime(targetSize, duration));
    }

    public IEnumerator ScaleOverTime(float target, float time)
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
            if (collision.gameObject.CompareTag("Player"))
            {
                ATrinityController player = collision.gameObject.GetComponent<ATrinityController>();
                if (player != null)
                {
                    FHitInfo hitInfo = new FHitInfo(IBController.gameObject, this.gameObject, collision, IBController.GetCurrentAttackDamage());
                    player.ApplyHit(hitInfo);
                    Debug.Log($"Particles Hit player " + IBController.GetCurrentAttackDamage());

                }
            }

            if (AOE != null)
            {
                ParticleSystem spawnedAOE = Instantiate(AOE, transform.position, Quaternion.identity);
                OrbExplosion projectileController = spawnedAOE.GetComponentInChildren<OrbExplosion>();
                projectileController.SetController(IBController);
                //spawnedAOE.Play();
            }

            Destroy(gameObject);
        }
    }

    public void GetController(AInvincibleBossController controller)
    {
        IBController = controller;
        Damage = controller.GetCurrentAttackDamage();
    }
}
