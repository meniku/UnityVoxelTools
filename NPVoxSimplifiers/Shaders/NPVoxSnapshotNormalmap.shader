// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NPVox/Snapshot/Normalmap" {
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
				// float3 color_Tex: COLOR;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				// float3 normal : NORMAL;
				float3 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			// sampler2D _MainTex;
			// float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 sourceNormal = mul(UNITY_MATRIX_V, v.normal);
				float3 normal;
				normal.x = clamp( (sourceNormal.x) * 0.5 + 0.5, 0.0, 1.0);
			 	normal.y = clamp( (sourceNormal.y) * 0.5 + 0.5, 0.0, 1.0);
				normal.z = clamp( (sourceNormal.z) * 0.5 + 0.5, 0.0, 1.0);
				// normal.x = .5;
				// normal.y = .5;
				// normal.z = 1;
				// normal = normalize(normal);
				o.color = normal;
				// o.normal = normal; 
				
				// float3 col;
				// col.r = 1.0f;
				// col.gb = 0.0f;
				// o.color = col;
				
				// float3 normal;
				// o.normal = normal;
				// o.normal = v.normal;
				// o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// UNITY_TRANSFER_FOG(o,o.vertex);
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
