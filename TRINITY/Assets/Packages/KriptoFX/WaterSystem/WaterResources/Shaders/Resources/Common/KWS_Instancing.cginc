#if defined (USE_WATER_INSTANCING)

#ifndef KWS_WATER_VARIABLES
	#include "..\Common\KWS_WaterVariables.cginc"
#endif

float4x4 inverseMatrix(float4x4 input)
{
    float det = determinant(input);
    float invDet = 1.0 / det;
    float3x3 m1 = float3x3(input._22_23_24, input._32_33_34, input._42_43_44);
    float3x3 m2 = float3x3(input._21_23_24, input._31_33_34, input._41_43_44);
    float3x3 m3 = float3x3(input._21_22_24, input._31_32_34, input._41_42_44);
    float3x3 m4 = float3x3(input._21_22_23, input._31_32_33, input._41_42_43);
    float3x3 m5 = float3x3(input._12_13_14, input._32_33_34, input._42_43_44);
    float3x3 m6 = float3x3(input._11_13_14, input._31_33_34, input._41_43_44);
    float3x3 m7 = float3x3(input._11_12_14, input._31_32_34, input._41_42_44);
    float3x3 m8 = float3x3(input._11_12_13, input._31_32_33, input._41_42_43);
    float3x3 m9 = float3x3(input._12_13_14, input._22_23_24, input._42_43_44);
    float3x3 m10 = float3x3(input._11_13_14, input._21_23_24, input._41_43_44);
    float3x3 m11 = float3x3(input._11_12_14, input._21_22_24, input._41_42_44);
    float3x3 m12 = float3x3(input._11_12_13, input._21_22_23, input._41_42_43);
    float3x3 m13 = float3x3(input._12_13_14, input._22_23_24, input._32_33_34);
    float3x3 m14 = float3x3(input._11_13_14, input._21_23_24, input._31_33_34);
    float3x3 m15 = float3x3(input._11_12_14, input._21_22_24, input._31_32_34);
    float3x3 m16 = float3x3(input._11_12_13, input._21_22_23, input._31_32_33);
    
    float4x4 cofactors = float4x4(
        determinant(m1), -determinant(m2), determinant(m3), -determinant(m4),
        -determinant(m5), determinant(m6), -determinant(m7), determinant(m8),
        determinant(m9), -determinant(m10), determinant(m11), -determinant(m12),
        -determinant(m13), determinant(m14), -determinant(m15), determinant(m16)
    );
    
    return transpose(cofactors) * invDet;
}


float4x4 inverse(float4x4 input)
{
#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
    float4x4 cofactors = float4x4(
        minor(_22_23_24, _32_33_34, _42_43_44),
        -minor(_21_23_24, _31_33_34, _41_43_44),
        minor(_21_22_24, _31_32_34, _41_42_44),
        -minor(_21_22_23, _31_32_33, _41_42_43),
        -minor(_12_13_14, _32_33_34, _42_43_44),
        minor(_11_13_14, _31_33_34, _41_43_44),
        -minor(_11_12_14, _31_32_34, _41_42_44),
        minor(_11_12_13, _31_32_33, _41_42_43),
        minor(_12_13_14, _22_23_24, _42_43_44),
        -minor(_11_13_14, _21_23_24, _41_43_44),
        minor(_11_12_14, _21_22_24, _41_42_44),
        -minor(_11_12_13, _21_22_23, _41_42_43),
        -minor(_12_13_14, _22_23_24, _32_33_34),
        minor(_11_13_14, _21_23_24, _31_33_34),
        -minor(_11_12_14, _21_22_24, _31_32_34),
        minor(_11_12_13, _21_22_23, _31_32_33)
        );
#undef minor
    return transpose(cofactors) / determinant(input);
}

inline float GetFlag(uint value, uint bit)
{
	return (value >> bit) & 0x01;
}

inline void UpdateInstanceSeamsAndSkirt(InstancedMeshDataStruct meshData, float2 uvData, inout float4 vertex)
{
	float quadOffset = uvData.y;
	uint mask = (uint)uvData.x;

	vertex.x -= quadOffset * GetFlag(mask, 1) * meshData.downSeam;
	vertex.z -= quadOffset * GetFlag(mask, 2) * meshData.leftSeam;
	vertex.x += quadOffset * GetFlag(mask, 3) * meshData.topSeam;
	vertex.z += quadOffset * GetFlag(mask, 4) * meshData.rightSeam;
	
	float downSkirt = GetFlag(mask, 5);
	float leftSkirt = GetFlag(mask, 6);
	float topSkirt = GetFlag(mask, 7);
	float rightSkirt = GetFlag(mask, 8);

	if (KWS_MeshType == KWS_MESH_TYPE_FINITE_BOX)
	{
		//#ifdef KWS_PRE_PASS_BACK_FACE
			vertex.y -= downSkirt * meshData.downInf;
			vertex.y -= leftSkirt * meshData.leftInf;
			vertex.y -= topSkirt * meshData.topInf;
			vertex.y -= rightSkirt* meshData.rightInf;
		//#endif

		if( downSkirt && meshData.downInf == 0 ||
			leftSkirt && meshData.leftInf == 0 ||
			topSkirt && meshData.topInf == 0   ||
			rightSkirt && meshData.rightInf == 0) vertex = NAN_VALUE; 
	}
	else
	{
		vertex.zy += 1000 * downSkirt * meshData.downInf * float2(-1, 0);
		vertex.xy += 1000 * leftSkirt * meshData.leftInf * float2(-1, 0);
		vertex.zy += 1000 * topSkirt * meshData.topInf * float2(1, 0);
		vertex.xy += 1000 * rightSkirt* meshData.rightInf * float2(1, 0);
	}
}


inline void UpdateInstaceRotation(inout float4 vertex, float4x4 matrixM, float4x4 matrixIM)
{
	//if (KWS_MeshType == KWS_MESH_TYPE_FINITE_BOX) vertex.xyz = mul((float3x3)KWS_InstancingRotationMatrix, vertex.xyz);
}

inline void SetMatrixM(float3 position, float3 size, inout float4x4 matrixM)
{
	position.y += 0.001;

	matrixM._11_21_31_41 = float4(size.x, 0,  0, 0);
	matrixM._12_22_32_42 = float4(0, size.y, 0, 0);
	matrixM._13_23_33_43 = float4(0, 0, size.z, 0);
	matrixM._14_24_34_44 = float4(position.xyz, 1);
	
	UNITY_BRANCH
	if (KWS_MeshType == KWS_MESH_TYPE_FINITE_BOX)
	{
		float s = sin(-KWS_WaterYRotationRad);
		float c = cos(-KWS_WaterYRotationRad);

		float4x4 rotationMatrix;
		rotationMatrix[0] = float4(c, 0, -s, 0);
		rotationMatrix[1] = float4(0, 1, 0, 0);
		rotationMatrix[2] = float4(s, 0, c, 0);
		rotationMatrix[3] = float4(0, 0, 0, 1);
 
		matrixM._14_24_34 -= KW_WaterPosition;
		float4x4 rotatedMatrix = mul(rotationMatrix, matrixM);
		float4x4 newCustomMatrix = matrixM;
	
		newCustomMatrix = rotatedMatrix;
		newCustomMatrix._14_24_34 += KW_WaterPosition;

		matrixM =  newCustomMatrix;
		
	}
}

inline void UpdateInstanceMatrixM(InstancedMeshDataStruct meshData, inout float4x4 matrixM)
{
	SetMatrixM(meshData.position.xyz, meshData.size.xyz, matrixM);
	matrixM = UpdateCameraRelativeMatrix(matrixM);
}


inline void UpdateAllInstanceMatrixes(InstancedMeshDataStruct meshData, inout float4x4 matrixM, inout float4x4 matrixIM)
{
	SetMatrixM(meshData.position.xyz, meshData.size.xyz, matrixM);
	
	UNITY_BRANCH
	if (KWS_MeshType == KWS_MESH_TYPE_FINITE_BOX)
	{
		matrixIM = inverse(matrixM);
	}
	else
	{	
		matrixIM = matrixM;
		matrixIM._14_24_34 *= -1;
		matrixIM._11_22_33 = 1.0f / matrixIM._11_22_33;
	}
	matrixM = UpdateCameraRelativeMatrix(matrixM);
}


inline void UpdateAllInstanceMatrixes(uint instanceID, inout float4x4 matrixM, inout float4x4 matrixIM)
{
	UpdateAllInstanceMatrixes(InstancedMeshData[instanceID], matrixM, matrixIM);
}


inline void UpdateInstanceData(uint instanceID, float2 uvData, inout float4 vertex, inout float4x4 matrixM, inout float4x4 matrixIM)
{
	InstancedMeshDataStruct meshData = InstancedMeshData[instanceID];
	UpdateInstanceSeamsAndSkirt(meshData, uvData, vertex);
	
	UpdateInstaceRotation(vertex, matrixM, matrixIM);
	UpdateAllInstanceMatrixes(meshData, matrixM, matrixIM);
}
#endif