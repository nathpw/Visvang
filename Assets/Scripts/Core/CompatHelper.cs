using UnityEngine;

namespace Visvang.Core
{
    /// <summary>
    /// Compatibility helpers for different Unity versions.
    /// FindFirstObjectByType requires Unity 2023+.
    /// This wrapper falls back to FindObjectOfType for older versions.
    /// </summary>
    public static class CompatHelper
    {
        public static T FindSingleton<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }
    }
}
