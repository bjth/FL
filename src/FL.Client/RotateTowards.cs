using System.Numerics;
namespace FL.Client;

public static class RotationHelper
{
    public static Matrix4x4 RotateTowards(this Vector3 position, Vector3 target)
    {
        var difference = Vector3.Subtract(position, target);
        var yaw = Convert.ToSingle(Math.Atan2(difference.Z, difference.X) + Math.PI / 2.0f);
        var newRotation = new Vector3(0, yaw, 0);
        return Matrix4x4.CreateFromQuaternion(ToQuaternion(newRotation));
    }
    
    public static Quaternion ToQuaternion(Vector3 v)
    {
        var cy = (float)Math.Cos(v.Z * 0.5);
        var sy = (float)Math.Sin(v.Z * 0.5);
        var cp = (float)Math.Cos(v.Y * 0.5);
        var sp = (float)Math.Sin(v.Y * 0.5);
        var cr = (float)Math.Cos(v.X * 0.5);
        var sr = (float)Math.Sin(v.X * 0.5);

        return new Quaternion
        {
            W = (cr * cp * cy + sr * sp * sy),
            X = (sr * cp * cy - cr * sp * sy),
            Y = (cr * sp * cy + sr * cp * sy),
            Z = (cr * cp * sy - sr * sp * cy)
        };
    }
    
    public static Vector3 ToEulerAngles(Quaternion q)
    {
        Vector3 angles = new();

        // roll / x
        double sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
        double cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.X = (float)Math.Atan2(sinrCosp, cosrCosp);

        // pitch / y
        double sinp = 2 * (q.W * q.Y - q.Z * q.X);
        if (Math.Abs(sinp) >= 1)
        {
            angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
        }
        else
        {
            angles.Y = (float)Math.Asin(sinp);
        }

        // yaw / z
        double sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
        double cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = (float)Math.Atan2(sinyCosp, cosyCosp);

        return angles;
    }
}