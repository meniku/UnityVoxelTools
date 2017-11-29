Shader "NPVox/Explode" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _NormalBlend("centoid <-> voxel", Range(0,1)) = 0.5
        _Step("Step", Range(0,1)) = 0.0
        [MaterialToggle] _Round("Round to Voxel-Space", Float) = 0
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
        float _Step;
        bool _Round;
        
        // explode effenct
        float3 explodeEffect(float3 vert, float3 vertPosInVoxel, float3 centoid, float minZ)
        {
            float step = _Step;

            const float PI = 3.14159265359;
            float3 impulse;
            impulse.xy = centoid.zz * 0.3 + centoid.xy*1.2;
            impulse.z = 1.4 + centoid.z * 0.6;

            vert.xy += impulse.xy * sin(step*PI*0.5);
            vert.z  += impulse.z * sin(step*PI) -1 * sin(0.5*step * PI);

            // stop at ground
            vert.z = max(vert.z, minZ + vertPosInVoxel.z);

            return vert;
        }
        
        float3 decodeModelSize(int encoded)
        {
            float3 size = {0,0,0};
            size.x = (encoded >> 14)&0x7F;
            size.y = (encoded >> 7)&0x7F;
            size.z = encoded&0x7F;
            return size;
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
            float3 modelSize = decodeModelSize(v.tangent.w);

            vert.xyz = explodeEffect(vert, vertPosInVoxel, centoid, modelSize.z * VoxelSize.z * -0.5f+ VoxelSize.z * 0.5f);

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
