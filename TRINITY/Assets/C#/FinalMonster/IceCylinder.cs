using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IceCylinder : MonoBehaviour
{
    public float CountDownTime = 5f;
    public float MinOffsetY;
    public float MaxOffsetY;
    public Transform Player;

    float targetY;
    float originalPositionY;
    float timer;

    void Start()
    {
        originalPositionY = transform.position.y;
        timer = CountDownTime;
        targetY = originalPositionY + Random.Range(MinOffsetY, MaxOffsetY);
    }
    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0)
        {
            float t = 1 - (timer / CountDownTime);
            Vector3 newPosition = transform.position;
            newPosition.y = Mathf.Lerp(originalPositionY, targetY, t);
            transform.position = newPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Player = collision.collider.transform;
            Player.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Player.SetParent(null);
            Player = null;
        }
    }
}
