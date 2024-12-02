#ifndef UNITY_STANDARD_PARTICLE_INSTANCING_INCLUDED
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
#define UNITY_STANDARD_PARTICLE_INSTANCING_INCLUDED

#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) && !defined(SHADER_TARGET_SURFACE_ANALYSIS)
	#define UNITY_PARTICLE_INSTANCING_ENABLED
#endif

#if defined(UNITY_PARTICLE_INSTANCING_ENABLED)

	#ifndef UNITY_PARTICLE_INSTANCE_DATA
		#define UNITY_PARTICLE_INSTANCE_DATA DefaultParticleInstanceData
	#endif

	struct DefaultParticleInstanceData
	{
		float3x4 transform;
		uint color;
		float animFrame;
	};

	StructuredBuffer<UNITY_PARTICLE_INSTANCE_DATA> unity_ParticleInstanceData;
	float4 unity_ParticleUVShiftData;
	float unity_ParticleUseMeshColors;

	void vertInstancingMatrices()
	{
		#ifdef unity_ObjectToWorld
			#undef unity_ObjectToWorld
		#endif
		#ifdef unity_WorldToObject
			#undef unity_WorldToObject
		#endif

		UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];

		// transform matrix
		unity_ObjectToWorld._11_21_31_41 = float4(data.transform._11_21_31, 0.0f);
		unity_ObjectToWorld._12_22_32_42 = float4(data.transform._12_22_32, 0.0f);
		unity_ObjectToWorld._13_23_33_43 = float4(data.transform._13_23_33, 0.0f);
		unity_ObjectToWorld._14_24_34_44 = float4(data.transform._14_24_34, 1.0f);

		// inverse transform matrix
		float3x3 w2oRotation;
		w2oRotation[0] = unity_ObjectToWorld[1].yzx * unity_ObjectToWorld[2].zxy - unity_ObjectToWorld[1].zxy * unity_ObjectToWorld[2].yzx;
		w2oRotation[1] = unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[2].yzx - unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[2].zxy;
		w2oRotation[2] = unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[1].zxy - unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[1].yzx;

		float det = dot(unity_ObjectToWorld[0].xyz, w2oRotation[0]);

		w2oRotation = transpose(w2oRotation);

		w2oRotation *= rcp(det);

		float3 w2oPosition = mul(w2oRotation, -unity_ObjectToWorld._14_24_34);

		unity_WorldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
		unity_WorldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
		unity_WorldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
		unity_WorldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);

	}

	void vertInstancingSetup()
	{
		#if defined(UNITY_COMPILER_DXC)
			// we'll do this later if DXC is on
		#else
			vertInstancingMatrices();
		#endif
	}

	void vertInstancingColor(inout half4 color)
	{
		#ifndef UNITY_PARTICLE_INSTANCE_DATA_NO_COLOR
			UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];
			color = lerp(half4(1.0f, 1.0f, 1.0f, 1.0f), color, unity_ParticleUseMeshColors);
			color *= float4(data.color & 255, (data.color >> 8) & 255, (data.color >> 16) & 255, (data.color >> 24) & 255) * (1.0f / 255);
		#endif
	}

	void vertInstancingUVs(in float2 uv, out float2 texcoord, out float3 texcoord2AndBlend)
	{
		if (unity_ParticleUVShiftData.x != 0.0f)
		{
			UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];

			float numTilesX = unity_ParticleUVShiftData.y;
			float2 animScale = unity_ParticleUVShiftData.zw;
			#ifdef UNITY_PARTICLE_INSTANCE_DATA_NO_ANIM_FRAME
				float sheetIndex = 0.0f;
			#else
				float sheetIndex = data.animFrame;
			#endif

			float index0 = floor(sheetIndex);
			float vIdx0 = floor(index0 / numTilesX);
			float uIdx0 = floor(index0 - vIdx0 * numTilesX);
			float2 offset0 = float2(uIdx0 * animScale.x, (1.0f - animScale.y) - vIdx0 * animScale.y);

			texcoord = uv * animScale.xy + offset0.xy;

			#ifdef _FLIPBOOK_BLENDING
				float index1 = floor(sheetIndex + 1.0f);
				float vIdx1 = floor(index1 / numTilesX);
				float uIdx1 = floor(index1 - vIdx1 * numTilesX);
				float2 offset1 = float2(uIdx1 * animScale.x, (1.0f - animScale.y) - vIdx1 * animScale.y);

				texcoord2AndBlend.xy = uv * animScale.xy + offset1.xy;
				texcoord2AndBlend.z = frac(sheetIndex);
			#else
				texcoord2AndBlend.xy = texcoord;
				texcoord2AndBlend.z = 0.0f;
			#endif
		}
		else
		{
			texcoord = uv;
			texcoord2AndBlend.xy = uv;
			texcoord2AndBlend.z = 0.0f;
		}
	}

	void vertInstancingUVs(in float2 uv, out float2 texcoord)
	{
		float3 texcoord2AndBlend = 0.0f;
		vertInstancingUVs(uv, texcoord, texcoord2AndBlend);
	}

#else

	void vertInstancingSetup()
	{

	}
	void vertInstancingColor(inout half4 color)
	{

	}
	void vertInstancingUVs(in float2 uv, out float2 texcoord, out float3 texcoord2AndBlend)
	{
		texcoord = uv; texcoord2AndBlend = 0.0f;
	}
	void vertInstancingUVs(in float2 uv, out float2 texcoord)
	{
		texcoord = uv;
	}

#endif

#endif // UNITY_STANDARD_PARTICLE_INSTANCING_INCLUDED


#define UNITY_PARTICLE_INSTANCE_DATA MyParticleInstanceData

struct MyParticleInstanceData
{
	float3x4 transform;
	uint color;
	float speed;
};



void GetInstancedData_float(float2 vertexUV, float4 vertexColor, out float2 uv, out float4 color)
{
	uv = vertexUV;
	color = vertexColor;
	vertInstancingColor(color);
	vertInstancingUVs(vertexUV, uv);
}


void GetPositionData_float(float3 vertex, out float3 result)
{
	#if !defined(UNITY_COMPILER_DXC) || !defined(UNITY_PARTICLE_INSTANCING_ENABLED)
		result = vertex;
	#else

		UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];
		float4x4 objectToWorld;
		
		objectToWorld._11_21_31_41 = float4(data.transform._11_21_31, 0.0f);
		objectToWorld._12_22_32_42 = float4(data.transform._12_22_32, 0.0f);
		objectToWorld._13_23_33_43 = float4(data.transform._13_23_33, 0.0f);
		objectToWorld._14_24_34_44 = float4(data.transform._14_24_34, 1.0f);

		result = mul(objectToWorld, float4(vertex.xyz, 1)).xyz;
		result = vertex;
	#endif
}