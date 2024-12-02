using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    //I can't record non "Unity.Objects" so I use this provider with saves links to data
    internal class UndoProvider : MonoBehaviour
    {
        [SerializeField] public List<ShorelineWavesScriptableData.ShorelineWave> ShorelineWaves;

        [SerializeField] public List<SplineScriptableData.Spline> Splines;

    }
}