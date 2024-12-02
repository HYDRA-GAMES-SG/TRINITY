using System;
using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    [Serializable]
    public class ShorelineWavesScriptableData : ScriptableObject
    {
        [SerializeField] public List<ShorelineWave> Waves = new List<ShorelineWave>();

        //Aim for structures with sizes divisible by 128 bits (sizeof float4)
        [Serializable]
        public struct ShorelineWave
        {
            [SerializeField] public Matrix4x4 WorldMatrix;

            [SerializeField] public int     WaveID;
            [SerializeField] public Vector3 Position;

            [SerializeField] public float   EulerRotationY;
            [SerializeField] public Vector3 Size;

            [SerializeField] public float   TimeOffset;
            [SerializeField] public Vector3 Scale;

            [SerializeField] public int     Flip;
            public                  Vector3 Pad;

         
            public ShorelineWave(int typeID, Vector3 pos, float rotationY, Vector3 scale, float timeOffset, bool flip)
            {
                WaveID         = typeID;
                Position       = pos;
                EulerRotationY = rotationY;
                Size           = WaveTypes[WaveID].Size;
                TimeOffset     = timeOffset;
                Scale          = scale;
                Flip           = flip ? 1 : 0;
                Pad            = Vector3.zero;
                var flippedScale = new Vector3(Scale.x, Scale.y, Scale.z * (Flip == 0 ? -1 : 1));
                WorldMatrix = Matrix4x4.TRS(Position, Quaternion.Euler(0, EulerRotationY, 0), Vector3.Scale(Size, flippedScale));
            }

            public void UpdateMatrix()
            {
                var flippedScale = new Vector3(Scale.x, Scale.y, Scale.z * (Flip == 0 ? -1 : 1));
                WorldMatrix = Matrix4x4.TRS(Position, Quaternion.Euler(0, EulerRotationY, 0), Vector3.Scale(Size, flippedScale));
            }

            public bool IsWaveInsideWorldArea(Vector3 minAreaPos, Vector3 maxAreaPos)
            {
                if (Position.x > minAreaPos.x && Position.x < maxAreaPos.x
                                              && Position.z > minAreaPos.z && Position.z < maxAreaPos.z) return true;

                return false;
            }

            public bool IsWaveVisible(ref Plane[] planes, ref Vector3[] corners)
            {
                var halfScale = Vector3.Scale(Size, Scale) * 0.5f;
                var minAABB   = new Vector3(Position.x - halfScale.x, Position.y - halfScale.y, Position.z - halfScale.z);
                var maxAABB   = new Vector3(Position.x + halfScale.x, Position.y + halfScale.y, Position.z + halfScale.z);
                return KW_Extensions.IsBoxVisibleAccurate(ref planes, ref corners, minAABB, maxAABB);
            }

            public float GetDistanceToCamera(Vector3 cameraPos)
            {
                return Vector3.Distance(cameraPos, Position);
            }

            public float GetWaveNormalizedAnimationTime(WaterSystem waterInstance)
            {
                const float waveFps = 6.0f;
                const float offsetMultiplier = 34.0f;
                const uint maxFrames = 70;

                var currentTime = KW_Extensions.TotalTime();
                currentTime *= waterInstance.Settings.GlobalTimeScale;
                currentTime += TimeOffset * offsetMultiplier;
                float interpolationTime = (waveFps * currentTime / maxFrames) % 1;
                return interpolationTime;
            }
        }

        internal class PrebakedWaveType
        {
            public Vector3 Size;
            //public float AnimationDuration;
            //public float MaxFrames;
        }

        internal static readonly Dictionary<int, PrebakedWaveType> WaveTypes = new Dictionary<int, PrebakedWaveType>()
        {
            {0, new PrebakedWaveType() {Size = new Vector3(14f, 4.5f, 16f)}}
        };

    }
}