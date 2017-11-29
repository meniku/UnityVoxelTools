Shader "NPVox/StaticSpecular" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Specular("Specular", Range(0,1)) = 0.5
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Emission ("Emission", Color) = (0,0,0,1)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based StandardSpecular lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular fullforwardshadows //vertex:vert
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input {
            float3 color_Tex: COLOR;
        };

        // float _NormalBlend;
        half _Smoothness;
        half _Specular;
        half _Occlusion;
        fixed4 _Color;
        fixed4 _Emission;
        
        // // Vertex Shader
        // void vert(inout appdata_full v)
        // {
        //     float3 voxelPos = v.tangent.xyz;
        //     float3 normal = v.normal.xyz;

        //     // blend together centoid & normals
        //     if(_NormalBlend < 1) {
        //         v.normal = normalize(voxelPos) * (1.0 - _NormalBlend) + normal * _NormalBlend;
        //     } 
        // }

        // Surface Shader
        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            o.Albedo = _Color * IN.color_Tex;
            o.Specular = _Specular;   
            o.Smoothness = _Smoothness;   
            o.Emission = _Emission; 
        }
        ENDCG
    }
    FallBack "Diffuse"
}
