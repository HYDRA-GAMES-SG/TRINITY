using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    internal enum ReadbackCode
    {
        UpToDate,
        Enqueued,
        Busy
    }

    public class AsyncTextureSynchronizer<T> where T : struct
    {
        // The pair of buffers that allows us to keep doing the async readback "permanently"
        NativeArray<T>[] m_InternalBuffers = new NativeArray<T>[2];

        // Tracker of the current "valid" buffer
        int m_CurrentBuffer = 1;


        // Is there any job on going right now?
        bool m_CurrentlyOnGoingJob = false;


        // Callback for the end of the async readback
        internal struct AsyncTextureSynchronizerCallBack
        {
            public AsyncTextureSynchronizer<T> ats;
            public void OnReceive(AsyncGPUReadbackRequest request)
            {
                if (!request.hasError)
                    ats.SwapCurrentBuffer();
                ats.m_CurrentlyOnGoingJob = false;
            }
        }
        AsyncTextureSynchronizerCallBack callback = new AsyncTextureSynchronizerCallBack();

        internal AsyncTextureSynchronizer()
        {
            callback.ats = this;
        }

        public NativeArray<T> CurrentBuffer()
        {
            return m_InternalBuffers[m_CurrentBuffer];
        }

        public bool IsCreated()
        {
            return m_InternalBuffers[m_CurrentBuffer].IsCreated;
        }


        public bool IsBusy()
        {
            return m_CurrentlyOnGoingJob;
        }

        void SwapCurrentBuffer()
        {
            m_CurrentBuffer = (m_CurrentBuffer + 1) % 2;
        }

        int NextBufferIndex()
        {
            return (m_CurrentBuffer + 1) % 2;
        }

        void ValidateNativeBuffer(ref NativeArray<T> buffer, int length)
        {
            if (!buffer.IsCreated || buffer.Length != length)
            {
                if (buffer.IsCreated)
                    buffer.Dispose();
                buffer = new NativeArray<T>(length, Allocator.Persistent);
            }
        }

        internal ReadbackCode EnqueueRequest(CommandBuffer cmd, ComputeBuffer buffer)
        {
            // A job is already going on, we need to wait before we do anything
            if (m_CurrentlyOnGoingJob)
                return ReadbackCode.Busy;

            // Ok we are now inside a read back job
            m_CurrentlyOnGoingJob = true;

            ValidateNativeBuffer(ref m_InternalBuffers[0], buffer.count);
            ValidateNativeBuffer(ref m_InternalBuffers[1], buffer.count);
            // Grab the next buffer
            NativeArray<T> nextBuffer = m_InternalBuffers[NextBufferIndex()];

            cmd.RequestAsyncReadbackIntoNativeArray(ref nextBuffer, buffer, callback.OnReceive);

            // Notify that we enqueued
            return ReadbackCode.Enqueued;
        }

        internal void ReleaseATSResources()
        {
            // If a job is still ongoing, we need to wait that it is done before we free the resources
            if (m_CurrentlyOnGoingJob) AsyncGPUReadback.WaitAllRequests();
            if (m_InternalBuffers[0].IsCreated) m_InternalBuffers[0].Dispose();
            if (m_InternalBuffers[1].IsCreated) m_InternalBuffers[1].Dispose();
        }
    }
}