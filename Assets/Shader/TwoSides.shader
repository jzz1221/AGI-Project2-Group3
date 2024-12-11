Shader "Custom/DoubleSidedURP" {
    Properties {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Base Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200

        // 关闭背面剔除，实现双面渲染
        Cull Off

        Pass {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 包含URP渲染功能的头文件
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 属性
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _BaseColor;

            struct Attributes {
                float4 positionOS : POSITION; // 顶点位置
                float2 uv : TEXCOORD0;        // UV坐标
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION; // 裁剪空间位置
                float2 uv : TEXCOORD0;           // 插值后的UV坐标
            };

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS); // 对象空间到裁剪空间
                OUT.uv = IN.uv; // 传递UV
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                // 采样纹理并乘以颜色
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return texColor * _BaseColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
