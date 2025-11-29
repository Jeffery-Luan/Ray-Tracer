using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            const double EPS = 1e-4;

            Vector3 oc = ray.Origin - this.center;
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2.0 * oc.Dot(ray.Direction);
            double c = oc.Dot(oc) - this.radius * this.radius;

            double disc = b * b - 4 * a * c;
            if (disc < 0) return null;

            double s = Math.Sqrt(disc);
            double inv2a = 0.5 / a;
            double t0 = (-b - s) * inv2a;
            double t1 = (-b + s) * inv2a;

            double t = (t0 > EPS) ? t0 : (t1 > EPS ? t1 : -1);
            if (t < 0) return null;

            Vector3 p = ray.Origin + ray.Direction * t;
            Vector3 n = (p - this.center).Normalized();
            // if (n.Dot(ray.Direction) > 0) n = -n;

            return new RayHit(p, n, ray.Direction, this.Material);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
