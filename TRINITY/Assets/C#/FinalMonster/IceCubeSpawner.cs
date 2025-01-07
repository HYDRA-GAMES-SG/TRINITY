using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCubeSpawner : MonoBehaviour
{
    public GameObject IceCubePref;
    public Transform parentTransform;
    public int rows = 30;
    public int columns = 30;
    public float rowSpacing = 1.0f;
    public float columnSpacing = 1.0f;

    void Start()
    {
        SpawnGrid();
    }

    void SpawnGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector3 localPosition = new Vector3(column * columnSpacing, 0, row * rowSpacing);

                GameObject block = Instantiate(IceCubePref, parentTransform);

                block.transform.localPosition = localPosition;
            }
        }
    }

}
