using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace KWS
{
    internal class MotionVectorsPass : WaterPass
    {
        internal override string PassName => "Water.MotionVectorsPass";

        public MotionVectorsPass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
           
        }

        void InitializeTextures()
        {
           
            //this.WaterLog(WaterSharedResources.CausticRTArray);
        }

        void ReleaseTextures()
        {
           
            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }


        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            ReleaseTextures();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            //if (changedTabs.HasFlag(WaterSystem.WaterTab.Caustic))
            {
                
            }
        }

        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
           
        }

    }
}