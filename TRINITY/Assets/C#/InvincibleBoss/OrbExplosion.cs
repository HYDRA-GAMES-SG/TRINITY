using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbExplosion : MonoBehaviour
{
    private AInvincibleBossController Controller;

    [Tooltip("If true, particles will deal damage only once during the lifetime of the particle system.")]
    public bool DealDamageOnlyOnce = true;
    public float ShakeDuration = 0.5f;
    public float delay;
    private bool hasDealtDamage = false;
    float timer;
    bool hasShake = false;
    public AudioSource OrbAudioSource;
    public AudioClip Chrage;
    public AudioClip Release;
    private void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        delay = ps.main.startDelay.constant;
        Controller.MediumCameraShake(.5f);
        OrbAudioSource.PlayOneShot(Chrage);
        Destroy(this.transform.root.gameObject, 5f);
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= delay && !hasShake)
        {
            hasShake = true;
            OrbAudioSource.PlayOneShot(Release);
            Controller.MediumCameraShake(.5f);
        }
    }
    private void OnParticleCollision(GameObject other)
    {
        //Debug.Log($"Particles Hit player ");
        if (DealDamageOnlyOnce && hasDealtDamage)
        {
            return;
        }

        ATrinityController player = other.GetComponent<ATrinityController>();

        if (player == null)
        {
            return;
        }

        FHitInfo hitInfo = new FHitInfo(Controller.gameObject, this.gameObject, null, Controller.OrbExplosionDMG);
        player.ApplyHit(hitInfo);
        hasDealtDamage = true;
        //Debug.Log($"Particles Hit player " + Controller.OrbExplosionDMG);
    }
    public void SetController(AInvincibleBossController enemyController)
    {
        Controller = enemyController;
    }

}
