Shader "NPVox/Wave" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _NormalBlend("centoid <-> voxel", Range(0,1)) = 0.5
        [MaterialToggle] _Round("Round to Voxel-Space", Float) = 0
        _Speed ("Speed", float) = 15.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input {
            float3 color_Tex: COLOR;
        };

        float _NormalBlend;
        fixed4 _Color;
        float _Speed;
        bool _Round;
        
        // explode effenct
        float3 waveEffect(float3 vert, float3 centoid)
        {
            // waving effenct
            vert.x = vert.x + centoid.x * sin(_Time * _Speed);
            vert.y = vert.y + centoid.y * sin((1-_Time) * _Speed);
            vert.z = vert.z + centoid.z * sin((2-_Time) * _Speed);
            
            return vert;
        }
        
        // Vertex Shader
        void vert(inout appdata_full v)
        {
            float3 voxelPos = v.tangent.xyz;
            float4 vert = v.vertex;
            float3 normal = v.normal.xyz;
            float3 centoid = normalize(voxelPos);
            float3 vertPosInVoxel = vert - voxelPos;
            float3 VoxelSize = abs(vertPosInVoxel)*2;

            vert.xyz = waveEffect(vert, centoid);

            // ensure we are in bounds
            if(_Round) {
                vert.xyz = round((vert.xyz ) * 1/VoxelSize) * VoxelSize;
            }

            v.vertex = vert;

            // blend together centoid & normals
            if(_NormalBlend < 1) {
                v.normal = centoid * (1.0 - _NormalBlend) + normal * _NormalBlend;
            } 
        }

        // Surface Shader
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Metallic and smoothness come from slider variables
            o.Albedo = _Color * IN.color_Tex;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
