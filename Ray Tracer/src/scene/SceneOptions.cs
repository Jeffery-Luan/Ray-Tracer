using System;

namespace RayTracer
{
    /// <summary>
    /// Immutable structure representing various scene options.
    /// You may not utilise all of these variables - this will depend
    /// on which add-ons you choose to implement.
    /// </summary>
    public readonly struct SceneOptions
    {
        private readonly int aaMultiplier;
        private readonly bool ambientLightingEnabled;
        private readonly double apertureRadius, focalLength;
        private readonly int quality;
        private readonly int frameCount;
        private readonly int framesPerSecond;

        /// <summary>
        /// Construct scene options object.
        /// </summary>
        /// <param name="aaMultiplier">Anti-aliasing multiplier</param>
        /// <param name="ambientLightingEnabled">Flag to enable ambient lighting</param>
        /// <param name="apertureRadius">Physical camera aperture radius</param>
        /// <param name="focalLength">Physical camera focal length</param>
        /// <param name="quality">Quality of the image render</param>
        /// <param name="frameCount">Number of frames to render (for animations)</param>
        /// <param name="framesPerSecond">Frames per second for the animation</param>
        public SceneOptions(
            int aaMultiplier,
            bool ambientLightingEnabled,
            double apertureRadius,
            double focalLength,
            int quality,
            int frameCount,
            int framesPerSecond)
        {
            this.aaMultiplier = aaMultiplier;
            this.ambientLightingEnabled = ambientLightingEnabled;
            this.apertureRadius = apertureRadius;
            this.focalLength = focalLength;
            this.quality = quality;
            this.frameCount = frameCount;
            this.framesPerSecond = framesPerSecond;
        }

        /// <summary>
        /// Anti-aliasing multiplier. Specifies how many samples per pixel in 
        /// each axis. e.g. 2 => 4 samples, 3 => 9 samples, etc.
        /// </summary>
        public int AAMultiplier { get { return this.aaMultiplier; } }

        /// <summary>
        /// Whether ambient lighting computation should be enabled in the scene.
        /// </summary>
        public bool AmbientLightingEnabled { get { return this.ambientLightingEnabled; } }

        /// <summary>
        /// Aperture radius for simulating physical camera depth of field effects.
        /// </summary>
        public double ApertureRadius { get { return this.apertureRadius; } }

        /// <summary>
        /// Focal length for simulating physical camera depth of field effects.
        /// </summary>
        public double FocalLength { get { return this.focalLength; } }

        /// <summary>
        /// Quality of the render (stage 3). See the specification for details.
        /// </summary>
        public int Quality { get { return this.quality; } }

        /// <summary>
        /// Number of frames to render in the scene (stage 3, Option C).
        /// </summary>
        public int FrameCount { get { return this.frameCount; } }

        /// <summary>
        /// Frames per second for the animation (stage 3, Option C).
        /// </summary>
        public int FramesPerSecond { get { return this.framesPerSecond; } }
    }
}
