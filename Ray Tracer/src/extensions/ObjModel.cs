using System;
using System.IO;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// </summary>
    public class ObjModel : SceneEntity
    {
        // === Public material property ===
        private readonly Material material;
        public Material Material => this.material;

        // === Geometry storage (all transformed into world space) ===
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector3> normals = new List<Vector3>();
        // Face definition: (v1,v2,v3,n1,n2,n3); if no vn, corresponding n* is -1
        private readonly List<(int v1, int v2, int v3, int n1, int n2, int n3)> faces
            = new List<(int, int, int, int, int, int)>();

        // === Pre-built triangles in world space ===
        private readonly List<Triangle> triangles = new List<Triangle>();

        // === Bounding volumes (AABB + bounding sphere, both in world space) ===
        private Vector3 aabbMin, aabbMax;
        private Vector3 bsCenter;
        private double bsRadius;

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objPath">File path of .obj</param>
        /// <param name="transform">Transform to apply to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objPath, Transform transform, Material material)
        {
            this.material = material ?? throw new ArgumentNullException(nameof(material));
            if (string.IsNullOrWhiteSpace(objPath))
                throw new ArgumentException("objPath is null or empty.", nameof(objPath));
            if (!File.Exists(objPath))
                throw new FileNotFoundException("OBJ file not found", objPath);

            // Read all lines from the OBJ file
            string[] lines = File.ReadAllLines(objPath);

            // Temporary storage in local space (before transform is applied)
            var positionsLocal = new List<Vector3>();
            var normalsLocal = new List<Vector3>();

            // === Parse file line by line ===
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i];
                if (string.IsNullOrWhiteSpace(raw)) continue;

                string line = raw.Trim();
                if (line.StartsWith("#")) continue; // Skip comments

                // Vertex: v x y z
                if (line.StartsWith("v "))
                {
                    var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    double x = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    double y = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                    double z = parts.Length > 3 ? double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture) : 0.0;
                    positionsLocal.Add(new Vector3(x, y, z));
                    continue;
                }

                // Normal: vn x y z
                if (line.StartsWith("vn "))
                {
                    var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    double x = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    double y = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                    double z = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                    normalsLocal.Add(new Vector3(x, y, z).Normalized());
                    continue;
                }

                // Face: f v1/... v2/... v3/...   Supports formats v, v//n, v/vt, v/vt/n
                if (line.StartsWith("f "))
                {
                    var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4) continue; // Require at least a triangle

                    int[] vIdx = new int[3];
                    int[] nIdx = new int[3] { -1, -1, -1 };

                    for (int k = 0; k < 3; k++)
                    {
                        var token = parts[k + 1];
                        var comps = token.Split('/');

                        // Vertex index (.obj starts from 1, convert to 0-based)
                        vIdx[k] = int.Parse(comps[0], System.Globalization.CultureInfo.InvariantCulture) - 1;

                        // Normal index (optional)
                        if (comps.Length == 3 && !string.IsNullOrEmpty(comps[2]))
                            nIdx[k] = int.Parse(comps[2], System.Globalization.CultureInfo.InvariantCulture) - 1;
                        else if (comps.Length == 2 && token.Contains("//"))
                        {
                            // Handle "v//n" case
                            var nn = comps[1];
                            if (!string.IsNullOrEmpty(nn))
                                nIdx[k] = int.Parse(nn, System.Globalization.CultureInfo.InvariantCulture) - 1;
                        }
                    }

                    // Store face (note: winding order reversed here to match conventions)
                    //faces.Add((vIdx[2], vIdx[1], vIdx[0], nIdx[2], nIdx[1], nIdx[0]));
                    // Store face with original winding from the OBJ file (do NOT reverse)
                    faces.Add((vIdx[0], vIdx[1], vIdx[2], nIdx[0], nIdx[1], nIdx[2]));
                }
            }

            // === Apply transform to local vertices → world space ===
            for (int i = 0; i < positionsLocal.Count; i++)
            {
                Vector3 pw = transform.Apply(positionsLocal[i]);
                vertices.Add(pw);
            }

            // Optionally transform normals into world space (simplified: only rotation)
            if (normalsLocal.Count > 0)
            {
                for (int i = 0; i < normalsLocal.Count; i++)
                    normals.Add(normalsLocal[i]);
            }

            // === Pre-build world-space triangles ===
            triangles.Clear();
            foreach (var f in faces)
            {
                Vector3 v0 = vertices[f.v1];
                Vector3 v1 = vertices[f.v2];
                Vector3 v2 = vertices[f.v3];
                triangles.Add(new Triangle(v0, v1, v2, this.material));
            }

            // === Compute bounding box (AABB) and bounding sphere ===
            if (vertices.Count > 0)
            {
                aabbMin = aabbMax = vertices[0];
                for (int i = 1; i < vertices.Count; i++)
                {
                    var v = vertices[i];
                    aabbMin = new Vector3(Math.Min(aabbMin.X, v.X),
                                          Math.Min(aabbMin.Y, v.Y),
                                          Math.Min(aabbMin.Z, v.Z));
                    aabbMax = new Vector3(Math.Max(aabbMax.X, v.X),
                                          Math.Max(aabbMax.Y, v.Y),
                                          Math.Max(aabbMax.Z, v.Z));
                }
                bsCenter = (aabbMin + aabbMax) * 0.5;
                bsRadius = 0.0;
                for (int i = 0; i < vertices.Count; i++)
                {
                    double d = (vertices[i] - bsCenter).Length();
                    if (d > bsRadius) bsRadius = d;
                }
            }
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// Performs a quick bounding test (AABB/sphere) before
        /// checking detailed triangle intersections.
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            // Fast rejection using AABB
            if (this.vertices.Count > 0)
            {
                bool hitAabb = IntersectsAABB(ray, aabbMin, aabbMax);
                if (!hitAabb)
                    return null;
            }

            RayHit closestHit = null;
            double bestT = double.PositiveInfinity;
            Vector3 Dn = ray.Direction.Normalized();

            // Traverse pre-built triangles
            for (int i = 0; i < triangles.Count; i++)
            {
                var h = triangles[i].Intersect(ray);
                if (h == null) continue;

                double tCand = (h.Position - ray.Origin).Length();
                if (tCand > 1e-6 && tCand < bestT)
                {
                    bestT = tCand;
                    closestHit = h;
                }
            }

            return closestHit;
        }

        // === Helper: Ray-sphere intersection (for fast rejection) ===
        private static bool IntersectsSphere(Ray ray, Vector3 c, double r)
        {
            Vector3 d = ray.Direction.Normalized();
            Vector3 oc = ray.Origin - c;
            double tca = -oc.Dot(d);
            if (tca < 0) return false;
            double d2 = oc.Dot(oc) - tca * tca;
            return d2 <= r * r;
        }

        // === Helper: Ray-AABB intersection using slab method ===
        private static bool IntersectsAABB(Ray ray, Vector3 mn, Vector3 mx)
        {
            Vector3 o = ray.Origin;
            Vector3 d = ray.Direction;

            double invDx = Math.Abs(d.X) < 1e-12 ? 1e12 : 1.0 / d.X;
            double invDy = Math.Abs(d.Y) < 1e-12 ? 1e12 : 1.0 / d.Y;
            double invDz = Math.Abs(d.Z) < 1e-12 ? 1e12 : 1.0 / d.Z;

            double t1 = (mn.X - o.X) * invDx, t2 = (mx.X - o.X) * invDx;
            double tmin = Math.Min(t1, t2), tmax = Math.Max(t1, t2);

            t1 = (mn.Y - o.Y) * invDy; t2 = (mx.Y - o.Y) * invDy;
            tmin = Math.Max(tmin, Math.Min(t1, t2));
            tmax = Math.Min(tmax, Math.Max(t1, t2));

            t1 = (mn.Z - o.Z) * invDz; t2 = (mx.Z - o.Z) * invDz;
            tmin = Math.Max(tmin, Math.Min(t1, t2));
            tmax = Math.Min(tmax, Math.Max(t1, t2));

            return tmax > Math.Max(tmin, 1e-6);
        }
        
        // Apply per-frame delta (world-space): translate, then rotate about model's current center
        public void ApplySimpleDelta(Vector3 translationPerFrame, Quaternion rotationPerFrame)
        {
            if (this.vertices.Count == 0) return;

            // pivot: current bounding-sphere center
            Vector3 pivot = this.bsCenter;

            // 1) Transform the vertex (first move the point to the local area with 
            // the pivot as the origin, then rotate it, then move it back + translate it)
            for (int i = 0; i < this.vertices.Count; i++)
            {
                Vector3 v = this.vertices[i];
                Vector3 vLocal = v - pivot;
                vLocal = rotationPerFrame.Rotate(vLocal);
                this.vertices[i] = vLocal + pivot + translationPerFrame;
            }

            // 2) Reconstruct the triangle 
            // (Triangle stores values ​​and will not be automatically updated with vertices)
            this.triangles.Clear();
            foreach (var f in this.faces)
            {
                Vector3 v0 = vertices[f.v1];
                Vector3 v1 = vertices[f.v2];
                Vector3 v2 = vertices[f.v3];
                this.triangles.Add(new Triangle(v0, v1, v2, this.material));
            }

            // 3) Recalculate AABB / bounding sphere
            if (this.vertices.Count > 0)
            {
                this.aabbMin = this.aabbMax = this.vertices[0];
                for (int i = 1; i < this.vertices.Count; i++)
                {
                    var v = this.vertices[i];
                    this.aabbMin = new Vector3(Math.Min(aabbMin.X, v.X),
                                            Math.Min(aabbMin.Y, v.Y),
                                            Math.Min(aabbMin.Z, v.Z));
                    this.aabbMax = new Vector3(Math.Max(aabbMax.X, v.X),
                                            Math.Max(aabbMax.Y, v.Y),
                                            Math.Max(aabbMax.Z, v.Z));
                }
                this.bsCenter = (this.aabbMin + this.aabbMax) * 0.5;
                this.bsRadius = 0.0;
                for (int i = 0; i < this.vertices.Count; i++)
                {
                    double d = (this.vertices[i] - this.bsCenter).Length();
                    if (d > this.bsRadius) this.bsRadius = d;
                }
            }
        }

    }
}
