using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IceCylinder : MonoBehaviour
{
    public float CountDownTime = 5f;
    public Vector3 MinOffset;
    public Vector3 MaxOffset;
    public bool BDectectedPlayer;

    Transform player;
    Vector3 originalPosition;
    float timer;
    bool BIsMovingUp;


    void Start()
    {
        originalPosition = transform.position;
        timer = CountDownTime;
    }
    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0)
        {
            float t = 1 - (timer / CountDownTime);

            if (BIsMovingUp)
            {
                transform.position = Vector3.Lerp(originalPosition + MinOffset, originalPosition + MaxOffset, t);
            }
            else
            {
                transform.position = Vector3.Lerp(originalPosition + MaxOffset, originalPosition + MinOffset, t);
            }
        }
        else
        {
            timer = CountDownTime;
            BIsMovingUp = !BIsMovingUp;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            player = collision.collider.transform;
            player.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            player.SetParent(null);
            player = null;
            Destroy(gameObject);
        }
    }
}
