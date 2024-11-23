using UnityEngine;

[RequireComponent(typeof(UHealthComponent))]
[RequireComponent(typeof(UManaComponent))]

public class ATrinityCharacter : MonoBehaviour
{
    [SerializeField]
    private ATrinityController Controller;
    private UHealthComponent HealthComponent;
    private UManaComponent ManaComponent;

    // Start is called before the first frame update
    private void Start()
    {

        if (HealthComponent == null)
        {
            HealthComponent = GetComponent<UHealthComponent>();
        }

        if (ManaComponent == null)
        {
            ManaComponent = GetComponent<UManaComponent>();
        }

        // Log warnings if dependencies are missing
        if (Controller == null)
        {
            Debug.LogWarning("Controller is not assigned to ATrinityCharacter!", this);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Placeholder for future logic
    }
}