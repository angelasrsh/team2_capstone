Shader "Custom/Platform_GradientFade"
{
    Properties
    {
        _Color ("Top Color", Color) = (1,1,1,1)
        _CenterHeight ("Center Height", Float) = 0.0
        _GradientRange ("Gradient Range", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float height : TEXCOORD0;
            };

            fixed4 _Color;
            float _CenterHeight;
            float _GradientRange;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.height = v.vertex.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize height into 0–1 gradient
                float t = saturate((i.height - _CenterHeight) / _GradientRange);

                // Fully opaque at top, fades to 0 alpha at bottom
                fixed4 col = _Color;
                col.a = t;

                return col;
            }
            ENDCG
        }
    }
}
