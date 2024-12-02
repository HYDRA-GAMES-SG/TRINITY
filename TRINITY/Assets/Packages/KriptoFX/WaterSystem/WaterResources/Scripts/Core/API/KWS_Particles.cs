using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{


    [ExecuteAlways]
    [RequireComponent(typeof(ParticleSystem))]
    public class KWS_Particles : MonoBehaviour
    {
        public ParticlesModeEnum ParticlesMode = ParticlesModeEnum.Default;

        public enum ParticlesModeEnum
        {
            Default,
            Decal,
            Infinite
        }

        private ParticleSystemRenderer[] _psRenderers;
        private Transform              _t;

        private int   posID   = Shader.PropertyToID("KWS_ParticlesPos");
        private int   scaleID = Shader.PropertyToID("KWS_ParticlesScale");
        private float _waterLevel;

        void OnEnable()
        {
            _psRenderers = GetComponentsInChildren<ParticleSystemRenderer>();
            _t           = transform;


            if (ParticlesMode == ParticlesModeEnum.Infinite && Application.isPlaying)
            {
                if (QualitySettings.renderPipeline == null)
                {
                    Camera.onPreRender += OnPreRenderOld;
                }
                else RenderPipelineManager.beginCameraRendering += OnPreRenderSRP;
            }
        }



        void OnDisable()
        {
            if (ParticlesMode == ParticlesModeEnum.Infinite && Application.isPlaying)
            {
                if (QualitySettings.renderPipeline == null)
                {
                    Camera.onPreRender -= OnPreRenderOld;
                }
                else RenderPipelineManager.beginCameraRendering -= OnPreRenderSRP;
            }
        }

        private void OnPreRenderOld(Camera cam)
        {
            UpdateInfiniteParticles(cam);
        }

        private void OnPreRenderSRP(ScriptableRenderContext ctx, Camera cam)
        {
            UpdateInfiniteParticles(cam);
        }

        void UpdateInfiniteParticles(Camera cam)
        {
            if (Application.isPlaying)
            {
                var camType = cam.cameraType;
                if (camType == CameraType.Game || camType == CameraType.VR)
                {
                    var camT       = cam.transform;
                    var camPos     = camT.position;
                    var camForward = camT.forward;
                    camPos += Vector3.Scale(camForward, _t.localScale * 0.45f);
                    _t.position = new Vector3(camPos.x, _waterLevel, camPos.z);
                    UpdateShaderParams();
                }
            }
        }

        void Update()
        {
            var pos = _t.position;
            _waterLevel = WaterSystem.FindWaterLevelAtLocation(pos);

            if (ParticlesMode is ParticlesModeEnum.Decal or ParticlesModeEnum.Infinite)
            {
                pos.y       = _waterLevel;
                _t.position = pos;
                _t.rotation = Quaternion.identity;
            } 

            UpdateShaderParams();
        }

        private void UpdateShaderParams()
        {
            var pos   = _t.position;
            pos   = new Vector3(pos.x, _waterLevel, pos.z);
            var scale = _t.localScale;

            foreach (var render in _psRenderers)
            {
                var mat = render.sharedMaterial;
                mat.SetVector(posID,   pos);
                mat.SetVector(scaleID, scale);
            }
           
        }
    }
}
