Shader "Hidden/KriptoFX/KWS/DynamicWaves"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "" { }
	}

	HLSLINCLUDE

	#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

	struct vertexInputMaskMesh
	{
		float4 vertex : POSITION;
		float3 uv : TEXCOORD0;
	};

	struct v2fMaskMesh
	{
		float4 pos : SV_POSITION;
		float worldHeight : TEXCOORD0;
	};

	struct v2fMaskProcedural
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		uint instanceID : TEXCOORD1;
	};

	Texture2D KWS_PreviousTarget;
	Texture2D KWS_CurrentTarget;
	float4 KWS_CurrentTarget_TexelSize;

	float3 KW_AreaOffset;
	float3 KW_LastAreaOffset;
	float KW_InteractiveWavesPixelSpeed;

	float KWS_DynamicWavesRainStrength;
	float KWS_DynamicWavesWaterSurfaceHeight;
	float KWS_DynamicWavesForce;
	float KWS_MeshIntersectionThreshold;
	float KWS_DynamicWavesGlobalForceScale;

	static const float2 QuadIndex[6] =
	{
		float2(-0.5, -0.5),
		float2(-0.5, 0.5),
		float2(0.5, 0.5),
		float2(0.5, 0.5),
		float2(0.5, -0.5),
		float2(-0.5, -0.5)
	};

	inline float PackMask(float x)
	{
		return x;
		return (x * 0.5 + 0.5) + 0.001;
	}

	inline float UnpackMask(float x)
	{
		return x;
		return (x - 0.001) * 2 - 1;
	}

	v2fMaskMesh vertMaskMesh(vertexInputMaskMesh v)
	{
		v2fMaskMesh o;
		o.pos = ObjectToClipPosOrtho(v.vertex);
		o.worldHeight = LocalToWorldPos(v.vertex.xyz).y;
		return o;
	}

	v2fMaskProcedural vertMaskProcedural(uint instanceID : SV_InstanceID, uint vertexID : SV_VertexID)
	{
		v2fMaskProcedural o;
		KWS_DynamicWavesMask data = KWS_DynamicWavesMaskBuffer[instanceID];

		float halfSize = DynamicWavesMaskPos.w * 0.5;
		float2 quadOffset = (data.position.xz - DynamicWavesMaskPos.xz) / halfSize;
		quadOffset.y = -quadOffset.y;
		float2 quadIndex = QuadIndex[vertexID];

		o.pos.xy = quadIndex * data.size / halfSize + quadOffset;
		o.pos.z = 0;
		o.pos.w = 1;

		o.uv = quadIndex;
		o.instanceID = instanceID;

		return o;
	}

	half4 fragDrawMesh(v2fMaskMesh i) : SV_Target
	{
		float diff = (KWS_DynamicWavesWaterSurfaceHeight - i.worldHeight);
		if (abs(diff) < KWS_MeshIntersectionThreshold) return PackMask(KWS_DynamicWavesForce);
		else return 0;
	}


	half4 fragDrawProcedural(v2fMaskProcedural i) : SV_Target
	{
		
		half alphaMask = 1 - length(i.uv * 2);
		if (alphaMask < 0.0001) return 0;

		KWS_DynamicWavesMask data = KWS_DynamicWavesMaskBuffer[i.instanceID];
		float halfSize = data.size * 0.5;
		float intersectionMask = saturate(abs(data.waterHeight - data.position.y) / halfSize);
		if (alphaMask < intersectionMask * intersectionMask * intersectionMask) return 0;
		
		return PackMask(data.force);
	}

	
	struct vertexInput
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		uint vertexID : SV_VertexID;
	};
	

	struct v2f
	{
		float4 vertex : SV_POSITION;
		//float3 worldPos : TEXCOORD0;
		float2 uv : TEXCOORD1;
	};
	
	v2f vert(vertexInput v)
	{
		v2f o;
		
		o.vertex = GetTriangleVertexPosition(v.vertexID);
		o.uv = GetTriangleUVScaled(v.vertexID);
		//float2 worldUV = o.uv  * KW_DynamicWavesAreaSize - KW_DynamicWavesAreaSize * 0.5;
		//o.worldPos = float3(worldUV.x, 0, worldUV.y) + KW_DynamicWavesWorldPos;
		return o;
	}

	struct FragmentOutput
	{
		half dest0 : SV_Target0;
		half2 dest1 : SV_Target1;
	};

	float RainNoise(float2 p, float threshold)
	{
		p = p * 9757.0 + frac(KW_Time);
		float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
		p3 += dot(p3, p3.yxz + 33.33);
		float3 noise = frac((p3.xxy + p3.yzz) * p3.zyx);
		return (noise.x * noise.y * noise.z) > threshold;
	}

	FragmentOutput frag(v2f i) : SV_Target
	{
		FragmentOutput o;
		
		UNITY_BRANCH
		if (IsOutsideUV(i.uv))
		{
			o.dest0 = 0.0;
			o.dest1 = 0.5;
			return o;
		}

		float3 size = float3(KWS_CurrentTarget_TexelSize.x, -KWS_CurrentTarget_TexelSize.y, 0) * KW_InteractiveWavesPixelSpeed;
		
		float prevFrame = KWS_PreviousTarget.SampleLevel(sampler_linear_clamp, i.uv + KW_LastAreaOffset.xz, 0).x;

		float right = KWS_CurrentTarget.SampleLevel(sampler_linear_clamp, i.uv + size.xz + KW_AreaOffset.xz, 0).x;
		float left = KWS_CurrentTarget.SampleLevel(sampler_linear_clamp, i.uv + size.yz + KW_AreaOffset.xz, 0).x;
		float top = KWS_CurrentTarget.SampleLevel(sampler_linear_clamp, i.uv + size.zx + KW_AreaOffset.xz, 0).x;
		float down = KWS_CurrentTarget.SampleLevel(sampler_linear_clamp, i.uv + size.zy + KW_AreaOffset.xz, 0).x;

		float mask = UnpackMask(DynamicWavesMaskRT.SampleLevel(sampler_linear_clamp, i.uv, 0).x);
		
		//float2 depthUV = (i.worldPos.xz - KW_DepthPos.xz) / KW_DepthOrthographicSize + 0.5;
		//float depth = tex2D(KW_OrthoDepth, depthUV) * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y;

		float data = mask * KWS_DynamicWavesGlobalForceScale;

		#if KW_USE_RAIN_EFFECT
			float rainThreshold = lerp(0.995, 0.92, KWS_DynamicWavesRainStrength);
			float rainStr = lerp(0.05, 0.25, KWS_DynamicWavesRainStrength);
			data -= rainStr * RainNoise(i.uv, rainThreshold);
		#endif
		
		data += (right + left + top + down) * 0.5 - prevFrame;
		data *= 0.994;

		data = clamp(data, -10, 10);

		//data = data * 0.5 + 0.5;
		o.dest0 = data;
		//float2 dxy = prevFrame - float2(right, top);
		//o.dest1 = normalize(float3(dxy, 1.0)).xy * 0.5 + 0.5;
		o.dest1 = (20 * float2(right - left, top - down)) * 0.5 + 0.5;

		float2 maskAlphaUV = 1.0 - abs(i.uv * 2 - 1);
		float maskAlpha = saturate((maskAlphaUV.x * maskAlphaUV.y - 0.001) * 3);
		o.dest1 = lerp(half2(0.5, 0.5), o.dest1, maskAlpha);

		
		return o;
	}


	ENDHLSL

	Subshader
	{

		//draw mesh mask
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Blend One One

			HLSLPROGRAM
			#pragma vertex vertMaskMesh
			#pragma fragment fragDrawMesh
			ENDHLSL
		}

		//draw procedural mask
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Blend One One

			HLSLPROGRAM
			#pragma vertex vertMaskProcedural
			#pragma fragment fragDrawProcedural
			ENDHLSL
		}

		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ KW_USE_RAIN_EFFECT
			ENDHLSL
		}
	}
}