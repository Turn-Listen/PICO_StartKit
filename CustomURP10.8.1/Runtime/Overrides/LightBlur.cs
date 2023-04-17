using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/LightBlur")]
    public sealed class LightBlur : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("down sample rendertexture scale")]
        public ClampedIntParameter passDownsample = new ClampedIntParameter(10, 1, 10);

        [Tooltip("Pass Loop")]
        public ClampedIntParameter passLoop = new ClampedIntParameter(3, 1, 10);

        [Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
        public MinFloatParameter passBlur = new MinFloatParameter(0.75f, 0.0f);

        [Tooltip("Strength of the lightBlur filter.")]
        public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);

        public bool IsActive() => intensity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}
