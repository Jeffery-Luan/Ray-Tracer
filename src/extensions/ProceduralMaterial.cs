
using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a texture material that is associated with an entity
    /// that is to be rendered by the ray tracer. This material generates its
    /// texture procedurally based on defined patterns, such as stripes or
    /// checkers, rather than using an image file. 
    /// </summary>
    public class ProceduralMaterial : Material
    {
        public enum PatternType
        {
            Checkers,
            Stripes,
        }

        private PatternType pattern;
        private double scaleU;
        private double scaleV;
        private Color color1;
        private Color color2;

        /// <summary>
        /// Construct a new material object.
        /// </summary>
        /// <param name="pattern">The pattern of the material</param>
        /// <param name="scaleU">Scale factor in the U direction</param>
        /// <param name="scaleV">Scale factor in the V direction</param>
        /// <param name="color1">First color of the pattern</param>
        /// <param name="color2">Second color of the pattern</param>
        public ProceduralMaterial(PatternType pattern, double scaleU, double scaleV, Color color1, Color color2)
            : base(new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), 0, 0, 0, 1)
        {
            this.pattern = pattern;
            this.scaleU = scaleU;
            this.scaleV = scaleV;
            this.color1 = color1;
            this.color2 = color2;
        }
    }
}
