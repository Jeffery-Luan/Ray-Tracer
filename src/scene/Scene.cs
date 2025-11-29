using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private Camera camera;
        private Color ambientLightColor;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;
        private ISet<Animation> animations;

        private const int MAX_DEPTH = 5;
        private const double RAY_EPS = 1e-4;
        private const double KT_SCALE = 1.50;

        private Color ShadeLocal(RayHit hit, SceneEntity entity, Ray viewRay)
        {
            // 1) Local shading frame
            Vector3 P = hit.Position;                  // Hit position (world coords)
            Vector3 N = hit.Normal.Normalized();       // Normalized surface normal
            var m = entity.Material;                   // Material properties

            // Transmissivity adjustment
            // double ktMat = Math.Max(0.0, Math.Min(1.0, m.Transmissivity * KT_SCALE));
            // double localScale = 1.0 - ktMat;   // Glass kt≈1 → local≈0

            // 2) ambient
            Color result = new Color(0, 0, 0);
            if (this.options.AmbientLightingEnabled)
            {
                result += (m.AmbientColor * this.ambientLightColor); 
                //result += (m.AmbientColor * this.ambientLightColor) * localScale;

            }

            // 3) View direction (towards camera)
            Vector3 V = (-viewRay.Direction).Normalized();

            // 4) For each light: diffuse + specular
            foreach (var Lgt in this.lights)
            {
                // Hit point to light vector (world space)
                Vector3 toL = Lgt.Position - P;
                double distL = toL.Length();
                //Ray shadowRay = new Ray(P + N * EPS, dirL);

                if (distL <= 1e-8) continue;

                // Unit direction
                // Vector3 dirL = toL.Normalized();
                // Vector3 L = (Lgt.Position - P).Normalized();
                // Vector3 dirL = toL / distL;  // = (Lgt.Position - P).Normalized()
                // Vector3 L = dirL;
                Vector3 L = toL / distL;

                // Backlight early screening
                double ndotl = N.Dot(L);
                if (ndotl <= 0.0) continue;  // Skip the backlight
                /*
                double ndotlRaw = N.Dot(L);
                if (ndotlRaw <= 0.0)
                    continue;
                double ndotl = ndotlRaw;
                */


                // Hard shadow detection (Starting point offset + distance judgment)
                const double EPS = RAY_EPS;
                
                Ray shadowRay = new Ray(P + N * EPS, L);
                bool blocked = false;

                foreach (var ent in this.entities)
                {
                    if (ReferenceEquals(ent, entity)) continue;  // avoid self-shadowing
                    var sh = ent.Intersect(shadowRay);
                    if (sh == null) continue;
                    double t = (sh.Position - shadowRay.Origin).Length();
                    if (t > EPS && t < distL - EPS) { blocked = true; break; }
                }
                if (blocked) continue;

                // Diffuse
                //double ndotl = Math.Max(0.0, N.Dot(L));
                //Color diffuse = m.DiffuseColor * Lgt.Color * (Math.Max(0.0, ndotl) * localScale);
                Color diffuse = m.DiffuseColor * Lgt.Color * Math.Max(0.0, ndotl);

                // Specular
                Vector3 R = Reflect(-L, N);
                double rdotv = Math.Max(0.0, R.Dot(V));
                Color spec = m.SpecularColor * Lgt.Color * Math.Pow(rdotv, m.Shininess);

                result += diffuse + spec;
            }

            return result;
        }

        // Reflect vector I around unit normal N
        private static Vector3 Reflect(Vector3 I, Vector3 N)
        {
            return I - 2.0 * I.Dot(N) * N;
        }

        // Recursive ray tracing: local + reflection + refraction
        private Color Trace(Ray ray, int depth)
        {
            // Find closest hit
            double closestT = double.PositiveInfinity;
            SceneEntity hitEntity = null;
            RayHit bestHit = null;

            foreach (var e in this.entities)
            {
                RayHit h = e.Intersect(ray);
                if (h == null) continue;
                double tCandidate = (h.Position - ray.Origin).Length();
                
                if (tCandidate > 1e-6 && tCandidate < closestT)
                {
                    closestT = tCandidate;
                    hitEntity = e;
                    bestHit = h;
                }
            }

            // No hit → background
            if (hitEntity == null) return new Color(0, 0, 0);

            // Local shading
            Color local = ShadeLocal(bestHit, hitEntity, ray);

            // Energy balance
            // Energy balance（Keep the local term decaying by kr+kt)
            double kr = hitEntity.Material.Reflectivity;
            double kt = Math.Max(0.0, Math.Min(1.0, hitEntity.Material.Transmissivity * KT_SCALE));
            double remain = Math.Max(0.0, 1.0 - Math.Min(1.0, kr + kt));
            local *= remain;

            double krUse = 0.0;
            double ktUse = 0.0;

            if (kt > 0.0)
            {
                // Transparent material: Schlick Fresnel + Total Internal Reflection (TIR)
                double n1 = 1.0, n2 = hitEntity.Material.RefractiveIndex;    // incident and transmitted IOR
                Vector3 N = bestHit.Normal.Normalized();                     // unit geometric normal
                Vector3 D = ray.Direction.Normalized();                      // unit incident direction (eye -> hit)

                // are we entering the surface? (outside→inside)
                bool entering = ray.Direction.Dot(bestHit.Normal) < 0.0;

                // Ensure N opposes D (i.e., N points to the incident medium).
                // If the ray is inside the object, flip N and swap IORs (n1 <-> n2).
                double cosi = -D.Dot(N);                                     // cos(theta_i)
                if (cosi < 0.0) { N = -N; cosi = -D.Dot(N); n1 = n2; n2 = 1.0; }
                double eta = n1 / n2;                                        // relative IOR

                // Schlick approximation of Fresnel reflectance
                double R0 = Math.Pow((n1 - n2) / (n1 + n2), 2.0);
                double Fr = R0 + (1.0 - R0) * Math.Pow(1.0 - cosi, 5.0);     // angle-dependent reflectance

                // Snell's law (vector form): check discriminant for TIR
                double k = 1.0 - eta * eta * (1.0 - cosi * cosi);
                bool tir = (k <= 0.0);                                       // TIR if no real solution for refraction

                // Mix material reflectivity (kr) with Fresnel term.
                // Clamp to [0,1] to preserve energy; if TIR, all energy goes to reflection.
                krUse = Math.Min(1.0, kr + (1.0 - kr) * (tir ? 1.0 : Fr));
                ktUse = tir ? 0.0 : (kt * (1.0 - Fr));

                // Spawn reflection ray (perfect mirror lobe)
                if (krUse > 0.0 && depth < MAX_DEPTH)
                {
                    Vector3 R = Reflect(D, N).Normalized();
                    // Offset origin along the normal to avoid self-intersection ("shadow acne")
                    Ray reflRay = new Ray(bestHit.Position + N * RAY_EPS, R);
                    local += Trace(reflRay, depth + 1) * krUse;
                }

                // Spawn refraction ray only if not in TIR
                if (ktUse > 0.0 && depth < MAX_DEPTH && !tir)
                {
                    // T = eta*D + (eta*cosi - sqrt(k)) * N
                    Vector3 T = (eta * D + (eta * cosi - Math.Sqrt(k)) * N).Normalized();
                    // Ray refrRay = new Ray(bestHit.Position + T * RAY_EPS, T); // bias along T to prevent self-hit
                    Vector3 bias = (entering ? -bestHit.Normal : bestHit.Normal) * RAY_EPS;
                    Ray refrRay = new Ray(bestHit.Position + bias, T);
                    local += Trace(refrRay, depth + 1) * ktUse;
                }
            }
            else
            {
                // Opaque material: only use intrinsic mirror term (kr). No refraction.
                if (kr > 0.0 && depth < MAX_DEPTH)
                {
                    Vector3 N = bestHit.Normal.Normalized();
                    Vector3 D = ray.Direction.Normalized();
                    Vector3 R = Reflect(D, N).Normalized();
                    Ray reflRay = new Ray(bestHit.Position + N * RAY_EPS, R); // small bias to avoid acne
                    local += Trace(reflRay, depth + 1) * kr;
                }
            }

            // 'local' already includes the weighted local shading and any recursive terms above.
            return local;


            /*
            if (kt > 0.0)
            {
                // Only transparent materials use Schlick to distribute between reflection/refraction
                double ior = hitEntity.Material.RefractiveIndex;
                Vector3 Nf = bestHit.Normal.Normalized();
                Vector3 Df = ray.Direction.Normalized();
                double cosiF = Math.Abs(Df.Dot(Nf));
                double R0 = Math.Pow((1.0 - ior) / (1.0 + ior), 2);
                double Fr = R0 + (1.0 - R0) * Math.Pow(1.0 - cosiF, 5);

                // Reflection coefficient = intrinsic reflection + Schlick component of the residual energy
                krUse = Math.Min(1.0, kr + (1.0 - kr) * Fr);
                // Refraction only comes from the transparent part
                ktUse = kt * (1.0 - Fr);
            }
            else
            {
                // Opaque: Use only the material's built-in reflection and disable refraction
                krUse = kr;
                ktUse = 0.0;
            }
            */

            /*
            double kr = hitEntity.Material.Reflectivity;
            double kt = Math.Max(0.0, Math.Min(1.0, hitEntity.Material.Transmissivity * KT_SCALE));
            double remain = Math.Max(0.0, 1.0 - Math.Min(1.0, kr + kt));
            local *= remain;

            double ior = hitEntity.Material.RefractiveIndex;
            Vector3 Nf = bestHit.Normal.Normalized();
            Vector3 Df = ray.Direction.Normalized();
            double cosiF = Math.Abs(Df.Dot(Nf));
            double R0 = Math.Pow((1.0 - ior) / (1.0 + ior), 2);
            double Fr = R0 + (1.0 - R0) * Math.Pow(1.0 - cosiF, 5);

            double krUse = Math.Max(kr, Fr);
            double ktUse = Math.Max(0.0, 1.0 - krUse);

            // Reflection
            if (krUse > 0.0 && depth < MAX_DEPTH)
            {
                Vector3 N = bestHit.Normal.Normalized();
                Vector3 D = ray.Direction.Normalized();
                Vector3 R = D - 2.0 * D.Dot(N) * N;
                Ray reflRay = new Ray(bestHit.Position + N * RAY_EPS, R);
                Color reflCol = Trace(reflRay, depth + 1);
                local += reflCol * krUse;
            }

            // Refraction
            if (ktUse > 0.0 && depth < MAX_DEPTH)
            {
                Vector3 N = bestHit.Normal.Normalized();
                Vector3 D = ray.Direction.Normalized();

                double n1 = 1.0, n2 = ior;
                double cosi = -D.Dot(N);
                if (cosi < 0.0)
                {
                    N = -N;
                    cosi = -D.Dot(N);
                    n1 = ior; n2 = 1.0;
                }
                double eta = n1 / n2;

                double k = 1.0 - eta * eta * (1.0 - cosi * cosi);
                if (k > 0.0)
                {
                    Vector3 T = eta * D + (eta * cosi - Math.Sqrt(k)) * N;
                    T = T.Normalized();
                    Ray refrRay = new Ray(bestHit.Position + T * RAY_EPS, T);
                    Color refrCol = Trace(refrRay, depth + 1);
                    local += refrCol * ktUse;
                }
            }
            */
        }

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.camera = new Camera(Transform.Identity);
            this.ambientLightColor = new Color(0, 0, 0);
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
            this.animations = new HashSet<Animation>();
        }

        /// <summary>
        /// Set the camera for the scene.
        /// </summary>
        /// <param name="camera">Camera object</param>
        public void SetCamera(Camera camera)
        {
            this.camera = camera;
        }

        /// <summary>
        /// Set the ambient light color for the scene.
        /// </summary>
        /// <param name="color">Color object</param>
        public void SetAmbientLightColor(Color color)
        {
            this.ambientLightColor = color;
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Add an animation to the scene.
        /// </summary>
        /// <param name="animation">Animation object</param>
        public void AddAnimation(Animation animation)
        {
            this.animations.Add(animation);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        /// <param name="time">Time since start in seconds</param>
        public void Render(Image outputImage, double time = 0)
        {
            // Begin writing your code here...
            // size 
            int width = outputImage.Width;
            int height = outputImage.Height;

            //
            //Vector3 camPos = new Vector3(0, 0, 0);
            double fov = Math.PI / 3.0;
            double aspect = (double)width / height;
            double halfTan = Math.Tan(fov * 0.5);

            // Camera data (position + rotation) parsed from SceneReader
            Transform camTr = this.camera.Transform;
            // Camera position in the world coordinate system (ray originates from this point)
            Vector3 camPos = camTr.Position;
            // Camera rotation (represented by a quaternion), used to convert the local orientation to the world orientation
            Quaternion camRot = camTr.Rotation;

            // random 
            var rng = new Random(1337);
            
            // === C1: apply simple animations once per frame ===
            foreach (var anim in this.animations)
            {
                if (anim is SimpleAnimation sa && sa.Entity is ObjModel obj)
                {
                    obj.ApplySimpleDelta(sa.TranslationPerFrame, sa.RotationPerFrame);
                }
            }


            // render each pixel
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // --- Anti-Aliasing: stratified jitter sampling ---
                    int aa = Math.Max(1, this.options.AAMultiplier);
                    int spp = aa * aa;
                    Color pixelColor = new Color(0, 0, 0);
                    for (int sy = 0; sy < aa; sy++)
                    {
                        for (int sx = 0; sx < aa; sx++)
                        {
                            // Sub-pixel random dithering (layered + dithering); 
                            // when aa==1, use the center 0.5 to ensure no noise
                            double u = (sx + (aa == 1 ? 0.5 : rng.NextDouble())) / aa;
                            double v = (sy + (aa == 1 ? 0.5 : rng.NextDouble())) / aa;

                            // NDC ([-1,1]), replace the original +0.5 with the sub-pixel position
                            double ndcX = ((x + u) / width) * 2.0 - 1.0;
                            double ndcY = 1.0 - ((y + v) / height) * 2.0;

                            // Projected onto the imaging plane (z=1)
                            double px = ndcX * halfTan * aspect;
                            double py = ndcY * halfTan;

                            double F = Math.Max(1e-6, this.options.FocalLength);
                            double aperture = this.options.ApertureRadius;

                            // Pixel's focus point on the focal plane z=F (camera local coords)
                            Vector3 focusLocal = new Vector3(px * F, py * F, F);
                            Vector3 focusWorld = camPos + camRot.Rotate(focusLocal);

                            Vector3 originWorld;
                            Vector3 dirWorld;

                            if (aperture > 0.0)
                            {
                                // sample a point on the lens aperture (uniform disk)
                                double theta = 2.0 * Math.PI * rng.NextDouble();
                                double r = aperture * Math.Sqrt(rng.NextDouble()); // sqrt for uniform disk
                                double dx = r * Math.Cos(theta);
                                double dy = r * Math.Sin(theta);

                                Vector3 apertureLocal = new Vector3(dx, dy, 0.0);
                                originWorld = camPos + camRot.Rotate(apertureLocal);
                                dirWorld = (focusWorld - originWorld).Normalized();
                            }
                            else
                            {
                                // pinhole fallback
                                Vector3 dirLocal = new Vector3(px, py, 1.0).Normalized();
                                originWorld = camPos;
                                dirWorld = camRot.Rotate(dirLocal).Normalized();
                            }

                            Ray ray = new Ray(originWorld, dirWorld);
                            pixelColor += Trace(ray, 0);




                            /*
                            // Construct ray (camera rotated to world coordinates)
                            Vector3 dirLocal = new Vector3(px, py, 1.0).Normalized();
                            Vector3 dirWorld = camRot.Rotate(dirLocal).Normalized();
                            Ray ray = new Ray(camPos, dirWorld);

                            // Accumulate the subsample color
                            pixelColor += Trace(ray, 0);
                            */
                        }
                    }
                    // trace recursively
                    pixelColor *= (1.0 / spp);
                    outputImage.SetPixel(x, y, pixelColor);
                }
            }
        }
    }
}