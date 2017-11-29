Shader "NPVox/Transparent" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Emission ("Emission", Color) = (0,0,0,1)
        _EmissionLerp ("EmissionLerp", Range(0,1)) = 0
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200

        pass {
           ZWrite On
           ColorMask 0
        }

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd alpha:fade

        struct Input 
        {
            float4 color_Tex: COLOR;
        };

        fixed4 _Color;
        half4 _Emission;
        float _EmissionLerp;

        // Surface Shader
        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 col = _Color * IN.color_Tex;
            o.Albedo = col.rgb;
            o.Emission = lerp(_Emission, IN.color_Tex, _EmissionLerp);
            o.Alpha = col.a;
        }
        ENDCG
    }

    FallBack "Transparent/VertexLit"
}
