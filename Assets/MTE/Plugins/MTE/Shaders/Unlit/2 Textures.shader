﻿Shader "MTE/Unlit/2 Textures (realtime shadow, no fog)"
{
    Properties
    {
        _Control("Control (RGBA)", 2D) = "red" {}
        _Splat0("Layer 1", 2D) = "white" {}
        _Splat1("Layer 2", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}
        Pass
        {
            Lighting Off
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase
                #pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF
                #pragma multi_compile_fog

                #include "UnityCG.cginc"
                #include "AutoLight.cginc"

                sampler2D _Control;
                float4 _Control_ST;
                sampler2D _Splat0,_Splat1;
                float4 _Splat0_ST,_Splat1_ST;

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float4 tc_Control : TEXCOORD0;
                    float4 tc_Splat01 : TEXCOORD1;
			        UNITY_FOG_COORDS(2)
                };

                struct appdata_lightmap
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                    float2 fogCoord : TEXCOORD2;
                };

                v2f vert(appdata_lightmap v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.tc_Control.xy = TRANSFORM_TEX(v.texcoord, _Control);
                    o.tc_Control.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;//save lightmap uv in tc_Control.zw
                    o.tc_Splat01.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
                    o.tc_Splat01.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
			        UNITY_TRANSFER_FOG(o, o.pos);
                    return o;
                }

                fixed4 frag(v2f i) : COLOR
                {
                    half4 splat_control = tex2D(_Control, i.tc_Control.xy);
                    half weight = dot(splat_control, half4(1, 1, 1, 1));
                    splat_control /= (weight + 1e-3f);

                    fixed4 mixedDiffuse = 0.0f;
                    mixedDiffuse += splat_control.r * tex2D(_Splat0, i.tc_Splat01.xy);
                    mixedDiffuse += splat_control.g * tex2D(_Splat1, i.tc_Splat01.zw);
                    fixed4 color = mixedDiffuse;

                    //apply light-map
                    color.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.tc_Control.zw));

                    //apply fog
	                UNITY_APPLY_FOG(i.fogCoord, color);

                    return color;
                }
            ENDCG
        }
    }
    Fallback "Diffuse"
    CustomEditor "MTE.MTEShaderGUI"
}
