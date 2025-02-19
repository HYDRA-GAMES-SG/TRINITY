using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

public class FlameblastZone : MonoBehaviour
{
    public GameObject FireballExplosion;
    public Volume FlameblastZone_PP;

    private AudioSource ZoneSource;

    public AudioClip BurstSFX;

    private float Duration;

    public Vector3 Centre;

    public GameObject FireballExplosionInstance;

    public bool bTutorial = false;
 
    void Start()
    {
        ZoneSource = GetComponent<AudioSource>();
        // Start a coroutine to pause particles after 1 second
        StartCoroutine(PauseParticlesAfterDelay());
        Duration = ATrinityGameManager.GetSpells().SecondaryFire.ZoneDuration;
        
        if (ATrinityGameManager.GetCamera())
        {
            FlameblastZone_PP = ATrinityGameManager.GetCamera().gameObject.transform.Find("PP_Flameblast").GetComponent<Volume>();
        }
    }

    void Update()
    {
        if (ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu && ATrinityGameManager.GetCamera())
        {
            FlameblastZone_PP = ATrinityGameManager.GetCamera().gameObject.transform.Find("PP_Flameblast").GetComponent<Volume>();
        }
        else
        {
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, ATrinityGameManager.GetPlayerController().Position);
        float ppRatio = Mathf.Clamp01(distanceToPlayer / 20f);
        
        FlameblastZone_PP.weight = Mathf.Lerp(.5f, 0, ppRatio);

        if (!bTutorial)
        {
            Duration -= Time.deltaTime;
        }
        
        if (Duration < 0f)
        {
            UnpauseParticles();
            Destroy(this.gameObject, 4f);
        }
    }

    void OnDestroy()
    {
        FlameblastZone_PP.weight = 0f;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Fireball>())
        {
            ZoneSource.PlayOneShot(BurstSFX);
            FireballExplosionInstance = Instantiate(FireballExplosion, transform.position, Quaternion.identity);
            FireballExplosionInstance.transform.localScale = Vector3.one * ATrinityGameManager.GetSpells().SecondaryFire.CurrentRadius / 3f;

            foreach (ParticleSystem ps in FireballExplosionInstance.GetComponentsInChildren<ParticleSystem>().ToList())
            {
                ps.Play();
            }
            
            Destroy(FireballExplosionInstance, 2f);
            
            ASecondaryFire flameblast = ATrinityGameManager.GetSpells().SecondaryFire;
            //Ray ray = new Ray(transform.position - new Vector3(5,5,5), Vector3.up);
            //Physics.SphereCast(ray, flameblast.CurrentRadius + 5, out RaycastHit hitInfo, 12f);
            Collider[] collidersHit = Physics.OverlapSphere(transform.position, 5);

            UEnemyColliderComponent enemyCollider;
            for (int i = 0; i < collidersHit.Length; i++) 
            {
                if (collidersHit[i].GetComponent<UEnemyColliderComponent>()) 
                {
                    print("overlap sphere hits");

                    enemyCollider = collidersHit[i].GetComponent<UEnemyColliderComponent>();

                    if (enemyCollider != null)
                    {
                        FDamageInstance damage = new FDamageInstance(other.GetComponent<Fireball>().Damage
                                                                     + flameblast.DamagePerStack * flameblast.CurrentRadius,
                                                                    EAilmentType.EAT_Ignite,
                                                                    ATrinityGameManager.GetSpells().PrimaryFire.StacksApplied);
                        enemyCollider.EnemyStatus += damage;
                        return;
                    }
                }
            }
            //if (hitInfo.collider != null)
            //{
            //    print("spherecast hits");

            //    print("Spherecast collider not null)");
            //    hitInfo.collider.TryGetComponent<UEnemyColliderComponent>(out enemyCollider);

            //    if (enemyCollider != null)
            //    {
            //        FDamageInstance damage = new FDamageInstance(other.GetComponent<Fireball>().Damage
            //                                                     + flameblast.DamagePerStack * flameblast.CurrentRadius,
            //                                                    EAilmentType.EAT_Ignite,
            //                                                    ATrinityGameManager.GetSpells().PrimaryFire.StacksApplied);
            //        enemyCollider.EnemyStatus += damage;
            //    }
            //}
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