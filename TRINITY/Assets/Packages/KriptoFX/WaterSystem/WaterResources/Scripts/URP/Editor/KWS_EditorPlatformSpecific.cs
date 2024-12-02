#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace KWS
{
    internal partial class KWS_Editor : Editor
    {
        void CheckPlatformSpecificMessages()
        {
            CheckPlatformSpecificMessages_Reflection();
        }

        void CheckPlatformSpecificMessages_VolumeLight()
        {
            
        }

        void CheckPlatformSpecificMessages_Reflection()
        {
            if (ReflectionProbe.defaultTexture.width == 1 && _waterInstance.Settings.OverrideSkyColor == false)
            {
                EditorGUILayout.HelpBox("Sky reflection doesn't work in this scene, you need to generate scene lighting! " + Environment.NewLine +
                                        "Open the \"Lighting\" window -> select the Generate Lighting option Reflection Probes", MessageType.Error);
            }
        }
    }

}
#endif