Shader "Axolotl/AlphaModulateShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Alpha ("Alpha", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
        //This allows for transparency of objects in world
        Blend SrcAlpha OneMinusSrcAlpha   

        Pass{
            Fog { Mode Off }
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag 
                #pragma multi_compile_instancing
                #include "UnityCG.cginc"
                #pragma target 3.0


                sampler2D _MainTex;
                fixed4 _Color;

                struct appdata 
                {
                    float4 vertex : POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                // Adds instancing support for this shader. Requires 'Enable Instancing' on materials that use the shader.
                // See https://docs.unity3d.com/Manual/GPUInstancing.html
                #pragma instancing_options assumeuniformscaling
                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(half, _Alpha)
                UNITY_INSTANCING_BUFFER_END(Props)

                v2f vert(appdata v, float2 uv : TEXCOORD0)
                {
                    UNITY_SETUP_INSTANCE_ID(v);
                    v2f o;
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    
                    
                    
                    o.uv = uv;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    return o;
                }



                fixed4 frag(v2f i) : SV_TARGET
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    fixed4 c = tex2D(_MainTex, i.uv) * _Color;

                    //Big disgusting to avoid if statement
                    float instAlpha = UNITY_ACCESS_INSTANCED_PROP(Props, _Alpha);
                    float checker = (float) ((instAlpha == 0) * -1.0f);
                    clip(checker);
                    //End of big disgusting

                    return fixed4(
                                c.r, c.g, c.b, 
                                c.a * instAlpha
                            );
                }
            ENDCG
        }
    }
    //FallBack "Diffuse"
}
