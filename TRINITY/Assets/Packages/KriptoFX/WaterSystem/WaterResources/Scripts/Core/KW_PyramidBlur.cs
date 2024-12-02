using System;
using UnityEngine;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;

namespace KWS
{
    internal class KW_PyramidBlur
    {
        private string BlurShaderName = "Hidden/KriptoFX/KWS/BlurGaussian";
        const int kMaxIterations = 8;
        Material _blurMaterial;

        RTHandle[] tempBuffersDown = new RTHandle[kMaxIterations];
        RTHandle[] tempBuffersUp = new RTHandle[kMaxIterations];

        private RTHandle _tempSeparableRT;

        private readonly int _sampleScale = Shader.PropertyToID("_SampleScale");
        ScaleFunc[] _targetScaleFuncArr;

        enum RtHandleScaleEnum
        {
            Func,
            Scale,
            FixedSize
        }

        public void Release()
        {
            KW_Extensions.SafeDestroy(_blurMaterial);
            for (var i = 0; i < tempBuffersDown.Length; i++)
            {
                if (tempBuffersDown[i] != null) tempBuffersDown[i].Release();
                tempBuffersDown[i] = null;
            }

            for (var i = 0; i < tempBuffersUp.Length; i++)
            {
                if (tempBuffersUp[i] != null) tempBuffersUp[i].Release();
                tempBuffersUp[i] = null;
            }
        }

        void InitalizeScaleFuncArray(ScaleFunc sourceScaleFunc)
        {
            _targetScaleFuncArr = new ScaleFunc[kMaxIterations];

            var currentDivider = 2;
            for (int i = 0; i < kMaxIterations; i++)
            {
                var divider = currentDivider;
                _targetScaleFuncArr[i] =  (screenSize) => sourceScaleFunc(screenSize) / divider;
                currentDivider         *= 2;
            }


        }

        public void ComputeBlurPyramid(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, int width, int height)
        {
            ComputeBlurPyramid(blurRadius, source, target, cmd, RtHandleScaleEnum.FixedSize, constantWidth: width, constantHeight: height);
        }

        public void ComputeBlurPyramid(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, Vector2 sourceScale)
        {
            ComputeBlurPyramid(blurRadius, source, target, cmd, RtHandleScaleEnum.Scale, sourceScale);
        }

        public void ComputeBlurPyramid(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, ScaleFunc sourceScaleFunc)
        {
            ComputeBlurPyramid(blurRadius, source, target, cmd, RtHandleScaleEnum.Func, sourceScaleFunc: sourceScaleFunc);
        }

        void ComputeBlurPyramid(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, RtHandleScaleEnum scaleType, Vector2 sourceScale = default, ScaleFunc sourceScaleFunc = null, int constantWidth=-1, int constantHeight =-1)
        {
            if (_blurMaterial == null) _blurMaterial = CreateMaterial(BlurShaderName);

            var targetRT = target.rt;
            RTHandle last = source;

            var scaledViewportSize = source.GetScaledSize(source.rtHandleProperties.currentViewportSize);
            var logh = Mathf.Log(Mathf.Max(scaledViewportSize.x, scaledViewportSize.y), 2) + blurRadius - 8;
            var logh_i = (int)logh;
            var iterations = Mathf.Clamp(logh_i, 2, kMaxIterations);
            if (_targetScaleFuncArr == null) InitalizeScaleFuncArray(sourceScaleFunc);

            cmd.SetGlobalFloat(_sampleScale, 0.5f + logh - logh_i);
           
            for (var level = 0; level < iterations; level++)
            {
                sourceScale *= 0.5f;
                if (tempBuffersDown[level] == null)
                {

                    switch (scaleType)
                    {
                        case RtHandleScaleEnum.Func:
                            tempBuffersDown[level] = KWS_CoreUtils.RTHandleAllocVR(_targetScaleFuncArr[level], colorFormat: targetRT.graphicsFormat);
                            break;
                        case RtHandleScaleEnum.Scale:
                            tempBuffersDown[level] = KWS_CoreUtils.RTHandleAllocVR(sourceScale, colorFormat: targetRT.graphicsFormat);
                            break;
                        case RtHandleScaleEnum.FixedSize:
                            tempBuffersDown[level] = KWS_CoreUtils.RTHandleAllocVR(constantWidth, constantHeight, colorFormat: targetRT.graphicsFormat);
                            break;
                      
                    }
                }
                var downPassTarget = iterations == 1 ? target : tempBuffersDown[level];

                var currentTarget = level == 0 ? source : last;
                cmd.BlitTriangleRTHandle(currentTarget, downPassTarget, _blurMaterial, ClearFlag.None, Color.clear);

                last = tempBuffersDown[level];
            }

            for (var level = iterations - 1; level >= 0; level--)
            {
                if (tempBuffersUp[level] == null && level != 0)
                {
                    switch (scaleType)
                    {
                        case RtHandleScaleEnum.Func:
                            tempBuffersUp[level] = KWS_CoreUtils.RTHandleAllocVR(_targetScaleFuncArr[level - 1], colorFormat: targetRT.graphicsFormat);
                            break;
                        case RtHandleScaleEnum.Scale:
                            tempBuffersUp[level] = KWS_CoreUtils.RTHandleAllocVR(tempBuffersDown[level].scaleFactor * 2f, colorFormat: targetRT.graphicsFormat);
                            break;
                        case RtHandleScaleEnum.FixedSize:
                            tempBuffersUp[level] = KWS_CoreUtils.RTHandleAllocVR(tempBuffersDown[level].rt.width * 2, tempBuffersDown[level].rt.height * 2, colorFormat: targetRT.graphicsFormat);
                            break;

                    }
                }

                if (level == 0) cmd.BlitTriangleRTHandle(last, target, _blurMaterial, ClearFlag.None, Color.clear, 1);
                else cmd.BlitTriangleRTHandle(last, tempBuffersUp[level], _blurMaterial, ClearFlag.None, Color.clear, 1);
                last = tempBuffersUp[level];
            }
        }

        public void ComputeSeparableBlur(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, int width, int height)
        {
            ComputeSeparableBlur(blurRadius, source, target, cmd, RtHandleScaleEnum.FixedSize, constantWidth: width, constantHeight: height);
        }

        public void ComputeSeparableBlur(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, Vector2 sourceScale)
        {
            ComputeSeparableBlur(blurRadius, source, target, cmd, RtHandleScaleEnum.Scale, sourceScale);
        }

        public void ComputeSeparableBlur(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, ScaleFunc sourceScaleFunc)
        {
            ComputeSeparableBlur(blurRadius, source, target, cmd, RtHandleScaleEnum.Func, sourceScaleFunc : sourceScaleFunc);
        }


        void ComputeSeparableBlur(float blurRadius, RTHandle source, RTHandle target, CommandBuffer cmd, RtHandleScaleEnum scaleType, Vector2 sourceScale = default, ScaleFunc sourceScaleFunc = null, int constantWidth = -1, int constantHeight = -1)
        {
            if (_tempSeparableRT == null)
            {
                switch (scaleType)
                {
                    case RtHandleScaleEnum.Func:
                        _tempSeparableRT = KWS_CoreUtils.RTHandleAllocVR(sourceScaleFunc, colorFormat: target.rt.graphicsFormat);
                            break;
                    case RtHandleScaleEnum.Scale:
                        _tempSeparableRT = KWS_CoreUtils.RTHandleAllocVR(sourceScale, colorFormat: target.rt.graphicsFormat);
                                break;
                    case RtHandleScaleEnum.FixedSize:
                        _tempSeparableRT = KWS_CoreUtils.RTHandleAllocVR(constantWidth, constantHeight, colorFormat: target.rt.graphicsFormat);
                                break;
                }
            }
            if (_blurMaterial == null) _blurMaterial = CreateMaterial(BlurShaderName);
            
            cmd.SetGlobalFloat(_sampleScale, blurRadius);

            var pass = blurRadius < 1.5f ? 2 : 4;
            cmd.BlitTriangleRTHandle(source, _tempSeparableRT, _blurMaterial, ClearFlag.None, Color.clear, pass);
            cmd.BlitTriangleRTHandle(_tempSeparableRT, target, _blurMaterial, ClearFlag.None, Color.clear, pass + 1);
        }
    }
}