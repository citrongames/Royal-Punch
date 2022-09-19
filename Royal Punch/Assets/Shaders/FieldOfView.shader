Shader "Unlit/FieldOfView"
{
    Properties
    {
        _Color ("Color", color) = (1, 0, 0, 0)
		_NearPlane("NearPlane", range(0, 1)) = 0.1
		_FarPlane("FarPlane", range(0, 1)) = 1
		_FieldOfView("Field Of View (grad)", range(0, 360)) = 45
		_ColorOverlap("ColorOverlap", range(0, 1)) = 0
    }

    SubShader
    {
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			float4 _Color;
			float _NearPlane;
			float _FarPlane;
			float _FieldOfView;
			float _ColorOverlap;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//get vector relative to center
				float2 dir = i.uv - float2(0.5, 0.5);
				//get normalize distance from centre (from 0 to 1)
				float distanceFromCentre = length(dir) * 2;
				//if distance out of range - return transparent color
				if (distanceFromCentre < _NearPlane || distanceFromCentre > _FarPlane)
					return float4(0, 0, 0, 0);
				//calc angle
				float angle = atan2(dir.y, dir.x) * 180 / 3.14159;
				//if angle out of range - return transparent color
				if (abs(angle) > _FieldOfView / 2)
					return float4(0, 0, 0, 0);
				//normalize intensity from _NearPlane to _FarPlane
				float intensity = smoothstep(_NearPlane, _FarPlane, distanceFromCentre);
				//multiple base color on intensity (with some overlapping)
                return _Color * saturate(intensity + _ColorOverlap);
            }
            ENDCG
        }
    }
}
