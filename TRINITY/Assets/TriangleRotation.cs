using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleRotation : MonoBehaviour
{
    public float AngleToRotate = 120f;
    public float RotationSpeed;

    RectTransform CurrentTransform;
    Quaternion TargetRotation;
    private bool bIsRotating = false;

    void Start()
    {
        CurrentTransform = GetComponent<RectTransform>();
        TargetRotation = CurrentTransform.rotation;
    }
    private void Update()
    {
        if (bIsRotating)
        {
            CurrentTransform.rotation = Quaternion.Lerp(CurrentTransform.rotation, TargetRotation, Time.deltaTime * RotationSpeed);

            if (Quaternion.Angle(CurrentTransform.rotation, TargetRotation) < 0.05f)
            {
                CurrentTransform.rotation = TargetRotation;
                bIsRotating = false;
            }
        }
    }
    public void TurnLeft()
    {
        if (!bIsRotating)
        {
            SetTargetRotation(-AngleToRotate);
        }
    }
    public void TurnRight()
    {
        if (!bIsRotating)
        {
            SetTargetRotation(AngleToRotate);
        }

    }
    private void SetTargetRotation(float angle)
    {
        Vector3 currentEuler = CurrentTransform.rotation.eulerAngles;
        Vector3 targetEuler = currentEuler + new Vector3(0, 0, angle);

        TargetRotation = Quaternion.Euler(targetEuler);

        bIsRotating = true;
    }
}
