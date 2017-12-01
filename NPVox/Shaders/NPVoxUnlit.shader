Shader "NPVox/Unlit" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        // _NormalBlend("centoid <-> voxel", Range(0,1)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows //vertex:vert
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input {
            float3 color_Tex: COLOR;
        };

        // half _NormalBlend;
        half _Metallic;
        half _Smoothness;
        half _Occlusion;
        fixed4 _Emission;
        fixed4 _Color;
        
        // // Vertex Shader
        // void vert(inout appdata_full v)
        // {
        //     // blend together centoid & normals
        //     if(_NormalBlend < 1) 
        //     {
        //         half3 voxelPos = v.tangent.xyz;
        //         half3 normal = v.normal.xyz;
        //         v.normal = normalize(voxelPos) * (1.0 - _NormalBlend) + normal * _NormalBlend;
        //     } 
        // }

        // Surface Shader
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color * IN.color_Tex;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
