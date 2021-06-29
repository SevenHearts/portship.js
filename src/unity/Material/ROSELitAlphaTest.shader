Shader "ROSE/Lit Alpha Test"
{
    Properties
    {
        [PerRendererData]_Color ("Color", Color) = (1,1,1,1)
        [PerRendererData]_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 2
		_Cutoff ("Base Alpha cutoff", Range (0,.9)) = .5
    }

    SubShader {
		Tags { "Queue" = "AlphaTest" /*"IgnoreProjector" = "True"*/ "RenderType" = "TransparentCutout" }

		// Render both front and back facing polygons.
		Cull Off

        // Set up basic lighting
        Material {
            Diffuse [_Color]
            Ambient [_Color]
        }

        Lighting On

        // first pass:
        // render any pixels that are more than [_Cutoff] opaque
        Pass {
            AlphaTest Greater [_Cutoff]
            SetTexture [_MainTex] {
                combine texture * primary, texture
            }
        }

        // Second pass:
        // render in the semitransparent details.
		Pass {
            // Dont write to the depth buffer
            ZWrite [_ZWrite]
            // Don't write pixels we have already written.
            ZTest [_ZTest]
            // Only render pixels less or equal to the value
            AlphaTest LEqual [_Cutoff]

            // Set up alpha blending
            Blend SrcAlpha OneMinusSrcAlpha

            SetTexture [_MainTex] {
                combine texture * primary, texture
            }
        }
    }
}
