using System;

namespace RayTracer
{
    /// <summary>
    /// Immutable structure to represent a two-dimensional texture coordinate.
    /// </summary>
    public readonly struct TextureCoord
    {
        private readonly double u, v;

        /// <summary>
        /// Construct a two-dimensional texture coordinate.
        /// </summary>
        /// <param name="u">U coordinate</param>
        /// <param name="v">V coordinate</param>
        public TextureCoord(double u, double v)
        {
            this.u = u;
            this.v = v;
        }

        /// <summary>
        /// Convert texture coordinate to a readable string.
        /// </summary>
        /// <returns>Texture coordinate as string in form (u, v)</returns>
        public override string ToString()
        {
            return "(" + this.u + "," + this.v + ")";
        }

        /// <summary>
        /// U component of the texture coordinate.
        /// </summary>
        public double U { get { return this.u; } }

        /// <summary>
        /// V component of the texture coordinate.
        /// </summary>
        public double V { get { return this.v; } }

        /// <summary>
        /// Add two texture coordinates together.
        /// </summary>
        /// <param name="a">First texture coordinate</param>
        /// <param name="b">Second texture coordinate</param>
        /// <returns>Sum of the two texture coordinates</returns>
        public static TextureCoord operator +(TextureCoord a, TextureCoord b)
        {
            return new TextureCoord(a.u + b.u, a.v + b.v);
        }

        /// <summary>
        /// Multiply a texture coordinate by a scalar value.
        /// </summary>
        /// <param name="a">Original vector</param>
        /// <param name="b">Scalar multiplier</param>
        /// <returns>Multiplied vector</returns>
        public static TextureCoord operator *(TextureCoord a, double b)
        {
            return new TextureCoord(a.u * b, a.v * b);
        }

        /// <summary>
        /// Multiply a texture coordinate by a scalar value (opposite operands).
        /// </summary>
        /// <param name="b">Scalar multiplier</param>
        /// <param name="a">Original vector</param>
        /// <returns>Multiplied vector</returns>
        public static TextureCoord operator *(double b, TextureCoord a)
        {
            return a * b;
        }
    }
}
