Shader "Hidden/KriptoFX/KWS/FluidSimulation"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}

	HLSLINCLUDE

	#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

	Texture2D _SourceRT;
	float4 _SourceRT_TexelSize;

	float MouseClicked;
	float2 _MousePos;
	float _MouseSize;
	float2 _MouseDir;

	float4 Test;
	float4 Test2;

	float3 _CurrentPositionOffset;
	float3 _LastPositionOffset;
	float _AreaSize;
	float _FlowSpeed;
	sampler2D _FluidsNextLod;
	float4 _FluidsNextLod_TexelSize;
	float _FoamTexelOffset;
	float3 _CurrentFluidMapWorldPos;
	sampler2D KW_FluidsPrebaked;
	float KW_FluidsRequiredReadPrebakedSim;

	#define FLUIDS_REMAP_MIN float4(20, 20, 0, 10)
	#define FLUIDS_REMAP_DIFF float4(40, 40, 100, 20)

	struct v2fCompute
	{
		float2 uv : TEXCOORD0;
		float3 worldPos : TEXCOORD1;
		float4 vertex : SV_POSITION;
	};

	v2fCompute vertCompute(uint vertexID : SV_VertexID)
	{
		v2fCompute o;
		
		o.vertex = GetTriangleVertexPosition(vertexID);
		o.vertex.z = 0.5;
		o.uv = GetTriangleUVScaled(vertexID);
		
		float2 worldUV = o.uv * _AreaSize - _AreaSize * 0.5;
		o.worldPos = float3(worldUV.x, 0, worldUV.y) + _CurrentFluidMapWorldPos ;

		o.uv = o.uv + _CurrentPositionOffset.xz;
		return o;
	}



	struct FragmentOutput
	{
		half4 dest0 : SV_Target0;
		half4 dest1 : SV_Target1;
	};

	FragmentOutput fragCompute(v2fCompute i)
	{
		float Koef = 0.15;
		float v = 0.06;
		float dt = 0.13;

		float4 data = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv, 0);
		float4 tr = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv + float2(_SourceRT_TexelSize.x, 0), 0);
		float4 tl = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv - float2(_SourceRT_TexelSize.x, 0), 0);
		float4 tu = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv + float2(0, _SourceRT_TexelSize.y), 0);
		float4 td = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv - float2(0, _SourceRT_TexelSize.y), 0);

		float3 dx = (tr.xyz - tl.xyz) * 0.5;
		float3 dy = (tu.xyz - td.xyz) * 0.5;
		float2 densDif = float2(dx.z, dy.z);

		data.z -= _FlowSpeed * 0.5 * dt * dot(float3(densDif, dx.x + dy.y), data.xyz); //density

		float2 laplacian = tu.xy + td.xy + tr.xy + tl.xy - 4.0 * data.xy;
		float2 viscForce = v * laplacian;

		data.xyw = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv - 2.0 * dt * data.xy * _SourceRT_TexelSize.xy * _FlowSpeed, 0).xyw; //adfloattion

		float2 newForce = 0;
		float2 flowMapUV = (i.worldPos.xz - KW_FlowMapOffset.xz) / KW_FlowMapSize + 0.5;
		float2 flowmap = KW_FlowMapTex.SampleLevel(sampler_linear_clamp, flowMapUV, 0).xy * 2 - 1;
		//if( flowMapUV.x < 0.001 || flowMapUV.x > 0.999 || flowMapUV.y < 0.001 || flowMapUV.y > 0.999) flowmap = 0;

		newForce.xy += (flowmap) * 0.05 * _FlowSpeed;

		data.xy += _FlowSpeed * 1.5 * dt * (viscForce.xy - Koef / dt * densDif + newForce); //update velocity
		data.xy = max(0, abs(data.xy) - 1e-4) * sign(data.xy); //linear velocity decay

		data.w = (tr.y - tl.y - tu.x + td.x);
		float2 vort = float2(abs(tu.w) - abs(td.w), abs(tl.w) - abs(tr.w));
		vort *= _FlowSpeed * 0.13 / length(vort + 1e-9) * data.w;

		data.xy += vort;
		
		float depth = KW_FluidsDepthTex.SampleLevel(sampler_linear_clamp, flowMapUV, 0).r;
		
		data.xy *= depth > 0.01;
		data.xyz *= 0.9999;

		bool isOutArea = i.uv.x < 0.01 || i.uv.x > 0.99 || i.uv.y < 0.01 || i.uv.y > 0.99;

		#if CAN_USE_NEXT_LOD
			if (isOutArea)
			{
				float2 fluidsUV_lod1 = (i.worldPos.xz - KW_FluidsMapWorldPosition_lod1.xz) / KW_FluidsMapAreaSize_lod1 + 0.5;
				data = tex2D(_FluidsNextLod, fluidsUV_lod1);
			}
			if (KW_FluidsRequiredReadPrebakedSim) data.xyzw = tex2D(KW_FluidsPrebaked, flowMapUV).xyzw * FLUIDS_REMAP_DIFF - FLUIDS_REMAP_MIN;
		#else
			// if (isOutArea) data.xyzw = 0;
			if (isOutArea || KW_FluidsRequiredReadPrebakedSim)
			{
				data.xyzw = tex2D(KW_FluidsPrebaked, flowMapUV).xyzw * FLUIDS_REMAP_DIFF - FLUIDS_REMAP_MIN;
				if (flowMapUV.x < 0.05 || flowMapUV.x > 0.95 || flowMapUV.y < 0.05 || flowMapUV.y > 0.95) data.xyzw = 0;
			}

		#endif

		#if KW_FLUIDS_PREBAKE_SIM
			float2 maskAlphaUV = 1 - abs(flowMapUV * 2 - 1);
			float maskAlpha = saturate((maskAlphaUV.x * maskAlphaUV.y - 0.001) * 20);
			data *= maskAlpha;
		#endif


		data = clamp(data, float4(-20, -20, 0.5, -10), float4(20, 20, 100, 10));

		FragmentOutput o;
		o.dest0 = data;
		//o.dest0.xy = (tex2D(KW_FluidsPrebaked, flowMapUV).xyzw * FLUIDS_REMAP_DIFF - float4(20, 20, 0, 10)).xy;
		// o.dest0.xyz = frac(flowmap.xyy);

		float3 texelOffset = float3(_SourceRT_TexelSize.xy * _FoamTexelOffset, 0);


		tr = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv + texelOffset.xz, 0);
		tl = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv - texelOffset.xz, 0);
		tu = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv + texelOffset.zy, 0);
		td = _SourceRT.SampleLevel(sampler_linear_clamp, i.uv - texelOffset.zy, 0);
		float divergence = saturate(max(length(tr.xy - tl.xy), length(tu.xy - td.xy)) - 0.05);


		i.uv = 1 - abs(i.uv * 2 - 1);
		float lerpLod = saturate((i.uv.x * i.uv.y - 0.01) * 3);

		o.dest1 = divergence * lerpLod * 2;
		
		
		return o;
	}

	ENDHLSL


	SubShader
	{
		Cull Off
		ZWrite Off
		

		//0 bake pass
		Pass
		{
			ZTest Always

			HLSLPROGRAM


			#pragma vertex vertCompute
			#pragma fragment fragCompute

			#pragma multi_compile _ CAN_USE_NEXT_LOD
			#pragma shader_feature KW_FLUIDS_PREBAKE_SIM

			
			ENDHLSL
		}

		//1 compute pass
		Pass
		{
			ZTest GEqual

			HLSLPROGRAM


			#pragma vertex vertCompute
			#pragma fragment fragCompute

			#pragma multi_compile _ CAN_USE_NEXT_LOD
			#pragma shader_feature KW_FLUIDS_PREBAKE_SIM

			
			ENDHLSL
		}

		Pass //2 remap prebake data to 0-1 range
		{


			HLSLPROGRAM

			
			#pragma vertex vert
			#pragma fragment frag


			sampler2D _MainTex;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = float4(v.vertex.xy - 0.5, 0, 0.5);
				o.uv = float2(v.uv.x, 1 - v.uv.y);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				// data = clamp(data, float4(-20, -20, 0.5, -10), float4(20, 20, 100, 10));
				float4 data = (tex2D(_MainTex, i.uv) + FLUIDS_REMAP_MIN) / FLUIDS_REMAP_DIFF;

				return data;
			}
			ENDHLSL
		}

		Pass //3 render stencil mask

		{
			
			// ZTest Always
			Cull Off
			ZWrite On
			// Blend One One


			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

	
			sampler2D _MainTex;
			float3 KWS_WorldOffset;

			struct vertexInput
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct vertexOutput
			{
				float4 vertex : SV_POSITION;
				float4 screenUV : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};


			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = ObjectToClipPosOrtho(v.vertex);
				o.vertex.z = 0.5;
				o.screenUV = ComputeScreenPos(o.vertex);
				return o;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				// depth = 0.5;
				return 1;
			}
			ENDHLSL
		}
	}
}