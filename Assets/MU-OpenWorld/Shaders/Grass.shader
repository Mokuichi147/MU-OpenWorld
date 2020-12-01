Shader "Custom/Grass"
{
    Properties
    {
        _TopColor ("TopColor", Color) = (0,1,0,1)
        _BottomColor ("BottomColor", Color) = (0,0.3,0,1)
        _Height ("Height", Range(0,1)) = 0.3
        _Width ("Width", Range(0,1)) = 0.1
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

            #include "UnityCG.cginc"

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

            fixed4 _TopColor, _BottomColor;
            float _Height, _Width;

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

            // 3(三角面の頂点数) * 3(三角面の数) * 1(草の数)
            [maxvertexcount(9)]
            void geom (triangle appdata input[3], inout TriangleStream<g2f> stream)
            {
                float3 input_p0 = input[0].vertex.xyz;
                float3 input_p1 = input[1].vertex.xyz;
                float3 input_p2 = input[2].vertex.xyz;

                float3 center_p = (input_p0 + input_p1 + input_p2) / 3;

                float4 output_p0 = float4(center_p.x, center_p.y, center_p.z - _Width * 3, 1);
                float4 output_p1 = float4(center_p.x, center_p.y, center_p.z + _Width * 3, 1);
                float4 output_p2 = float4(center_p.x, center_p.y + _Height, center_p.z - _Width * 2, 1);
                float4 output_p3 = float4(center_p.x, center_p.y + _Height * 2, center_p.z + _Width, 1);
                float4 output_p4 = float4(center_p.x, center_p.y + _Height * 3, center_p.z, 1);

                fixed4 output_c0 = _BottomColor;
                fixed4 output_c1 = lerp(_BottomColor, _TopColor, 0.3);
                fixed4 output_c2 = lerp(_BottomColor, _TopColor, 0.6);
                fixed4 output_c3 = _TopColor;

                g2f p0;
                p0.color = output_c0;
                p0.vertex = output_p0;

                g2f p1;
                p1.color = output_c0;
                p1.vertex = output_p1;

                g2f p2;
                p2.color = output_c1;
                p2.vertex = output_p2;

                g2f p3;
                p3.color = output_c2;
                p3.vertex = output_p3;

                g2f p4;
                p4.color = output_c3;
                p4.vertex = output_p4;
                
                stream.Append(p0);
                stream.Append(p2);
                stream.Append(p1);
                stream.Append(p1);
                stream.Append(p2);
                stream.Append(p3);
                stream.Append(p2);
                stream.Append(p4);
                stream.Append(p3);
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
