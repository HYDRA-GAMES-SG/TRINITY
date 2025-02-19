using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldRope : MonoBehaviour
{
    public Transform pointA, pointB;  // Start and End points
    public int segmentCount = 10;     // More segments = smoother rope
    public float ropeSegmentLength = 0.5f; // Distance between segments
    public LineRenderer lineRenderer; // Reference to LineRenderer
    public GameObject ropeSegmentPrefab; // Prefab with Rigidbody

    public AInvincibleBossShield BossShield;

    private Material ShieldMaterial;
    public float HitEffectDuration = 0.2f;
    private float CurrentHitTime = 0f;

    private List<Transform> ropeSegments = new List<Transform>();

    void Start()
    {
        BossShield = pointB.GetComponentInParent<AInvincibleBossShield>();
        ShieldMaterial = lineRenderer.material;
        GenerateRope();
    }
    private void Update()
    {
        if (BossShield.bIsHit)
        {
            CurrentHitTime += Time.deltaTime;
            Vector4 alphaChannel = ShieldMaterial.GetVector("_MainAlphaChannel");
            alphaChannel.y = Mathf.Lerp(1f, -2f, CurrentHitTime / HitEffectDuration);
            ShieldMaterial.SetVector("_MainAlphaChannel", alphaChannel);

            if (CurrentHitTime >= HitEffectDuration)
            {
                CurrentHitTime = 0f;
            }
        }
    }
    void FixedUpdate()
    {
        UpdateRopeVisuals();
        UpdateRopeEndPositions();
    }

    void GenerateRope()
    {
        // Clear previous rope
        foreach (var segment in ropeSegments)
        {
            Destroy(segment.gameObject);
        }
        ropeSegments.Clear();

        // Create rope segments
        Vector3 segmentPosition = pointA.position;
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = Instantiate(ropeSegmentPrefab, segmentPosition, Quaternion.identity);
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            ropeSegments.Add(segment.transform);

            // Connect segments with Joints
            if (i > 0)
            {
                SpringJoint joint = segment.AddComponent<SpringJoint>();
                joint.connectedBody = ropeSegments[i - 1].GetComponent<Rigidbody>();
                joint.spring = 50f;   // Adjust stiffness
                joint.damper = 2f;    // Adjust damping
                joint.minDistance = 0f;
                joint.maxDistance = ropeSegmentLength;
            }

            segmentPosition += (pointB.position - pointA.position) / segmentCount;
        }

        // Set up LineRenderer
        lineRenderer.positionCount = segmentCount;
    }

    void UpdateRopeVisuals()
    {
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            lineRenderer.SetPosition(i, ropeSegments[i].position);
        }
    }

    void UpdateRopeEndPositions()
    {
        if (ropeSegments.Count > 0)
        {
            // Force the first and last segments to follow pointA and pointB
            ropeSegments[0].position = pointA.position;
            ropeSegments[0].GetComponent<Rigidbody>().velocity = Vector3.zero;
            ropeSegments[0].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            ropeSegments[^1].position = pointB.position;
            ropeSegments[^1].GetComponent<Rigidbody>().velocity = Vector3.zero;
            ropeSegments[^1].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}
