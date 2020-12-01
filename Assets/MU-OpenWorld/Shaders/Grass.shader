Shader "Custom/Grass"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 3.0

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
            //UNITY_INSTANCING_BUFFER_END(Props)

            v2g vert (appdata input)
            {
                v2g output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                return output;
            }

            void geom (triangle appdata input, inout triangleStream<g2f> stream)
            {
                float3 position = input.vertex.xyz;
            }

            fixed4 frag (g2f input) : SV_Target
            {
                fixed4 output = input.color;
                return output;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
