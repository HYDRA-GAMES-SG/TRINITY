using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    public GameObject[] CloudPrefabs;
    [SerializeField] float SizeLerpDuration = 2f;
    [SerializeField] int CloudSpawnAmout = 5;
    [SerializeField] Vector3 SpawnCloudsArea = new Vector3(50f, 0,50f);
    [SerializeField] Vector2 minMaxSize = new Vector2(30f, 45f);

    List<CloudData> _activeClouds = new List<CloudData>();
    void Start()
    {
        SpawnClouds();
    }

    void SpawnClouds()
    {
        for (int i = 0; i < CloudSpawnAmout; i++)
        {
            SpawnCloudFunction();
        }
    }
    void SpawnCloudFunction()
    {
        int randomCloudIndex = Random.Range(0, CloudPrefabs.Length);
        GameObject cloud = Instantiate(CloudPrefabs[randomCloudIndex]);

        float randomX = Random.Range(transform.position.x - SpawnCloudsArea.x / 2, transform.position.x + SpawnCloudsArea.x / 2);
        float randomY = Random.Range(transform.position.y - SpawnCloudsArea.y / 2, transform.position.y + SpawnCloudsArea.y / 2);
        float randomZ = Random.Range(transform.position.z - SpawnCloudsArea.z / 2, transform.position.z + SpawnCloudsArea.z / 2);

        cloud.transform.position = new Vector3(randomX, randomY, randomZ);

        Vector3 initialScale = cloud.transform.localScale;
        Vector3 targetScale = initialScale * Random.Range(minMaxSize.x, minMaxSize.y);

        _activeClouds.Add(new CloudData
        {
            CloudObject = cloud,
            InitialScale = initialScale,
            TargetScale = targetScale,
            StartTime = Time.time
        });
    }
    private void Update()
    {
        for (int i = _activeClouds.Count - 1; i >= 0; i--)
        {
            CloudData cloudData = _activeClouds[i];
            float elapseTime = Time.time - cloudData.StartTime;

            if (elapseTime < SizeLerpDuration)
            {
                float t = elapseTime / SizeLerpDuration;
                t = Mathf.SmoothStep(0f, 1f, t);
                cloudData.CloudObject.transform.localScale = Vector3.Lerp(cloudData.InitialScale, cloudData.TargetScale, t);
            }
            else
            {
                cloudData.CloudObject.transform.localScale = cloudData.TargetScale;
                _activeClouds.RemoveAt(i);
            }
        }
    }
    class CloudData
    {
        public GameObject CloudObject;
        public Vector3 InitialScale;
        public Vector3 TargetScale;
        public float StartTime;
    }
}
