using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [SerializeField] float minSpeed = 1f;
    [SerializeField] float maxSpeed = 3f;
    [SerializeField] float minThunderInterval = 5f;
    [SerializeField] float maxThunderInterval = 15f;
    [SerializeField] float scaleLerpDuration = 4f;
    [SerializeField] Vector3 movementDirection = Vector3.right;

    ParticleSystem[] thunderParticles;
    float currentSpeed;
    float scaleTimer;
    float thunderTimer;
    Vector2 minMaxSize = new Vector2(550f, 750f);
    Vector3 movementAreaSize = new Vector3(250f, 50f, 250f);
    Vector3 initialPosition;
    Vector3 initialScale;
    Vector3 targetScale;

    void Start()
    {
        thunderParticles = GetComponentsInChildren<ParticleSystem>();
        thunderTimer = Random.Range(minThunderInterval, maxThunderInterval);
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        initialPosition = transform.position;
        SetRandomScale();
    }

    void Update()
    {
        transform.Translate(movementDirection * currentSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - initialPosition.x) > movementAreaSize.x / 2 ||
            Mathf.Abs(transform.position.y - initialPosition.y) > movementAreaSize.y / 2 ||
            Mathf.Abs(transform.position.z - initialPosition.z) > movementAreaSize.z / 2)
        {
            ResetCloudPosition();
        }
        scaleTimer += Time.deltaTime;
        if (scaleTimer >= scaleLerpDuration)
        {
            SetRandomScale();
            scaleTimer = 0f;
        }
        transform.localScale = Vector3.Lerp(initialScale, targetScale, Mathf.SmoothStep(0f, 1f, scaleTimer / scaleLerpDuration));
        thunderTimer += Time.deltaTime;
        if (thunderTimer >= Random.Range(minThunderInterval, maxThunderInterval))
        {
            if (thunderParticles != null)
            {
                PlayRandomThunder();
            }
            thunderTimer = 0f;
        }
    }

    void ResetCloudPosition()
    {
        float newX = Mathf.Clamp(
            transform.position.x,
            initialPosition.x - movementAreaSize.x / 2,
            initialPosition.x + movementAreaSize.x / 2
        );

        float newY = Mathf.Clamp(
            transform.position.y,
            initialPosition.y - movementAreaSize.y / 2,
            initialPosition.y + movementAreaSize.y / 2
        );

        float newZ = Mathf.Clamp(
            transform.position.z,
            initialPosition.z - movementAreaSize.z / 2,
            initialPosition.z + movementAreaSize.z / 2
        );

        transform.position = new Vector3(newX, newY, newZ);

        movementDirection = -movementDirection;
    }
    void SetRandomScale()
    {
        initialScale = transform.localScale;
        targetScale = new Vector3
            (
            Random.Range(minMaxSize.x, minMaxSize.y),
            Random.Range(minMaxSize.x, minMaxSize.y),
            Random.Range(minMaxSize.x, minMaxSize.y)
            );
    }
    void PlayRandomThunder()
    {
        int numThunderToPlay = Random.Range(1, thunderParticles.Length + 1);

        for (int i = 0; i < numThunderToPlay; i++)
        {
            int randomIndex = Random.Range(0, thunderParticles.Length);
            if (thunderParticles[randomIndex] != null)
            {
                thunderParticles[randomIndex].Play();
            }
        }
    }
}
