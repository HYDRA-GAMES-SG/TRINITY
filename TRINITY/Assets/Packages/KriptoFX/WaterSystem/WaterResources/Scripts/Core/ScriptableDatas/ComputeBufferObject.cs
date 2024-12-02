using System.Runtime.InteropServices;
using UnityEngine;

namespace KWS
{
    internal class ComputeBufferObject : MonoBehaviour
    {
        [HideInInspector] [SerializeField] public int PackedSize = 0;

        [HideInInspector] [SerializeField] public uint[] ComputeBufferDataUint1;
        [HideInInspector] [SerializeField] public KWS_CoreUtils.Uint2[] ComputeBufferDataUint2;
        [HideInInspector] [SerializeField] public KWS_CoreUtils.Uint3[] ComputeBufferDataUint3;
        [HideInInspector] [SerializeField] public KWS_CoreUtils.Uint4[] ComputeBufferDataUint4;

        public ComputeBuffer GetComputeBuffer()
        {
            ComputeBuffer buffer = null;
            switch (PackedSize)
            {
                case 1: 
                    buffer = new ComputeBuffer(ComputeBufferDataUint1.Length, Marshal.SizeOf(typeof(uint)));
                    buffer.SetData(ComputeBufferDataUint1);
                    break;
                case 2: 
                    buffer = new ComputeBuffer(ComputeBufferDataUint2.Length, Marshal.SizeOf(typeof(KWS_CoreUtils.Uint2)));
                    buffer.SetData(ComputeBufferDataUint2);
                    break;
                case 3:
                    buffer = new ComputeBuffer(ComputeBufferDataUint3.Length, Marshal.SizeOf(typeof(KWS_CoreUtils.Uint3)));
                    buffer.SetData(ComputeBufferDataUint3);
                    break;
                case 4:
                    buffer = new ComputeBuffer(ComputeBufferDataUint4.Length, Marshal.SizeOf(typeof(KWS_CoreUtils.Uint4)));
                    buffer.SetData(ComputeBufferDataUint4);
                    break;
            }

            return buffer;
        }
    }

}