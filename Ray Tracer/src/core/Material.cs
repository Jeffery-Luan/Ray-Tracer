
namespace RayTracer
{
    /// <summary>
    /// Class to represent a material that is associated with an entity
    /// that is to be rendered by the ray tracer. Materials contain
    /// properties that describe how light should interact with an object.
    /// </summary>
    public class Material
    {
        public Color AmbientColor { get; private set; }
        public Color DiffuseColor { get; private set; }
        public Color SpecularColor { get; private set; }
        public double Shininess { get; private set; }
        public double Reflectivity { get; private set; }
        public double Transmissivity { get; private set; }
        public double RefractiveIndex { get; private set; }

        /// <summary>
        /// Construct a new material object.
        /// </summary>
        /// <param name="ambientColor">The ambient color of the material</param>
        /// <param name="diffuseColor">The diffuse color of the material</param>
        /// <param name="specularColor">The specular color of the material</param>
        /// <param name="shininess">The shininess of the material</param>
        /// <param name="reflectivity">The reflectivity of the material</param>
        /// <param name="transmissivity">The transmissivity of the material</param>
        /// <param name="refractiveIndex">The refractive index of the material</param>
        public Material(
            Color ambientColor,
            Color diffuseColor,
            Color specularColor,
            double shininess,
            double reflectivity,
            double transmissivity,
            double refractiveIndex)
        {
            this.AmbientColor = ambientColor;
            this.DiffuseColor = diffuseColor;
            this.SpecularColor = specularColor;
            this.Shininess = shininess;
            this.Reflectivity = reflectivity;
            this.Transmissivity = transmissivity;
            this.RefractiveIndex = refractiveIndex;
        }

        /// <summary>
        /// Get the color of the material at a given texture coordinate.
        /// </summary>
        /// <param name="u">The u coordinate in the texture map</param>
        /// <param name="v">The v coordinate in the texture map</param>
        /// <returns>The color at the specified texture coordinate</returns>
        public virtual Color GetDiffuseColor(TextureCoord coord)
        {
            // For basic materials, return the base color. Texture materials
            // will override this method to provide texture-specific colors.
            return this.DiffuseColor;
        }

        /// <summary>
        /// Get the normal vector of the material at a given texture coordinate.
        /// </summary>
        /// <param name="u">The u coordinate in the texture map</param>
        /// <param name="v">The v coordinate in the texture map</param>
        /// <returns>The normal vector at the specified texture coordinate</returns>
        public virtual Vector3 GetNormal(TextureCoord coord)
        {
            // No normal map, return default normal
            return new Vector3(0, 0, 1); 
        }
    }
}
