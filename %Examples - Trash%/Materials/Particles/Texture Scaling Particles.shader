Shader "Example/Texture Scaling Particles"
{
    Properties 
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        _MainTex ("Texture", 2D) = "white" {}
        
		_WOffset("W Offset", Vector) = (3, 1, 0, 0)

        _Offset ("Detail", 2D) = "bump" {}
        _OffsetAmount ("Offset Amount", Float) = 1
        
        _Detail ("Detail", 2D) = "white" {}
        _DetailAmount ("Detail Amount", Float) = 1
    }
    SubShader 
    {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		//Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		Cull Off
		ZTest On
		//ZWrite Off
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 uv : TEXCOORD0;
			};

			sampler2D _MainTex;

			sampler2D _Detail;
			float4 _Detail_ST;
			float _DetailAmount;

			sampler2D _Offset;
			float4 _Offset_ST;
			float _OffsetAmount;

			float4 _Color;

			float2 _WOffset;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.uv.xy;

				float2 uvTiled = (uv - 0.5) * i.uv.z + _WOffset * i.uv.w;

				float3 detail = (1.0 + (tex2D(_Detail, uvTiled * _Detail_ST.xy).rgb - 0.5) * _DetailAmount);
				float2 off = UnpackNormal(tex2D(_Offset, uvTiled * _Offset_ST.xy)).xy * _OffsetAmount;
				float4 c = tex2D(_MainTex, uv + off);
				clip(c.a - 0.5);
				c *= _Color * i.color;
				c.rgb *= detail;

				return c;
			}
			ENDCG
		}
    } 
}