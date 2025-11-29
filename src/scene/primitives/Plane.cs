using System;
using System.Numerics;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Plane : SceneEntity
    {
        private Vector3 center;
        private Vector3 normal;
        private Material material;

        /// <summary>
        /// Construct an infinite plane object.
        /// </summary>
        /// <param name="center">Position of the center of the plane</param>
        /// <param name="normal">Direction that the plane faces</param>
        /// <param name="material">Material assigned to the plane</param>
        public Plane(Vector3 center, Vector3 normal, Material material)
        {
            this.center = center;
            this.normal = normal.Normalized();
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the plane, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            const double EPS = 1e-6;

            // dot product of the ray and the normal, 0 -> parallel
            double denom = ray.Direction.Dot(this.normal);
            if (Math.Abs(denom) < EPS) return null;

            double t = (this.center - ray.Origin).Dot(this.normal) / denom;
            if (t <= EPS) return null;  // too close or behind

            // hit point and normal
            Vector3 p = ray.Origin + ray.Direction * t;
            Vector3 n = this.normal;  // already Nornalized when construct 

            // unified normal facing outward
            // if (n.Dot(ray.Direction) > 0) n = -n;

            return new RayHit(p, n, ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the plane.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
