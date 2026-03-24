using UnityEngine;

namespace Visvang.Core
{
    /// <summary>
    /// Provides reliable shader access that doesn't break in builds.
    /// Shader.Find("Standard") returns null when the shader is stripped from the build.
    /// This helper tries multiple fallbacks and caches the result.
    /// </summary>
    public static class ShaderHelper
    {
        private static Shader cachedOpaque;
        private static Shader cachedTransparent;
        private static Shader cachedParticle;

        /// <summary>
        /// Get a reliable opaque shader for 3D objects.
        /// </summary>
        public static Shader GetOpaque()
        {
            if (cachedOpaque != null) return cachedOpaque;

            cachedOpaque = TryFindShader(
                "Standard",
                "Legacy Shaders/Diffuse",
                "Mobile/Diffuse",
                "Sprites/Default",
                "UI/Default"
            );
            return cachedOpaque;
        }

        /// <summary>
        /// Get a reliable transparent shader for water/clouds.
        /// </summary>
        public static Shader GetTransparent()
        {
            if (cachedTransparent != null) return cachedTransparent;

            cachedTransparent = TryFindShader(
                "Standard",
                "Legacy Shaders/Transparent/Diffuse",
                "Mobile/Particles/Alpha Blended",
                "Sprites/Default",
                "UI/Default"
            );
            return cachedTransparent;
        }

        /// <summary>
        /// Get a reliable particle shader.
        /// </summary>
        public static Shader GetParticle()
        {
            if (cachedParticle != null) return cachedParticle;

            cachedParticle = TryFindShader(
                "Particles/Standard Unlit",
                "Legacy Shaders/Particles/Alpha Blended",
                "Mobile/Particles/Alpha Blended",
                "Sprites/Default"
            );
            return cachedParticle;
        }

        /// <summary>
        /// Create a material with a solid color that works in any build.
        /// </summary>
        public static Material CreateOpaqueMaterial(Color color)
        {
            var shader = GetOpaque();
            if (shader == null)
            {
                Debug.LogError("[ShaderHelper] No shader found at all!");
                return new Material(Shader.Find("Hidden/InternalErrorShader"));
            }

            var mat = new Material(shader);
            mat.color = color;
            return mat;
        }

        /// <summary>
        /// Create a transparent material that works in any build.
        /// </summary>
        public static Material CreateTransparentMaterial(Color color)
        {
            var shader = GetTransparent();
            if (shader == null) return CreateOpaqueMaterial(color);

            var mat = new Material(shader);
            mat.color = color;

            // Try to enable transparency (Standard shader mode)
            if (mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = 3000;
            }

            return mat;
        }

        private static Shader TryFindShader(params string[] names)
        {
            foreach (var name in names)
            {
                var shader = Shader.Find(name);
                if (shader != null)
                {
                    Debug.Log($"[ShaderHelper] Using shader: {name}");
                    return shader;
                }
            }
            Debug.LogWarning("[ShaderHelper] No suitable shader found!");
            return null;
        }
    }
}
