using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorAction : MonoBehaviour
{
    public TriangleRotation Triangle;

    public RectTransform leftPanel;
    public RectTransform rightPanel;
    public float checkTime;

    public bool useHover;

    float timer;

    void Update()
    {
        Vector2 cursorPosition = Mouse.current.position.ReadValue();
        bool isCursorOnLeftPanel = RectTransformUtility.RectangleContainsScreenPoint(leftPanel, cursorPosition);

        bool isCursorOnRightPanel = RectTransformUtility.RectangleContainsScreenPoint(rightPanel, cursorPosition);
        if (useHover)
        {
            timer += Time.deltaTime;
            if (timer > checkTime)
            {
                timer = 0;
                if (isCursorOnLeftPanel)
                {
                    Debug.Log("Cursor is on the LEFT panel.");
                    Triangle.TurnLeft();
                }
                else if (isCursorOnRightPanel)
                {
                    Debug.Log("Cursor is on the RIGHT panel.");
                    Triangle.TurnRight();
                }
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            timer = 0;
            if (isCursorOnLeftPanel)
            {
                Debug.Log("Clicked on the LEFT panel.");
                Triangle.TurnLeft();
            }
            else if (isCursorOnRightPanel)
            {
                Debug.Log("Clicked on the RIGHT panel.");
                Triangle.TurnRight();
            }
        }
    }
}
