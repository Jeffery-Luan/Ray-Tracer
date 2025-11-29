using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            const double EPS = 1e-4;

            Vector3 e1 = this.v1 - this.v0;
            Vector3 e2 = this.v2 - this.v0;

            Vector3 pvec = ray.Direction.Cross(e2);
            double det = e1.Dot(pvec);
            if (Math.Abs(det) < EPS) return null;

            double invDet = 1.0 / det;
            Vector3 tvec = ray.Origin - this.v0;

            double u = tvec.Dot(pvec) * invDet;
            if (u < 0 || u > 1) return null;

            Vector3 qvec = tvec.Cross(e1);
            double v = ray.Direction.Dot(qvec) * invDet;
            if (v < 0 || u + v > 1) return null;

            double t = e2.Dot(qvec) * invDet;
            if (t <= EPS) return null;

            Vector3 p = ray.Origin + ray.Direction * t;
            Vector3 n = e1.Cross(e2).Normalized();
            // if (n.Dot(ray.Direction) > 0) n = -n;

            return new RayHit(p, n, ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}
