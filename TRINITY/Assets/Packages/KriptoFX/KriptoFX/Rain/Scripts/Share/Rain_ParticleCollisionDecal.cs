using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Rain_ParticleCollisionDecal : MonoBehaviour
{
    public ParticleSystem DecalParticles;
    public float MaxGroundAngleDeviation = 30;
    public Vector3 RandomizeRotation = new Vector3(0, 0, 360);
    public bool LookAtForward;

    public bool InstantiateWhenZeroSpeed;
    public float MinDistanceBetweenDecals = 0.1f;


    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    ParticleSystem.Particle[] particles;

    ParticleSystem initiatorPS;
    List<GameObject> collidedGameObjects = new List<GameObject>();
    bool canUpdateCollisionDetect;
    float collisionDetectTime = 0.2f;
    float currentCollisionTime;
    Vector3 prevPos;
    Vector3 currentDirection;

    void OnEnable()
    {
        collisionEvents.Clear();
        collidedGameObjects.Clear();
        initiatorPS = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[DecalParticles.main.maxParticles];
        if (InstantiateWhenZeroSpeed) canUpdateCollisionDetect = true;
        prevPos = transform.position - transform.forward;
    }

    void OnDisable()
    {
        if (InstantiateWhenZeroSpeed) canUpdateCollisionDetect = false;
    }

    void CollisionDetect()
    {
        int aliveParticles = 0;

        if (InstantiateWhenZeroSpeed)
            aliveParticles = DecalParticles.GetParticles(particles);
        foreach (var collidedGameObject in collidedGameObjects)
        {
            OnParticleCollisionManual(collidedGameObject, aliveParticles);
        }
    }

    void Update()
    {
        if (LookAtForward)
        {
            var tempDir = transform.position - prevPos;
            if (tempDir.magnitude > 0.001f) currentDirection = tempDir;
            else currentDirection = transform.forward;
            prevPos = transform.position;
            currentDirection.y = 0;
        }

        if (!InstantiateWhenZeroSpeed || !canUpdateCollisionDetect) return;

        currentCollisionTime += Time.deltaTime;
        if (currentCollisionTime > collisionDetectTime)
        {
            currentCollisionTime = 0;
            CollisionDetect();
        }
    }

    private void OnParticleCollisionManual(GameObject other, int aliveParticles = -1)
    {
       
        collisionEvents.Clear();
        if (other == null) return;

        var aliveEvents = initiatorPS.GetCollisionEvents(other, collisionEvents);
        for (int i = 0; i < aliveEvents; i++)
        {
          
            var angle = Vector3.Angle(collisionEvents[i].normal, Vector3.up);
            if (angle > MaxGroundAngleDeviation) continue;
            if (InstantiateWhenZeroSpeed)
            {
               
                if (collisionEvents[i].velocity.magnitude > initiatorPS.main.gravityModifier.constantMax) continue;
                var isNearDistance = false;
                for (int j = 0; j < aliveParticles; j++)
                {
                    var distance = Vector3.Distance(collisionEvents[i].intersection, particles[j].position);
                    if (distance < MinDistanceBetweenDecals) isNearDistance = true;
                }
                if (isNearDistance) continue;
            }
            var emiter = new ParticleSystem.EmitParams();
            emiter.position = collisionEvents[i].intersection;
            Vector3 rotation;
           
            if (LookAtForward) rotation = Quaternion.LookRotation(currentDirection.normalized).eulerAngles;
            else rotation = Quaternion.LookRotation(-collisionEvents[i].normal).eulerAngles;
            
            if (RandomizeRotation.x > 0.01f) rotation.x += Random.Range(0, RandomizeRotation.x * 2) - RandomizeRotation.x;
            if (RandomizeRotation.y > 0.01f) rotation.y += Random.Range(0, RandomizeRotation.y * 2) - RandomizeRotation.y;
            if (RandomizeRotation.z > 0.01f) rotation.z += Random.Range(0, RandomizeRotation.z * 2) - RandomizeRotation.z;
            emiter.rotation3D = rotation;

            DecalParticles.Emit(emiter, 1);
            DecalParticles.Play();
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (InstantiateWhenZeroSpeed)
        {
            if (!collidedGameObjects.Contains(other))
                collidedGameObjects.Add(other);
        }
        else
        {
            OnParticleCollisionManual(other);
        }
    }
}
