using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UBulletPool : MonoBehaviour
{
    [SerializeField]
    public GameObject BulletPrefab;
    public bool bDisableBulletsOnSpawnerDeath;
    public int PoolSize = 20;
    private Queue<GameObject> PoolQueue = new Queue<GameObject>();
    private float BulletLifetime = 0f; 

    private Coroutine DisableCoro = null;
    private IEnemyController EnemyController;

    private void Awake()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            GameObject bullet = Instantiate(BulletPrefab);
            bullet.SetActive(false);
            bullet.GetComponentInChildren<ABullet>().Pool = this;
            PoolQueue.Enqueue(bullet);
        }
    }

    private void Start()
    {
        if (EnemyController == null)
        {
            EnemyController = GetComponent<IBulletSpawner>().EnemyController;
        }

        BulletLifetime = BulletPrefab.GetComponent<ABullet>().Lifetime;
    }

    private void OnEnable()
    {
        EnableColliders<SphereCollider>();
        EnableColliders<BoxCollider>();
    }

    private void Update()
    {

        if (EnemyController != null)
        {

            if (bDisableBulletsOnSpawnerDeath && IsSpawnerDead())
            {
                if (DisableCoro == null)
                {
                    DisableCoro = StartCoroutine(DisableBulletsCoroutine());
                }
            }
        }
    }

    private IEnumerator DisableBulletsCoroutine()
    {
            // Copy the active bullets to a list
            List<GameObject> activeBullets = new List<GameObject>();
            foreach (var bullet in PoolQueue)
            {
                if (bullet.activeSelf)
                {
                    activeBullets.Add(bullet);
                }
            }

            // Iterate over the list and disable bullets
            foreach (var bullet in activeBullets)
            {
                bullet.SetActive(false);
                yield return new WaitForSeconds(0.05f);
            }
    }

    private bool IsSpawnerDead()
    {
        if (EnemyController.gameObject.activeSelf == false)
        {
                return true;
        }
        else
        {
            return false;
        }

        return false;

    }

    public GameObject GetBullet()
    {
        if (PoolQueue.Count > 0)
        {
            GameObject bullet = PoolQueue.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        else
        {
            GameObject bullet = Instantiate(BulletPrefab);
            return bullet;
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.GetComponent<ABullet>().Reset();
        bullet.SetActive(false);
        PoolQueue.Enqueue(bullet);
    }
    
    private void EnableColliders<T>() where T : Collider
    {
        T[] colliders = GetComponentsInChildren<T>(true); // true includes inactive children
        foreach (T collider in colliders)
        {
            collider.enabled = true;
        }
    }
}
