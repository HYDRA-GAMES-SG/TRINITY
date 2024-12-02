using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class KWS_Decal : MonoBehaviour
    {
        private MeshFilter _filter;
        private Transform  _t;
        private Vector3    _lastScale;
        private Mesh       _mesh;

        private const float VerticesPerMeter = 2f;
        private const float MaxResolution    = 64;

        void OnEnable()
        {
            _filter    = GetComponent<MeshFilter>();
            _t         = transform;
            _lastScale = _t.lossyScale;

            UpdateScale();
            UpdateMesh();

#if UNITY_EDITOR
            _decalTRS = Matrix4x4.TRS(_t.position, _t.rotation, new Vector3(_lastScale.x, 1, _lastScale.z));
#endif
        }


        void OnDisable()
        {

        }

        void UpdateMesh()
        {
            if (_mesh != null) KW_Extensions.SafeDestroy(_mesh);

            var vertScalePerMeter = VerticesPerMeter;
            var maxScale          = Mathf.Max(_lastScale.x, _lastScale.z);
            if (maxScale      < 5) vertScalePerMeter  *= 2;
            else if (maxScale > 20) vertScalePerMeter *= 0.5f;

            var res = (int)Mathf.Clamp(maxScale * vertScalePerMeter, 1, MaxResolution);
            _mesh = MeshUtils.CreatePlaneXZMesh(res, 1);

            _filter.sharedMesh = _mesh;
        }

        void UpdateScale()
        {
            var localScale = _t.localScale;
            localScale.x  = Mathf.Max(0.1f, localScale.x);
            localScale.z  = Mathf.Max(0.1f, localScale.z);
            localScale.y  = 100;
            _t.localScale = localScale;

            _lastScale = _t.lossyScale;
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying && _t.hasChanged)
            {
                _t.hasChanged = false;

                var pos = _t.position;
                pos.y = WaterSystem.FindWaterLevelAtLocation(pos);
                _t.position = pos;

                _t.rotation = Quaternion.Euler(0, _t.eulerAngles.y, 0);

                if (Vector3.Magnitude(_lastScale - _t.lossyScale) > 1)
                {
                    UpdateScale();
                    UpdateMesh();
                }
            }
        }

        Color _defaultDecalColor = new Color(0.25f, 0.5f, 0.95f, 0.99f);
        Matrix4x4 _decalTRS;
        void OnDrawGizmos()
        {
            Gizmos.color  = _defaultDecalColor; //by some reason cached public variable doesnt work with alpha
            Gizmos.matrix = _decalTRS;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        void OnDrawGizmosSelected()
        {
            if (_mesh == null) return;

            Gizmos.color = new Color(0.25f, 0.5f, 0.95f, 0.05f);

            _decalTRS     = Matrix4x4.TRS(_t.position, _t.rotation, new Vector3(_lastScale.x, 1, _lastScale.z));
            Gizmos.matrix = _decalTRS;
            Gizmos.DrawWireMesh(_mesh, 0);
        }
#endif
    }
}