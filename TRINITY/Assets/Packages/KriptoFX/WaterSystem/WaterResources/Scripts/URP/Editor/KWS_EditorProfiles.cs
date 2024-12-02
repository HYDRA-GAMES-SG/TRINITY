using System;
using System.Collections.Generic;
using static KWS.WaterSystemScriptableData;

namespace KWS
{
    internal static class KWS_EditorProfiles
    {
        private const float tolerance = 0.001f;

        public interface IWaterPerfomanceProfile
        {
            WaterProfileEnum GetProfile(WaterSystem water);
            void SetProfile(WaterProfileEnum profile, WaterSystem water);
            void ReadDataFromProfile(WaterSystem water);
            void CheckDataChangesAnsSetCustomProfile(WaterSystem water);
        }

        public static class PerfomanceProfiles
        {
            public struct Reflection : IWaterPerfomanceProfile
            {
                static readonly Dictionary<WaterProfileEnum, PlanarReflectionResolutionQualityEnum> PlanarReflectionResolutionQuality = new Dictionary<WaterProfileEnum, PlanarReflectionResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, PlanarReflectionResolutionQualityEnum.Ultra},
                    {WaterProfileEnum.High, PlanarReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, PlanarReflectionResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, PlanarReflectionResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, PlanarReflectionResolutionQualityEnum.VeryLow},
                };

                static readonly Dictionary<WaterProfileEnum, ScreenSpaceReflectionResolutionQualityEnum> ScreenSpaceReflectionResolutionQuality = new Dictionary<WaterProfileEnum, ScreenSpaceReflectionResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, ScreenSpaceReflectionResolutionQualityEnum.Ultra},
                    {WaterProfileEnum.High, ScreenSpaceReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, ScreenSpaceReflectionResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, ScreenSpaceReflectionResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, ScreenSpaceReflectionResolutionQualityEnum.VeryLow},
                };

                static readonly Dictionary<WaterProfileEnum, CubemapReflectionResolutionQualityEnum> CubemapReflectionResolutionQuality = new Dictionary<WaterProfileEnum, CubemapReflectionResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, CubemapReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.High, CubemapReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, CubemapReflectionResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, CubemapReflectionResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, CubemapReflectionResolutionQualityEnum.Low},
                };

                static readonly Dictionary<WaterProfileEnum, float> CubemapUpdateInterval = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 10},
                    {WaterProfileEnum.High, 10},
                    {WaterProfileEnum.Medium, 10},
                    {WaterProfileEnum.Low, 30},
                    {WaterProfileEnum.PotatoPC, 60},
                };

                static readonly Dictionary<WaterProfileEnum, bool> UseAnisotropicReflections = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, true},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                static readonly Dictionary<WaterProfileEnum, bool> AnisotropicReflectionsHighQuality = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, false},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };


                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.ReflectionProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.ReflectionProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ReflectionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.PlanarReflectionResolutionQuality = PlanarReflectionResolutionQuality[currentProfile];
                        water.Settings.ScreenSpaceReflectionResolutionQuality = ScreenSpaceReflectionResolutionQuality[currentProfile];
                        water.Settings.UseAnisotropicReflections = UseAnisotropicReflections[currentProfile];
                        water.Settings.AnisotropicReflectionsHighQuality = AnisotropicReflectionsHighQuality[currentProfile];
                    }
                }


                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ReflectionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.PlanarReflectionResolutionQuality != PlanarReflectionResolutionQuality[currentProfile]) isChanged = true;
                        else if (water.Settings.ScreenSpaceReflectionResolutionQuality != ScreenSpaceReflectionResolutionQuality[currentProfile]) isChanged = true;
                        else if (water.Settings.UseAnisotropicReflections != UseAnisotropicReflections[currentProfile]) isChanged = true;
                        else if (water.Settings.AnisotropicReflectionsHighQuality != AnisotropicReflectionsHighQuality[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.ReflectionProfile = WaterProfileEnum.Custom;
                    }
                }

            }

            public struct ColorRerfraction : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, RefractionResolutionEnum> RefractionResolution = new Dictionary<WaterProfileEnum, RefractionResolutionEnum>()
                {
                    { WaterProfileEnum.Ultra, RefractionResolutionEnum.Full },
                    { WaterProfileEnum.High, RefractionResolutionEnum.Half },
                    { WaterProfileEnum.Medium, RefractionResolutionEnum.Half },
                    { WaterProfileEnum.Low, RefractionResolutionEnum.Half },
                    { WaterProfileEnum.PotatoPC, RefractionResolutionEnum.Quarter }
                };

                public static readonly Dictionary<WaterProfileEnum, RefractionModeEnum> RefractionMode = new Dictionary<WaterProfileEnum, RefractionModeEnum>
                {
                    {WaterProfileEnum.Ultra, RefractionModeEnum.PhysicalAproximationIOR},
                    {WaterProfileEnum.High, RefractionModeEnum.PhysicalAproximationIOR},
                    {WaterProfileEnum.Medium, RefractionModeEnum.PhysicalAproximationIOR},
                    {WaterProfileEnum.Low, RefractionModeEnum.Simple},
                    {WaterProfileEnum.PotatoPC, RefractionModeEnum.Simple}
                };

                public static readonly Dictionary<WaterProfileEnum, bool> UseRefractionDispersion = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.RefractionProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.RefractionProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RefractionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.RefractionResolution = RefractionResolution[currentProfile];
                        water.Settings.RefractionMode = RefractionMode[currentProfile];
                        water.Settings.UseRefractionDispersion = UseRefractionDispersion[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RefractionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.RefractionResolution != RefractionResolution[currentProfile]) isChanged = true;
                        else if (water.Settings.RefractionMode != RefractionMode[currentProfile]) isChanged = true;
                        else if (water.Settings.UseRefractionDispersion != UseRefractionDispersion[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.RefractionProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Flowing : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, FlowmapTextureResolutionEnum> FlowMapTextureResolution = new Dictionary<WaterProfileEnum, FlowmapTextureResolutionEnum>()
                {
                    {WaterProfileEnum.Ultra, FlowmapTextureResolutionEnum._4096},
                    {WaterProfileEnum.High, FlowmapTextureResolutionEnum._4096},
                    {WaterProfileEnum.Medium, FlowmapTextureResolutionEnum._2048},
                    {WaterProfileEnum.Low, FlowmapTextureResolutionEnum._1024},
                    {WaterProfileEnum.PotatoPC, FlowmapTextureResolutionEnum._512},
                };

                public static readonly Dictionary<WaterProfileEnum, int> FluidsSimulationIterrations = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 3},
                    {WaterProfileEnum.High, 2},
                    {WaterProfileEnum.Medium, 2},
                    {WaterProfileEnum.Low, 2},
                    {WaterProfileEnum.PotatoPC, 2},
                };

                public static readonly Dictionary<WaterProfileEnum, int> FluidsTextureSize = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 2048},
                    {WaterProfileEnum.High, 1536},
                    {WaterProfileEnum.Medium, 1024},
                    {WaterProfileEnum.Low, 768},
                    {WaterProfileEnum.PotatoPC, 512},
                };

                public static readonly Dictionary<WaterProfileEnum, int> FluidsAreaSize = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 45},
                    {WaterProfileEnum.High, 35},
                    {WaterProfileEnum.Medium, 25},
                    {WaterProfileEnum.Low, 20},
                    {WaterProfileEnum.PotatoPC, 15},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.FlowmapProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.FlowmapProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.FlowmapProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.FlowMapTextureResolution = FlowMapTextureResolution[currentProfile];
                        water.Settings.FluidsSimulationIterrations = FluidsSimulationIterrations[currentProfile];
                        water.Settings.FluidsTextureSize = FluidsTextureSize[currentProfile];
                        water.Settings.FluidsAreaSize = FluidsAreaSize[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.FlowmapProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.FlowMapTextureResolution != FlowMapTextureResolution[currentProfile]) isChanged = true;
                        else if (water.Settings.FluidsSimulationIterrations != FluidsSimulationIterrations[currentProfile]) isChanged = true;
                        else if (water.Settings.FluidsTextureSize != FluidsTextureSize[currentProfile]) isChanged = true;
                        else if (water.Settings.FluidsAreaSize != FluidsAreaSize[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.FlowmapProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct DynamicWaves : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, int> DynamicWavesAreaSize = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 60},
                    {WaterProfileEnum.High, 50},
                    {WaterProfileEnum.Medium, 40},
                    {WaterProfileEnum.Low, 30},
                    {WaterProfileEnum.PotatoPC, 20},
                };

                public static readonly Dictionary<WaterProfileEnum, int> DynamicWavesResolutionPerMeter = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 34},
                    {WaterProfileEnum.High, 34},
                    {WaterProfileEnum.Medium, 34},
                    {WaterProfileEnum.Low, 25},
                    {WaterProfileEnum.PotatoPC, 20},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.DynamicWavesProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.DynamicWavesProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.DynamicWavesProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.DynamicWavesAreaSize = DynamicWavesAreaSize[currentProfile];
                        water.Settings.DynamicWavesResolutionPerMeter = DynamicWavesResolutionPerMeter[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.DynamicWavesProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.DynamicWavesAreaSize != DynamicWavesAreaSize[currentProfile]) isChanged = true;
                        else if (water.Settings.DynamicWavesResolutionPerMeter != DynamicWavesResolutionPerMeter[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.DynamicWavesProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Shoreline : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, ShorelineFoamQualityEnum> FoamLodQuality = new Dictionary<WaterProfileEnum, ShorelineFoamQualityEnum>
                {
                    {WaterProfileEnum.Ultra, ShorelineFoamQualityEnum.High},
                    {WaterProfileEnum.High, ShorelineFoamQualityEnum.High},
                    {WaterProfileEnum.Medium, ShorelineFoamQualityEnum.Medium},
                    {WaterProfileEnum.Low, ShorelineFoamQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, ShorelineFoamQualityEnum.VeryLow},
                };

                public static readonly Dictionary<WaterProfileEnum, bool> UseFastMode = new Dictionary<WaterProfileEnum, bool>
                {
                    {WaterProfileEnum.Ultra, false},
                    {WaterProfileEnum.High, false},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, true},
                    {WaterProfileEnum.PotatoPC, true},
                };

                public static readonly Dictionary<WaterProfileEnum, bool> FoamReceiveShadows = new Dictionary<WaterProfileEnum, bool>
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, true},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.ShorelineProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.ShorelineProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.ShorelineFoamLodQuality = FoamLodQuality[currentProfile];
                        water.Settings.UseShorelineFoamFastMode = UseFastMode[currentProfile];
                        water.Settings.ShorelineFoamReceiveDirShadows = FoamReceiveShadows[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.ShorelineFoamLodQuality != FoamLodQuality[currentProfile]) isChanged = true;
                        else if (water.Settings.UseShorelineFoamFastMode != UseFastMode[currentProfile]) isChanged = true;
                        else if (water.Settings.ShorelineFoamReceiveDirShadows != FoamReceiveShadows[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.ShorelineProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Foam : IWaterPerfomanceProfile
            {
                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.ShorelineProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.ShorelineProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {

                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        //var isChanged = false;


                        //if (isChanged) water.Settings.ShorelineProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct VolumetricLight : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, VolumetricLightResolutionQualityEnum> VolumetricLightResolutionQuality = new Dictionary<WaterProfileEnum, VolumetricLightResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, VolumetricLightResolutionQualityEnum.Ultra},
                    {WaterProfileEnum.High, VolumetricLightResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, VolumetricLightResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, VolumetricLightResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, VolumetricLightResolutionQualityEnum.VeryLow},
                };

                public static readonly Dictionary<WaterProfileEnum, int> VolumetricLightIteration = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 8},
                    {WaterProfileEnum.High, 7},
                    {WaterProfileEnum.Medium, 6},
                    {WaterProfileEnum.Low, 8},
                    {WaterProfileEnum.PotatoPC, 8},
                };

                public static readonly Dictionary<WaterProfileEnum, float> VolumetricLightTemporalAccumulationFactor = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 0.3f},
                    {WaterProfileEnum.High, 0.35f},
                    {WaterProfileEnum.Medium, 0.4f},
                    {WaterProfileEnum.Low, 0.5f},
                    {WaterProfileEnum.PotatoPC, 0.6f},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.GetGlobalSettings().VolumetricLightProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.GetGlobalSettings().VolumetricLightProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var globalSettings = water.GetGlobalSettings();
                    var currentProfile = globalSettings.VolumetricLightProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        globalSettings.VolumetricLightResolutionQuality = VolumetricLightResolutionQuality[currentProfile];
                        globalSettings.VolumetricLightIteration = VolumetricLightIteration[currentProfile];
                        globalSettings.VolumetricLightTemporalReprojectionAccumulationFactor = VolumetricLightTemporalAccumulationFactor[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var globalSettings = water.GetGlobalSettings();
                    var currentProfile = globalSettings.VolumetricLightProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (globalSettings.VolumetricLightResolutionQuality != VolumetricLightResolutionQuality[currentProfile]) isChanged = true;
                        else if (globalSettings.VolumetricLightIteration != VolumetricLightIteration[currentProfile]) isChanged = true;
                        else if (Math.Abs(globalSettings.VolumetricLightTemporalReprojectionAccumulationFactor - VolumetricLightTemporalAccumulationFactor[currentProfile]) > tolerance) isChanged = true;

                        if (isChanged) globalSettings.VolumetricLightProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Caustic : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, bool> UseCausticHighQualityFiltering = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, false},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public static readonly Dictionary<WaterProfileEnum, bool> UseCausticDispersion = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public static readonly Dictionary<WaterProfileEnum, CausticTextureResolutionQualityEnum> CausticTextureResolution = new Dictionary<WaterProfileEnum, CausticTextureResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, CausticTextureResolutionQualityEnum.Ultra},
                    {WaterProfileEnum.High, CausticTextureResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, CausticTextureResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, CausticTextureResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, CausticTextureResolutionQualityEnum.Low},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.CausticProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.CausticProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.CausticProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        if (water.Settings.UseGlobalWind)
                        {
                            water.Settings.UseGlobalCausticHighQualityFiltering = UseCausticHighQualityFiltering[currentProfile];
                            water.Settings.GlobalCausticTextureResolutionQuality = CausticTextureResolution[currentProfile];
                        }
                        else
                        {
                            water.Settings.UseLocalCausticHighQualityFiltering = UseCausticHighQualityFiltering[currentProfile];
                            water.Settings.LocalCausticTextureResolutionQuality = CausticTextureResolution[currentProfile];
                        }

                        water.Settings.UseCausticDispersion = UseCausticDispersion[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var globalSettings = water.GetGlobalSettings();
                    var currentProfile = water.Settings.CausticProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.UseGlobalWind)
                        {
                            if (globalSettings.UseGlobalCausticHighQualityFiltering != UseCausticHighQualityFiltering[currentProfile]) isChanged = true;
                            else if (globalSettings.UseCausticDispersion != UseCausticDispersion[currentProfile]) isChanged = true;
                            else if (globalSettings.GlobalCausticTextureResolutionQuality != CausticTextureResolution[currentProfile]) isChanged = true;
                        }
                        else
                        {
                            if (globalSettings.UseLocalCausticHighQualityFiltering != UseCausticHighQualityFiltering[currentProfile]) isChanged = true;
                            else if (globalSettings.UseCausticDispersion != UseCausticDispersion[currentProfile]) isChanged = true;
                            else if (globalSettings.LocalCausticTextureResolutionQuality != CausticTextureResolution[currentProfile]) isChanged = true;
                        }



                        if (isChanged) globalSettings.CausticProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Mesh : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, WaterMeshQualityEnum> MeshQualityInfinite = new Dictionary<WaterProfileEnum, WaterMeshQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, WaterMeshQualityEnum.Ultra},
                    {WaterProfileEnum.High, WaterMeshQualityEnum.High},
                    {WaterProfileEnum.Medium, WaterMeshQualityEnum.Medium},
                    {WaterProfileEnum.Low, WaterMeshQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, WaterMeshQualityEnum.VeryLow},
                };

                //public static readonly Dictionary<WaterProfileEnum, int> OceanDetailingFarDistance = new Dictionary<WaterProfileEnum, int>()
                //{
                //    {WaterProfileEnum.Ultra, 5000},
                //    {WaterProfileEnum.High, 4000},
                //    {WaterProfileEnum.Medium, 3000},
                //    {WaterProfileEnum.Low, 2000},
                //    {WaterProfileEnum.PotatoPC, 1000},
                //};

                public static readonly Dictionary<WaterProfileEnum, WaterMeshQualityEnum> MeshQualityFinite = new Dictionary<WaterProfileEnum, WaterMeshQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, WaterMeshQualityEnum.Ultra},
                    {WaterProfileEnum.High, WaterMeshQualityEnum.High},
                    {WaterProfileEnum.Medium, WaterMeshQualityEnum.Medium},
                    {WaterProfileEnum.Low, WaterMeshQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, WaterMeshQualityEnum.VeryLow},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationFactor = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 1.0f},
                    {WaterProfileEnum.High, 0.75f},
                    {WaterProfileEnum.Medium, 0.5f},
                    {WaterProfileEnum.Low, 0.25f},
                    {WaterProfileEnum.PotatoPC, 0.15f},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationMaxDistance_Infinite = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 5000},
                    {WaterProfileEnum.High, 2000},
                    {WaterProfileEnum.Medium, 1000},
                    {WaterProfileEnum.Low, 500},
                    {WaterProfileEnum.PotatoPC, 200},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationMaxDistance_Finite = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 300},
                    {WaterProfileEnum.High, 200},
                    {WaterProfileEnum.Medium, 100},
                    {WaterProfileEnum.Low, 50},
                    {WaterProfileEnum.PotatoPC, 10},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationMaxDistance_River = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 200},
                    {WaterProfileEnum.High, 100},
                    {WaterProfileEnum.Medium, 50},
                    {WaterProfileEnum.Low, 25},
                    {WaterProfileEnum.PotatoPC, 10},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationMaxDistance_Custom = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 200},
                    {WaterProfileEnum.High, 100},
                    {WaterProfileEnum.Medium, 50},
                    {WaterProfileEnum.Low, 25},
                    {WaterProfileEnum.PotatoPC, 10},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.MeshProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.MeshProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.MeshProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.WaterMeshQualityInfinite = MeshQualityInfinite[currentProfile];
                        // water.Settings.OceanDetailingFarDistance = OceanDetailingFarDistance[currentProfile];
                        water.Settings.WaterMeshQualityFinite = MeshQualityFinite[currentProfile];

                        water.Settings.TesselationFactor_Infinite = TesselationFactor[currentProfile];
                        water.Settings.TesselationFactor_Finite = TesselationFactor[currentProfile];
                        water.Settings.TesselationFactor_River = TesselationFactor[currentProfile];
                        water.Settings.TesselationFactor_Custom = TesselationFactor[currentProfile];

                        water.Settings.TesselationMaxDistance_Infinite = TesselationMaxDistance_Infinite[currentProfile];
                        water.Settings.TesselationFactor_Finite = TesselationMaxDistance_Finite[currentProfile];
                        water.Settings.TesselationFactor_River = TesselationMaxDistance_River[currentProfile];
                        water.Settings.TesselationFactor_Custom = TesselationMaxDistance_Custom[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.MeshProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.WaterMeshQualityInfinite != MeshQualityInfinite[currentProfile]) isChanged = true;
                        // else if (water.Settings.OceanDetailingFarDistance                                                                   != OceanDetailingFarDistance[currentProfile]) isChanged = true;
                        else if (water.Settings.WaterMeshQualityFinite != MeshQualityFinite[currentProfile]) isChanged = true;
                        else if (Math.Abs(water.Settings.CurrentTesselationFactor - TesselationFactor[currentProfile]) > tolerance) isChanged = true;
                        else if (Math.Abs(water.Settings.TesselationMaxDistance_Infinite - TesselationMaxDistance_Infinite[currentProfile]) > tolerance) isChanged = true;
                        else if (Math.Abs(water.Settings.TesselationMaxDistance_Finite - TesselationMaxDistance_Finite[currentProfile]) > tolerance) isChanged = true;
                        else if (Math.Abs(water.Settings.TesselationMaxDistance_River - TesselationMaxDistance_River[currentProfile]) > tolerance) isChanged = true;
                        else if (Math.Abs(water.Settings.TesselationMaxDistance_Custom - TesselationMaxDistance_Custom[currentProfile]) > tolerance) isChanged = true;

                        if (isChanged) water.Settings.MeshProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Rendering : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, bool> UseFiltering = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, true},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.RenderingProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.RenderingProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RenderingProfile;
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RenderingProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (isChanged) water.Settings.RenderingProfile = WaterProfileEnum.Custom;
                    }
                }
            }
        }
    }
}