Shader "Backbenchers/ArtPaper" {
 Properties { _MainTex ("Drawing", 2D) = "white" {} _Color ("Room illumination", Color) = (1,1,1,1) }
 SubShader {
  Tags { "RenderType"="Opaque" }
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float3 normal:NORMAL; };
   struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 normal:TEXCOORD1; };
   sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color;
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.normal=UnityObjectToWorldNormal(v.normal);return o;}
   // Neutral paper reflectance avoids the grey cast of unbaked dynamic props.
   // _Color follows the room switch; there is no bloom or emissive light spill.
   fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv)*_Color;c.rgb*=.92+.08*saturate(normalize(i.normal).y);return c;}
   ENDCG
  }
 }
}
