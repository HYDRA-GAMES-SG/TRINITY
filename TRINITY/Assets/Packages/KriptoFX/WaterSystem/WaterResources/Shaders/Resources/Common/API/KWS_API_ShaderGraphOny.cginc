#ifndef KWS_API_SHADERGRAPH_ONLY
#define KWS_API_SHADERGRAPH_ONLY


//////////////////////////////////////  Stochastic sample //////////////////////////////////////////////

float2 hash2D2D(float2 s)
{
	return frac(sin(fmod(float2(dot(s, float2(127.1, 311.7)), dot(s, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
}

void Tex2DStochastic_float(UnityTexture2D tex, float2 uv, out float4 result)
{
	#ifdef SHADERGRAPH_PREVIEW
		result = 0;
	#else

		uv = tex.GetTransformedUV(uv);
		float2 skewUV = mul(float2x2(1.0, 0.0, -0.57735027, 1.15470054), uv * 3.464);

		float2 vxID = float2(floor(skewUV));
		float3 barry = float3(frac(skewUV), 0);
		barry.z = 1.0 - barry.x - barry.y;

		float2 hash1, hash2, hash3;
		float3 weight;

		if (barry.z > 0) {
			hash1 = vxID;
			hash2 = vxID + float2(0, 1);
			hash3 = vxID + float2(1, 0);
			weight = barry.zyx;
		} else {
			hash1 = vxID + float2(1, 1);
			hash2 = vxID + float2(1, 0);
			hash3 = vxID + float2(0, 1);
			weight = float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x);
		}

		float2 dx = ddx(uv);
		float2 dy = ddy(uv);
		
		float4 tex1 = SAMPLE_TEXTURE2D_GRAD(tex, tex.samplerstate, uv + hash2D2D(hash1.xy), dx, dy) * weight.x;
		float4 tex2 = SAMPLE_TEXTURE2D_GRAD(tex, tex.samplerstate, uv + hash2D2D(hash2.xy), dx, dy) * weight.y;
		float4 tex3 = SAMPLE_TEXTURE2D_GRAD(tex, tex.samplerstate, uv + hash2D2D(hash3.xy), dx, dy) * weight.z;

		result = tex1 + tex2 + tex3;
		
	#endif
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////



#endif