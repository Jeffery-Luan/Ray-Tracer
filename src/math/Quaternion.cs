
using System;

namespace RayTracer
{
    /// <summary>
    /// Structure to represent a quaternion, which is used for representing rotations in 3D space.
    /// Quaternions are more efficient and avoid gimbal lock compared to Euler angles.
    /// This implementation is immutable and provides methods for quaternion operations.
    /// </summary>
    public struct Quaternion
    {
        public double W { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        public Quaternion(double w, double x, double y, double z)
        {
            this.W = w;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Quaternion Identity => new Quaternion(1, 0, 0, 0);

        /// <summary>
        /// Construct a quaternion from an axis and an angle.
        /// The angle is in degrees.
        /// </summary>
        /// <param name="axis">The axis of rotation</param>
        /// <param name="angle">The angle of rotation in degrees</param>
        public Quaternion(Vector3 axis, double angle)
        {
            double halfAngle = angle * 0.5 * Math.PI / 180;
            double sinHalfAngle = Math.Sin(halfAngle);
            this.W = Math.Cos(halfAngle);
            this.X = axis.X * sinHalfAngle;
            this.Y = axis.Y * sinHalfAngle;
            this.Z = axis.Z * sinHalfAngle;
        }

        /// <summary>
        /// Multiply two quaternions.
        /// </summary>
        /// <param name="q1">First quaternion</param>
        /// <param name="q2">Second quaternion</param>
        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(
                q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z,
                q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y,
                q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X,
                q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W
            );
        }

        /// <summary>
        /// Rotate a vector by this quaternion.
        /// </summary>
        /// <param name="v">Vector to rotate</param>
        /// <returns>Rotated vector</returns>
        public Vector3 Rotate(Vector3 v)
        {
            Vector3 u = new Vector3(X, Y, Z);
            double s = W;
            return u * (2.0 * u.Dot(v)) + v * (s * s - u.Dot(u)) +
                   u.Cross(v) * (2.0 * s);
        }

        /// <summary>
        /// Normalize the quaternion.
        /// </summary>
        /// <returns>Normalized quaternion</returns>
        public Quaternion Normalized()
        {
            double length = Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
            if (length < double.Epsilon)
            {
                return Identity;
            }
            return new Quaternion(W / length, X / length, Y / length, Z / length);
        }
    }
}
