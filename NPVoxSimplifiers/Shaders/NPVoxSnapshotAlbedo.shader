// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NPVox/Snapshot/Albedo" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
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
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 color_Tex: COLOR;
			};

			struct v2f
			{
				float3 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			// sampler2D _MainTex;
			// float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color_Tex;
				// o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed3 frag (v2f i) : SV_Target
			{
				fixed3 col = i.color;
				return col;
			}
			ENDCG
		}
	}
}
