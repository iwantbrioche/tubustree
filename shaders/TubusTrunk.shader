Shader "Tubus/TubusTrunk"
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
                    float f = 1 - min(distance(i.uv.xy, float2(0.5, 0.5)) * 2, 1);
                    float n = tex2D(_NoiseTex, i.uv.xy).x;
                    f -= pow(n, f * lerp(10, 200, i.clr.a));

                    float y = i.uv.y;
                    y += i.clr.a;
                    y /= sin(i.uv.x * 0.85 + 1.14) * 0.8;
                    y -= floor(y);
                    

                    float h = tex2D(_NoiseTex, float2(i.uv.x, y) * 2.5).x;
                    if (h < 0.5)
                        h = pow(h, 1) * 2 - 0.5;
                    else
                        h = pow(h, 0.25) * 2.5;

                    h -= sin(y * (i.clr.a * 20) * 3.14) * 1.5;
                    h += sin(i.uv.x) / 3.14;
                    h *= 0.4;

                    if (f <= 0) return float4(0, 0, 0, 0); 

                    if (_fogAmount > 0.0) 
                    {
                        i.clr.r += 0.1;
                        i.clr.r *= _fogAmount * 0.6;
                    }

                    float4 effectCol1 = tex2D(_PalTex, float2(30.5/32.0, 5.5/8.0));
                    float4 fogCol = tex2D(_PalTex, float2(1.5/32.0, 7.5/8.0));



                    float4 trunkCol = float4(lerp(float3(0.58, 0.37, 0.23), effectCol1, 0.3 * n), 1);
                    if (i.clr.r < 0.0) trunkCol = lerp(trunkCol, fogCol, clamp(abs(i.clr.r), 0.2, 0.7));
                    trunkCol = lerp(trunkCol, tex2D(_PalTex, float2(lerp(0.5, 26.5, clamp(f * 0.8, 0.0, 1.0))/32.0, 2.5/8.0)), 0.5);

                    float4 lineCol = float4(lerp(trunkCol, lerp(tex2D(_PalTex, float2(2.5/32.0, 0.5/8.0)), effectCol1, n * 0.4), -h + 0.8).rgb + 0.01, 1.0);
                    if (i.clr.r < 0.0) lineCol = lerp(lineCol, fogCol, clamp(abs(i.clr.r), 0.2, 0.7));

                    trunkCol = lerp(tex2D(_PalTex, float2(2.5/32.0, 0.5/8.0)), trunkCol, tex2D(_PalTex, float2(29.5/32.0, 0.5/8.0)) + 0.3);
                    lineCol = lerp(tex2D(_PalTex, float2(2.5/32.0, 0.5/8.0)), lineCol, tex2D(_PalTex, float2(29.5/32.0, 0.5/8.0)) + 0.3);
                    
                    if (h < 0.65) return float4(lineCol.rgb, 1.0);

                    return float4(trunkCol.rgb, 1.0);
                }
                ENDCG
            }
        }
    }
}
