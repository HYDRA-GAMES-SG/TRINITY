using UnityEngine;

public class MovementController : MonoBehaviour
{
    public bool useVelocity = true;
    public bool useRotation = true;
    public bool useAcceleration = true;

    public Vector3 initialVelocity = new Vector3(5f, 0f, 0f); // Initial velocity of the object
    public Vector3 acceleration; // Acceleration of the object
    public Vector3 rotationSpeed; // Rotation speed of the object on each axis
    public Vector3 offsetRange; // Range of offset value for the transform

    private Vector3 initialLocalPosition;

    void Start()
    {
        // Store the initial local position of the object
        initialLocalPosition = transform.localPosition;

        // Generate random offset values within the specified range
        float randomOffsetX = Random.Range(-offsetRange.x, offsetRange.x);
        float randomOffsetY = Random.Range(-offsetRange.y, offsetRange.y);
        float randomOffsetZ = Random.Range(-offsetRange.z, offsetRange.z);

        // Apply the random offset to the initial local position
        transform.localPosition = initialLocalPosition + new Vector3(randomOffsetX, randomOffsetY, randomOffsetZ);
    }

    void Update()
    {
        if (useVelocity)
        {
            // Move the object with the given velocity and acceleration
            MoveWithVelocityAndAcceleration();
        }

        if (useRotation)
        {
            // Rotate the object with the given rotation speed
            RotateObject();
        }
    }

    void MoveWithVelocityAndAcceleration()
    {
        if (useAcceleration)
        {
            // Calculate the new velocity based on the acceleration and time
            Vector3 newVelocity = initialVelocity + acceleration * Time.deltaTime;

            // Calculate the displacement using the average velocity
            Vector3 displacement = (initialVelocity + newVelocity) * 0.5f * Time.deltaTime;

            // Update the position of the object locally
            transform.localPosition += displacement;

            // Update the initial velocity to the new velocity
            initialVelocity = newVelocity;
        }
        else
        {
            // Update the position of the object locally using only the initial velocity
            transform.localPosition += initialVelocity * Time.deltaTime;
        }
    }

    void RotateObject()
    {
        // Rotate the object around its respective local axis with the given rotation speed
        if (rotationSpeed.x != 0f)
        {
            transform.Rotate(rotationSpeed.x * Time.deltaTime, 0f, 0f, Space.Self);
        }
        if (rotationSpeed.y != 0f)
        {
            transform.Rotate(0f, rotationSpeed.y * Time.deltaTime, 0f, Space.Self);
        }
        if (rotationSpeed.z != 0f)
        {
            transform.Rotate(0f, 0f, rotationSpeed.z * Time.deltaTime, Space.Self);
        }
    }
}
