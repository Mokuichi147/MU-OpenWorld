Shader "Custom/Grass"
{
    Properties
    {
        _TopColor ("TopColor", Color) = (0,1,0,1)
        [MainColor] _BaseColor ("Color", Color) = (0,0.3,0,1)
        _Height ("Height", Range(0,1)) = 0.3
        _Width ("Width", Range(0,0.1)) = 0.03

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
        LOD 200
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
                float4 positionCS             : SV_POSITION;
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
                output.positionCS = input.positionOS;
                return output;
            }

            inline float random(float2 uv, float2 seed)
            {
                float2 map = uv + seed;
                return frac(sin(dot(uv, float2(12.9898,78.233))) * 43758.5453);
            }

            Varyings geom_stream(Varyings v, float4 positionOS, half3 normalWS)
            {
                Varyings output = v;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS.xyz);
                float dx = random(v.uv, float2(3.0f, 5.0f)) * 0.12f;
                float dz = random(v.uv, float2(7.0f, 11.0f)) * 0.12f;
                output.positionCS = vertexInput.positionCS + float4(dx, 0.0f, dz, 0.0f);
                output.normalWS = normalWS;
                return output;
            }
            
            // 3(三角面の頂点数) * 3(三角面の数) * 1(草の数)
            [maxvertexcount(5)]
            void geom(triangle Varyings input[3], inout TriangleStream<Varyings> stream)
            {
                float4 cp = (input[0].positionCS + input[1].positionCS + input[2].positionCS) / 3.0f;
                half3  cn = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3.0f;
                float4 height_n4 = float4(cn, 1.0f);
                float4 width_n4 = float4(normalize(input[2].positionCS*random(input[0].uv,float2(13.0f,17.0f)) - input[0].positionCS*random(input[0].uv,float2(23.0f,31.0f))).xyz, 1.0f);

                float height = max(_Height * random(input[0].uv, float2(0.0f, 1.0f)), 0.01f) * 5.0f;

                float4 p0 = float4(_Width*-3,     0.0f, 0.0f, 0.0f);
                float4 p1 = float4(_Width* 3,     0.0f, 0.0f, 0.0f);
                float4 p2 = float4(_Width*-2,   height, 0.0f, 0.0f);
                float4 p3 = float4(_Width* 1, height*2, 0.0f, 0.0f);
                float4 p4 = float4(     0.0f, height*3, 0.0f, 0.0f);

                float sx = ((_SinTime.w + 1.0f) * 0.5f - 0.5f) * 5.0f;

                stream.Append(geom_stream(input[0], cp + width_n4 * _Width * -3, cn));
                stream.Append(geom_stream(input[0], cp + width_n4 * _Width * 3, cn));
                stream.Append(geom_stream(input[0], cp + width_n4 * _Width * (1+sx) * -1 + height_n4 * _Height, cn));
                stream.Append(geom_stream(input[0], cp + width_n4 * _Width * (1-sx) * 2 + height_n4 * _Height * 2, cn));
                stream.Append(geom_stream(input[0], cp + width_n4 * _Width * -3*sx + height_n4 * _Height * 3, cn));
                //stream.RestartStrip();
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
        //UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        //UsePass "Universal Render Pipeline/Lit/DepthOnly"
        //UsePass "Universal Render Pipeline/Lit/Meta"
    }
}
