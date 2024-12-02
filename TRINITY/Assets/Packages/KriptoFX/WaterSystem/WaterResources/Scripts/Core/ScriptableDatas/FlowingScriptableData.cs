using System;
using UnityEngine;

namespace KWS
{
    [Serializable]
    public class FlowingScriptableData : ScriptableObject
    {
        [SerializeField] public int AreaSize;
        [SerializeField] public Vector3 AreaPosition;

        [SerializeField] public int FlowmapResolution;

        [SerializeField] public Texture2D FlowmapTexture;


        [SerializeField] public Texture2D FluidsMaskTexture;
        [SerializeField] public Texture2D FluidsPrebakedTexture;
    }
}