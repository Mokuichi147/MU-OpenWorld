Shader "Custom/GrassLow"
{
    Properties
    {
        _TopColor ("TopColor", Color) = (0,1,0,1)
        [MainColor] _BaseColor ("Color", Color) = (0,0.3,0,1)
        _Height ("Height", Range(0,1)) = 0.3
        _HeightRange ("HeightRange", Float) = 0.05
        _Width ("Width", Range(0,0.5)) = 0.03
        _GrowHeight ("GrowHeight", Float) = 0.0
        _GrowRange ("GrowRange", Float) = 1.0

        // Specular vs Metallic workflow
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0

        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        _ReceiveShadows("Receive Shadows", Float) = 1.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 100
        // 両面表示
        Cull off
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
                half3 tangentWS               : TEXCOORD4;
                half3 bitangentWS             : TEXCOORD5;
#endif

#ifdef _MAIN_LIGHT_SHADOWS
                // compute shadow coord per-vertex for the main light
                float4 shadowCoord            : TEXCOORD6;
#endif
                float3 positionWS             : TEXCOORD7;
                float4 positionCS             : SV_POSITION;
            };


            float4 _TopColor, _BottomColor;
            float _Height, _HeightRange, _Width, _GrowHeight, _GrowRange;

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
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            inline float random(float2 uv, int seed)
            {
                return frac(sin(dot(uv, float2(12.9898,78.233)) + seed) * 43758.5453);
            }

            inline float pmrandom(float2 uv, int seed)
            {
                // -1.0f ～ 1.0f
                return (random(uv, seed) - 0.5f) * 2.0f;
            }

            inline float3 dir_random(float2 uv)
            {
                float3 vec1 = float3(pmrandom(uv,0), 0, pmrandom(uv,3));
                return normalize(vec1);
            }

            Varyings geom_stream(Varyings v, float3 pos)
            {
                Varyings output = v;
                output.positionWS = pos;
                output.positionCS = TransformWorldToHClip(pos);
                return output;
            }
            
            // (三角面の頂点数) * (草の数)
            [maxvertexcount(3)]
            void geom(triangle Varyings input[3], inout TriangleStream<Varyings> stream)
            {
                float3 cp = (input[0].positionWS + input[1].positionWS + input[2].positionWS) / 3.0f;
                half3  cn = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3.0f;
                half3 height_n3 = cn;

                float grow = _GrowHeight + _GrowRange * pmrandom(cp.xy, 0);

                float3 p;
                uint i;

                //[unroll]
                float _r1 = random(cp.xy, 0);
                p = lerp(input[0].positionWS, input[1].positionWS, _r1);
                float _r2 = random(cp.xy, 2);
                p = lerp(p, input[2].positionWS, _r2);

                //[loop]
                if (p.y > grow)
                {
                    float3 width_n3 = dir_random(p.xy);
                    stream.Append(geom_stream(input[0], p + width_n3 * _Width * -6));
                    stream.Append(geom_stream(input[0], p + height_n3 * _Height * 3.0));
                    stream.Append(geom_stream(input[0], p + width_n3 * _Width *  6));
                    stream.RestartStrip();
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData);

#if _NORMALMAP
                half3 normalWS = TransformTangentToWorld(surfaceData.normalTS,
                    half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
#else
                half3 normalWS = input.normalWS;
#endif
                normalWS = normalize(normalWS);

#ifdef LIGHTMAP_ON
                half3 bakedGI = SampleLightmap(input.uvLM, normalWS);
#else
                half3 bakedGI = SampleSH(normalWS);
#endif

                float3 positionWS = input.positionWSAndFogFactor.xyz;
                half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - positionWS);

                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

#ifdef _MAIN_LIGHT_SHADOWS
                Light mainLight = GetMainLight(input.shadowCoord);
#else
                Light mainLight = GetMainLight();
#endif
                half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWS, viewDirectionWS);

                color += LightingPhysicallyBased(brdfData, mainLight, normalWS, viewDirectionWS);

#ifdef _ADDITIONAL_LIGHTS
                int additionalLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    Light light = GetAdditionalLight(i, positionWS);

                    color += LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS);
                }
#endif
                color += surfaceData.emission;

                float fogFactor = input.positionWSAndFogFactor.w;

                color = MixFog(color, fogFactor);
                return half4(color, surfaceData.alpha);
            }
            ENDHLSL
        }
    }
}
