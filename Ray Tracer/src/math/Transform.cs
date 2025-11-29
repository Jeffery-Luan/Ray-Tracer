using System;

namespace RayTracer
{
    /// <summary>
    /// Structure to represent a transformation that can be applied to entities
    /// in the ray tracing scene. This includes translation, rotation, and scaling.
    /// </summary>
    public struct Transform
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public double Scale { get; private set; }

        public static Transform Identity => new Transform(Vector3.Zero, Quaternion.Identity, 1.0);

        /// <summary>
        /// Construct a new transformation with translation and default rotation and scale.
        /// </summary>
        /// <param name="translation">Translation vector</param>
        public Transform(Vector3 translation)
        {
            this.Position = translation;
            this.Rotation = Quaternion.Identity;
            this.Scale = 1.0;
        }

        /// <summary>
        /// Construct a new transformation with translation, rotation, and default scale.
        /// </summary>
        /// <param name="translation">Translation vector</param>
        /// <param name="rotation">Rotation quaternion</param>
        public Transform(Vector3 translation, Quaternion rotation)
        {
            this.Position = translation;
            this.Rotation = rotation;
            this.Scale = 1.0;
        }

        /// <summary>
        /// Construct a new transformation with translation, rotation, and scale.
        /// </summary>
        /// <param name="translation">Translation vector</param>
        /// <param name="rotation">Rotation quaternion</param>
        /// <param name="scale">Scale factor</param>
        /// 
        public Transform(Vector3 translation, Quaternion rotation, double scale)
        {
            this.Position = translation;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        /// <summary>
        /// Apply this transformation to a vector.
        /// </summary>
        /// <param name="v">Vector to transform</param>
        /// <returns>Transformed vector</returns>
        public Vector3 Apply(Vector3 v)
        {
            Vector3 scaled = new Vector3(
                v.X * Scale,
                v.Y * Scale,
                v.Z * Scale
            );
            Vector3 rotated = Rotation.Rotate(scaled);
            return new Vector3(
                rotated.X + Position.X,
                rotated.Y + Position.Y,
                rotated.Z + Position.Z
            );
        }
    }
}
