// <copyright project="ThirdPersonCamera" file="MouseCursorHelper.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using UnityEngine;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

namespace ThirdPersonCamera
{
    public static class MouseCursorHelper
    {
        public static bool locked;
        
#if UNITY_STANDALONE_WIN
        [StructLayout(LayoutKind.Sequential)]
        private struct MousePoint
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);
        private static MousePoint cursorPoint;

        public static void Hide()
        {
            if (locked)
                return;
            
            GetCursorPos(out cursorPoint);

            Cursor.visible = false;
            locked = true;
        }

        public static void Show()
        {
            if (!locked) 
                return;
            
            Cursor.visible = true;
            locked = false;
        }

        public static void Update()
        {
            if (locked)
            {
                SetCursorPos(cursorPoint.x, cursorPoint.y);
            }
        }

        public static void Toggle()
        {
            if (locked)
                Show();
            else
                Hide();
        }
#else
        public static void Hide()
        {            
            if (Cursor.lockState == CursorLockMode.None)
            {
                locked = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public static void Show()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                locked = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        
        public static void Toggle()
        {
            if (locked)
                Show();
            else
                Hide();
        }
#endif    
    }
}
