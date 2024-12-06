// <copyright project="ThirdPersonCamera" file="DebugVisualizationHelper.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using UnityEngine;

namespace ThirdPersonCamera
{
    public static class DebugVisualizationHelper
    {
        public static void DrawArrow(Vector3 start, Vector3 end, Color color, float arrowHeadLength = 0.1f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawLine(start, end, color);

            Vector3 right = Quaternion.LookRotation(end - start) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(end - start) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawLine(end, end + right * arrowHeadLength, color);
            Debug.DrawLine(end, end + left * arrowHeadLength, color);
        }

        public static void DrawDirectionArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.1f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawLine(pos, pos + direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawLine(pos + direction, pos + direction + right * arrowHeadLength, color);
            Debug.DrawLine(pos + direction, pos + direction + left * arrowHeadLength, color);
        }

        public static void DrawGizmoArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            var tmpColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);

            Gizmos.color = tmpColor;
        }

        public static void DrawBoxLines(Transform transform, Vector3 p1, Vector3 p2, Vector3 extents, bool boxes = false)
        {
            var halfExtents = extents / 2;
            var halfExtentsZ = transform.forward * halfExtents.z;
            var halfExtentsY = transform.up * halfExtents.y;
            var halfExtentsX = transform.right * halfExtents.x;

            if (boxes)
            {
                var matrix = Gizmos.matrix;
                Gizmos.color = Color.red;
                Gizmos.matrix = Matrix4x4.TRS(p1, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, extents);

                Gizmos.color = Color.green;
                Gizmos.matrix = Matrix4x4.TRS(p2, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, extents);
                Gizmos.matrix = matrix;
                Gizmos.color = Color.white;
            }

            // draw connect lines 1
            Gizmos.DrawLine(p1 - halfExtentsX - halfExtentsY - halfExtentsZ, p2 - halfExtentsX - halfExtentsY - halfExtentsZ);
            Gizmos.DrawLine(p1 + halfExtentsX - halfExtentsY - halfExtentsZ, p2 + halfExtentsX - halfExtentsY - halfExtentsZ);
            Gizmos.DrawLine(p1 - halfExtentsX + halfExtentsY - halfExtentsZ, p2 - halfExtentsX + halfExtentsY - halfExtentsZ);
            Gizmos.DrawLine(p1 + halfExtentsX + halfExtentsY - halfExtentsZ, p2 + halfExtentsX + halfExtentsY - halfExtentsZ);

            // draw connect lines 2
            Gizmos.DrawLine(p1 - halfExtentsX - halfExtentsY + halfExtentsZ, p2 - halfExtentsX - halfExtentsY + halfExtentsZ);
            Gizmos.DrawLine(p1 + halfExtentsX - halfExtentsY + halfExtentsZ, p2 + halfExtentsX - halfExtentsY + halfExtentsZ);
            Gizmos.DrawLine(p1 - halfExtentsX + halfExtentsY + halfExtentsZ, p2 - halfExtentsX + halfExtentsY + halfExtentsZ);
            Gizmos.DrawLine(p1 + halfExtentsX + halfExtentsY + halfExtentsZ, p2 + halfExtentsX + halfExtentsY + halfExtentsZ);
        }
    }
}