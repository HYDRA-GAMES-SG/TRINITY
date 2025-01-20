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
    [SerializeField] Vector2 movementAreaSize = new Vector2(250f, 250f);
    [SerializeField] Vector2 minMaxSize = new Vector2(30f, 45f);

    ParticleSystem thunderParticles;
    float currentSpeed;
    float scaleTimer;
    float thunderTimer;
    Vector3 initialPosition;
    Vector3 initialScale;
    Vector3 targetScale;

    void Start()
    {
        thunderParticles = GetComponentInChildren<ParticleSystem>();
        thunderTimer = Random.Range(minThunderInterval, maxThunderInterval);
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        initialPosition = transform.position;
        SetRandomScale();
    }

    void Update()
    {
        transform.Translate(movementDirection * currentSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - initialPosition.x) > movementAreaSize.x / 2 ||
            Mathf.Abs(transform.position.y - initialPosition.y) > movementAreaSize.y / 2)
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
                thunderParticles.Play();
            }
            thunderTimer = 0f;
        }
    }

    void ResetCloudPosition()
    {
        float newX = transform.position.x;
        float newY = transform.position.y;

        if (Mathf.Abs(transform.position.x - initialPosition.x) > movementAreaSize.x / 2)
        {
            newX = initialPosition.x - Mathf.Sign(transform.position.x - initialPosition.x) * movementAreaSize.x / 2;
        }
        if (Mathf.Abs(transform.position.y - initialPosition.y) > movementAreaSize.y / 2)
        {
            newY = initialPosition.y - Mathf.Sign(transform.position.y - initialPosition.y) * movementAreaSize.y / 2;
        }
        transform.position = new Vector3(newX, newY, transform.position.z);
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        movementDirection = -movementDirection;
    }
    void SetRandomScale()
    {
        initialScale = transform.localScale;
        targetScale = Vector3.one * Random.Range(minMaxSize.x, minMaxSize.y);
    }
}
