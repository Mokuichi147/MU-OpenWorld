Shader "Custom/Grass"
{
    Properties
    {
        _TopColor ("TopColor", Color) = (0,1,0,1)
        _BottomColor ("BottomColor", Color) = (0,0.3,0,1)
        _Height ("Height", Range(0,1)) = 0.3
        _Width ("Width", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 300
        Pass
        {
            Name "Grass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            // マテリアルキーワード
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _OCCLUSIONMAP

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            // URPキーワード
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            // Unityデフォルトキーワード
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float2 uvLM       : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv                     : TEXCOORD0;
                float2 uvLM                   : TEXCOORD1;
                // xyz: positionWS, w: vertex fog factor
                float4 positionWSAndFogFactor : TEXCOORD2;
                half3  normalWS               : TEXCOORD3;
#if _NORMALMAP
                half3 tangentWS                 : TEXCOORD4;
                half3 bitangentWS               : TEXCOORD5;
#endif

#ifdef _MAIN_LIGHT_SHADOWS
                // compute shadow coord per-vertex for the main light
                float4 shadowCoord              : TEXCOORD6;
#endif
                float4 positionCS               : SV_POSITION;
            };


            float4 _TopColor, _BottomColor;
            float _Height, _Width;

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.uvLM = input.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw;

                output.positionWSAndFogFactor = float4(vertexInput.positionWS, fogFactor);
                output.normalWS = vertexNormalInput.normalWS;

#ifdef _NORMALMAP
                output.tangentWS = vertexNormalInput.tangentWS;
                output.bitangentWS = vertexNormalInput.bitangentWS;
#endif

#ifdef _MAIN_LIGHT_SHADOWS
                output.shadowCoord = GetShadowCoord(vertexInput);
#endif
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            inline Varyings cp2stream(Varyings v, float4 vertex, float4 color)
            {
                Varyings p = v;
                p.positionCS = float4(vertex.xyz, 1.0f);
                return p;
            }
            
            // 3(三角面の頂点数) * 3(三角面の数) * 1(草の数)
            [maxvertexcount(3)]
            void geom (triangle Varyings input[3], inout TriangleStream<Varyings> stream)
            {
                /*
                float3 input_p0 = input[0].positionCS.xyz;
                float3 input_p1 = input[1].positionCS.xyz;
                float3 input_p2 = input[2].positionCS.xyz;

                float3 center_p = (input_p0 + input_p1 + input_p2) / 3;

                float4 p0 = float4(center_p.x, center_p.y, center_p.z - _Width * 3, 1.0f);
                float4 p1 = float4(center_p.x, center_p.y, center_p.z + _Width * 3, 1.0f);
                float4 p2 = float4(center_p.x, center_p.y + _Height, center_p.z - _Width * 2, 1.0f);
                float4 p3 = float4(center_p.x, center_p.y + _Height * 2, center_p.z + _Width, 1.0f);
                float4 p4 = float4(center_p.x, center_p.y + _Height * 3, center_p.z, 1.0f);

                float4 c0 = _BottomColor;
                float4 c1 = lerp(_BottomColor, _TopColor, 0.3);
                float4 c2 = lerp(_BottomColor, _TopColor, 0.6);
                float4 c3 = _TopColor;

                stream.Append(cp2stream(input[0], c0, p0));
                stream.Append(cp2stream(input[0], c1, p2));
                stream.Append(cp2stream(input[0], c0, p1));
                stream.RestartStrip();
                stream.Append(cp2stream(input[0], c0, p1));
                stream.Append(cp2stream(input[0], c1, p2));
                stream.Append(cp2stream(input[0], c2, p3));
                stream.RestartStrip();
                stream.Append(cp2stream(input[0], c1, p2));
                stream.Append(cp2stream(input[0], c3, p4));
                stream.Append(cp2stream(input[0], c2, p3));
                stream.RestartStrip();
                */
                stream.Append(input[0]);
                stream.Append(input[1]);
                stream.Append(input[2]);
                //stream.RestartStrip();
            }

            float4 frag (Varyings input) : SV_Target
            {
                float4 output = _TopColor;
                return output;
            }
            ENDHLSL
        }
        //UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        //UsePass "Universal Render Pipeline/Lit/DepthOnly"
        //UsePass "Universal Render Pipeline/Lit/Meta"
    }
}
