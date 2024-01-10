Shader "Impostor/UPR/BaseUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ImpostorTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
           "Queue" = "Transparent"   "RenderPipeline" = "UniversalRenderPipeline"
        }
        LOD 300
//		Cull Off
//        Blend SrcAlpha OneMinusSrcAlpha
//        ZWrite Off
//			
        Pass
        {
            Name "Main"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_instancing
            #pragma multi_compile  _MAIN_LIGHT_SHADOWS
            #pragma multi_compile  _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile  _SHADOWS_SOFT
            #include "ImpostorCommon.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS: SV_POSITION;
                //世界空间顶点
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);

                float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.positionWS);
                Light lightData = GetMainLight(SHADOW_COORDS);
                half shadow = lightData.shadowAttenuation;

                col = shadow * col * _Color;
                return col;
            }
            ENDHLSL
        }

       Pass
       {
           Name "ImpostorSnapshot"
           Tags
           {
               "LightMode" = "UniversalForward"
           }
           HLSLPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           // make fog work
           #pragma multi_compile_instancing
           #pragma multi_compile  _MAIN_LIGHT_SHADOWS
           #pragma multi_compile  _MAIN_LIGHT_SHADOWS_CASCADE
           #pragma multi_compile  _SHADOWS_SOFT
           #include "ImpostorCommon.hlsl"

           struct appdata
           {
               float4 positionOS : POSITION;
               float2 uv : TEXCOORD0;
               UNITY_VERTEX_INPUT_INSTANCE_ID
           };

           struct v2f
           {
               float2 uv : TEXCOORD0;
               float4 positionCS: SV_POSITION;
               //世界空间顶点
               float3 positionWS : TEXCOORD1;
               UNITY_VERTEX_INPUT_INSTANCE_ID
           };

           sampler2D _MainTex;
           float4 _MainTex_ST;
           float4 _Color;

           v2f vert(appdata input)
           {
               v2f output;
               UNITY_SETUP_INSTANCE_ID(input);
               UNITY_TRANSFER_INSTANCE_ID(input, output);
               VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
               output.positionCS = positionInputs.positionCS;
               output.positionWS = positionInputs.positionWS;
               output.uv = TRANSFORM_TEX(input.uv, _MainTex);
               return output;
           }

           half4 frag(v2f i) : SV_Target
           {
               half4 col = tex2D(_MainTex, i.uv);
               float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.positionWS);
               Light lightData = GetMainLight(SHADOW_COORDS);
               half shadow = lightData.shadowAttenuation;
               col = shadow * col * _Color;
               return col;
           }
           ENDHLSL
       }

       Pass
       {
           Name "Impostor"
           Tags
           {
               "LightMode" = "UniversalForward"
           }

           Cull[_Cull]
           HLSLPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma multi_compile_instancing
           #include "ImpostorCommon.hlsl"

           UNITY_INSTANCING_BUFFER_START(Props)
           UNITY_DEFINE_INSTANCED_PROP(float, _Size)
           #define _Size_arr Props
           UNITY_INSTANCING_BUFFER_END(Props)

           sampler2D _ImpostorTex;
           float4 _ImpostorTex_ST;

           struct appdata
           {
               float4 positionOS : POSITION;
               float2 uv : TEXCOORD0;
               UNITY_VERTEX_INPUT_INSTANCE_ID
           };

           float3 GetBillboard(float3 vertex)
           {
               // object space
               float3 center = float3(0, 0, 0);
               float3 viewer = TransformWorldToObject(_WorldSpaceCameraPos);

               float3 normalDir = viewer - center;

               //normalDir.y = normalDir.y * _VerticalBillboarding;
               normalDir = normalize(normalDir);

               // float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1): float3(0, 1, 0);
               float3 upDir = float3(0, 1, 0);
               float3 rightDir = normalize(cross(upDir, normalDir));
               upDir = normalize(cross(normalDir, rightDir));

               float3 centerOffs = vertex.xyz - center;
               float3 localPos = center - rightDir * centerOffs.x + upDir * centerOffs.y; // + normalDir * centerOffs.z;

               return localPos;
           }

           float3 GetBillboardNoRot(float3 vertex)
           {
               // object space
               float3 center = float3(0, 0, 0);

               float3 normalDir = -GetViewForwardDir();
               normalDir = normalize(normalDir);

               //float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
               float3 upDir = float3(0, 1, 0);
               float3 rightDir = normalize(cross(upDir, normalDir));
               upDir = normalize(cross(normalDir, rightDir));

               float3 centerOffs = vertex.xyz - center;
               float3 localPos = center - rightDir * centerOffs.x + upDir * centerOffs.y; // + normalDir * centerOffs.z;
               return localPos;
           }

           struct v2f
           {
               float2 uv : TEXCOORD0;
               float4 positionCS: SV_POSITION;
               //世界空间顶点
               float3 positionWS : TEXCOORD1;
               UNITY_VERTEX_INPUT_INSTANCE_ID
           };

           v2f vert(appdata input)
           {
               v2f output;
               UNITY_SETUP_INSTANCE_ID(input);
               UNITY_TRANSFER_INSTANCE_ID(input, output);
               float size = UNITY_ACCESS_INSTANCED_PROP(_Size_arr, _Size);
               input.positionOS.xy *= size;
               float3 positionOS = GetBillboard(input.positionOS.xyz);
               float3 positionOS2 = GetBillboardNoRot(input.positionOS.xyz);
               VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS.xyz);
               output.positionCS = vertexInput.positionCS;
               output.positionWS = TransformObjectToWorld(positionOS2);
               output.uv = input.uv;
               return output;
           }

           half4 frag(v2f input) : SV_Target
           {
               half4 color = tex2D(_ImpostorTex, input.uv);
               return color;
           }
           ENDHLSL
       }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}