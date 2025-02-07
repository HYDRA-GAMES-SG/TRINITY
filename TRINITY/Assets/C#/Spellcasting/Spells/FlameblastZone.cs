using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class FlameblastZone : MonoBehaviour
{
    public GameObject FireballExplosion;
    public Volume FlameblastZone_PP;
    
    private float Duration;
    void Start()
    {
        
        // Start a coroutine to pause particles after 1 second
        StartCoroutine(PauseParticlesAfterDelay());
        Duration = ATrinityGameManager.GetSpells().SecondaryFire.ZoneDuration;
        FlameblastZone_PP = ATrinityGameManager.GetCamera().gameObject.transform.Find("PP_Flameblast").GetComponent<Volume>();
    }

    void Update()
    {
        Duration -= Time.deltaTime;
        if (Duration < 0f)
        {
            UnpauseParticles();
            Destroy(this.gameObject, 4f);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, ATrinityGameManager.GetPlayerController().Position);
        float ppRatio = Mathf.Clamp01(distanceToPlayer / 20f);
        
        FlameblastZone_PP.weight = Mathf.Lerp(.5f, 0, ppRatio);
    }

    void OnDestroy()
    {
        FlameblastZone_PP.weight = 0f;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Fireball>())
        {
            GameObject fireballExplosion = Instantiate(FireballExplosion, transform.position, Quaternion.identity);
            fireballExplosion.transform.localScale = Vector3.one * ATrinityGameManager.GetSpells().SecondaryFire.CurrentRadius / 3f;

            foreach (ParticleSystem ps in fireballExplosion.GetComponentsInChildren<ParticleSystem>().ToList())
            {
                ps.Play();
            }
            
            Destroy(fireballExplosion, 2f);
            
            ASecondaryFire flameblast = ATrinityGameManager.GetSpells().SecondaryFire;
            Ray ray = new Ray(transform.position, Vector3.up);
            Physics.SphereCast(ray, flameblast.CurrentRadius, out RaycastHit hitInfo, 5f);

            UEnemyColliderComponent enemyCollider;
            if (hitInfo.collider != null)
            {
                print("spherecast hits");

                //print("Spherecast collider not null)");
                hitInfo.collider.TryGetComponent<UEnemyColliderComponent>(out enemyCollider);

                if (enemyCollider != null)
                {
                    FDamageInstance damage = new FDamageInstance(other.GetComponent<Fireball>().Damage 
                                                                 + flameblast.DamagePerStack * flameblast.CurrentRadius, 
                                                                EAilmentType.EAT_Ignite, 
                                                                ATrinityGameManager.GetSpells().PrimaryFire.StacksApplied);
                    enemyCollider.EnemyStatus += damage;
                }
            }
        }
    }

    private IEnumerator PauseParticlesAfterDelay()
    {
        // Wait for 1 second
        yield return new WaitForSeconds(1f);
        
        // Get all particle systems on this object and its children
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        
        // Pause each particle system
        foreach (ParticleSystem ps in particles)
        {
            ps.Pause();
        }
    }
    private void UnpauseParticles()
    {
        // Get all particle systems on this object and its children
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        
        // Pause each particle system
        foreach (ParticleSystem ps in particles)
        {
            ps.Play();
        }
    }
}