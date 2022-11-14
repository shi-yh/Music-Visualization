Shader "Unlit/Glow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color",color) = (1,1,1,1)
        _Glow("Intensity",Range(0,10)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "IgnoreProjector" = "True"
            "RenderType" ="Transparent"
        }

        LOD 100
        Cull Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                ///在顶点着色器输入输出结构中使用它来定义实例id
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                ///在结构体中注册一下ID
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            ///必须在专门命名的常量缓冲区中定义各个实例属性
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(half, _Glow)
            UNITY_INSTANCING_BUFFER_END(Props)


            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;

                ///使用此选项可使着色器函数可访问实例ID,它必须在顶点着色器最开始的时候使用
                UNITY_SETUP_INSTANCE_ID(v);

                ///使用此选项可以将实例ID 从输入结构复制到顶点着色器的输出结构
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i)
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                col *= UNITY_ACCESS_INSTANCED_PROP(Props, _Glow);
                return col;
            }
            ENDCG
        }
    }
}