Shader "TextMeshPro/Dissolve"
{
    Properties
    {
        _FaceTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.0
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _DissolveTex ("Dissolve Noise", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (1,0.5,0,1)
        _EdgeWidth ("Edge Width", Range(0,0.2)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FaceTex;
            float4 _FaceTex_ST;
            float4 _FaceColor;
            float4 _OutlineColor;
            float _OutlineWidth;
            sampler2D _DissolveTex;
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _FaceTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_FaceTex, uv) * _FaceColor;
                float sdf = col.a;

                // Outline
                float outline = smoothstep(0.5 - _OutlineWidth, 0.5 + _OutlineWidth, sdf);
                col.rgb = lerp(_OutlineColor.rgb, col.rgb, outline);

                // Dissolve
                float dissolve = tex2D(_DissolveTex, uv).r;
                float edge = smoothstep(_DissolveAmount, _DissolveAmount + _EdgeWidth, dissolve);
                float alpha = col.a * (1 - step(dissolve, _DissolveAmount));
                col.rgb = lerp(_EdgeColor.rgb, col.rgb, edge);
                col.a = alpha;

                return col;
            }
            ENDCG
        }
    }
}