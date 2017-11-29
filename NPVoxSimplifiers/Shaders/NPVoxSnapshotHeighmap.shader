// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NPVox/Snapshot/Heighmap" {
    Properties {
        //_Color ("Color", Color) = (1,1,1,1)
    }
	SubShader
	{
		Lighting Off
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
		//	#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				// float3 normal : NORMAL;
				float3 color : COLOR;
				float4 vertex : SV_POSITION;
			};
			
			float3 decodeModelSize(int encoded)
			{
				float3 size = {0,0,0};
				size.x = (encoded >> 14)&0x7F;
				size.y = (encoded >> 7)&0x7F;
				size.z = encoded&0x7F;
				return size;
			}

			// sampler2D _MainTex;
			// float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				float3 voxelPos = v.tangent.xyz;
				float4 vert = v.vertex;
				float3 vertPosInVoxel = vert - voxelPos;
            	float3 VoxelSize = abs(vertPosInVoxel)*2;
				float3 modelSize = decodeModelSize(v.tangent.w);
				float3 modelSizeUnity = modelSize * VoxelSize;
				
				// float3 sourceNormal = mul(UNITY_MATRIX_V, v.normal);
				// float3 normal;
				// normal.x = clamp( (sourceNormal.x) * 0.5 + 0.5, 0.0, 1.0);
			 	// normal.y = clamp( (sourceNormal.y) * 0.5 + 0.5, 0.0, 1.0);
				// normal.z = clamp( (sourceNormal.z) * 0.5 + 0.5, 0.0, 1.0);
				
				float4 color;
				color.rgb = abs( voxelPos.y * voxelPos.y ) ; //2.0f * abs((float) voxelPos.y / (float) (modelSizeUnity.y * 1.0f));
				color.a = 1.0f;
				
				o.color = color;
				return o;
			}
			
			float3 frag (v2f i) : SV_Target
			{
				float3 col = i.color;
				// col.r = 0.5f;
				// col.g = 0.5f;
				// col.b = 1;
				return col;
			}
			ENDCG
		}
	}
}
