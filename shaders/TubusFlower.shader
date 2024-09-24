Shader "Tubus/TubusFlower"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        //_NoiseTex ("Noise", 2D) = "white" {}
        //_Color ("Color", Vector) = (0.0, 0.0, 0.0, 0.0)
        //_PalTex ("Palette", 2D) = "white" {}
    }
    Category
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Color(0,0,0,0) }
		Lighting Off
		Cull Off 
    
        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                sampler2D _NoiseTex;
                sampler2D _PalTex;
                sampler2D _GrabTexture;
                float4 _MainTex_ST;
                //float4 _Color;
                uniform float4 _spriteRect;
                uniform float2 _screenOffset;
                uniform float4 _lightDirAndPixelSize;
                uniform float _fogAmount;

                struct v2f {
                    float4  pos : SV_POSITION;
                    float2  uv : TEXCOORD0;
                    float2 scrPos : TEXCOORD1;
                    float4 clr : COLOR;
                    //float4 clr : _Color;
                };


                v2f vert (appdata_full v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos (v.vertex);
                    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                    o.scrPos = ComputeScreenPos(o.pos);
                    o.clr = v.color;
                    return o;
                }

                float4 frag (v2f i) : SV_Target
                {
                    float f = 1 - min(distance(i.uv.y, float2(0.5, 0.5)), 0.75);
                    if (i.uv.y < 0.5) f = 1.0;
                    //float n = tex2D(_NoiseTex, i.uv.xy).x;
                    //f -= pow(n, f * lerp(10, 200, 1));
                    // f is what controls the circular shape

                    float4 tex = tex2D(_MainTex, i.uv);

                    if (tex.a == 0.0) return 0.0;
                    return lerp(1.0, i.clr, f);
                }
                ENDCG
            }
        }
    }
}
