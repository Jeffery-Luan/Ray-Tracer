
namespace RayTracer
{
    /// <summary>
    /// Interface to represent an animation that can be applied to entities in the scene.
    /// </summary>
    public interface Animation
    {
        /// <summary>
        /// The entity that this animation is applied to.
        /// </summary>
        SceneEntity Entity { get; }
    }
}
