using UnityEngine;
using System.Collections;

// from http://answers.unity3d.com/questions/402280/how-to-decompose-a-trs-matrix.html
public class Matrix4x4Util
{

    public static Vector3 GetPosition(Matrix4x4 m)
    {
        return new Vector3(m[0, 3], m[1, 3], m[2, 3]);
    }

    public static Vector3 GetScale(Matrix4x4 m)
    {
        return new Vector3
            (m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
    }

    public static Quaternion GetRotation(Matrix4x4 m)
    {
        Vector3 s = GetScale(m);

        // Normalize Scale from Matrix4x4
        float m00 = m[0, 0] / s.x;
        float m01 = m[0, 1] / s.y;
        float m02 = m[0, 2] / s.z;
        float m10 = m[1, 0] / s.x;
        float m11 = m[1, 1] / s.y;
        float m12 = m[1, 2] / s.z;
        float m20 = m[2, 0] / s.x;
        float m21 = m[2, 1] / s.y;
        float m22 = m[2, 2] / s.z;

        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m00 + m11 + m22)) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m00 - m11 - m22)) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m00 + m11 - m22)) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m00 - m11 + m22)) / 2;
        q.x *= Mathf.Sign(q.x * (m21 - m12));
        q.y *= Mathf.Sign(q.y * (m02 - m20));
        q.z *= Mathf.Sign(q.z * (m10 - m01));

        // q.Normalize()
        float qMagnitude = Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
        q.w /= qMagnitude;
        q.x /= qMagnitude;
        q.y /= qMagnitude;
        q.z /= qMagnitude;

        return q;
    }

    public static Vector3 QuaternionToEuler(Quaternion q)
    {
        Vector3 result;

        float test = q.x * q.y + q.z * q.w;
        // singularity at north pole
        if (test > 0.499)
        {
            result.x = 0;
            result.y = 2 * Mathf.Atan2(q.x, q.w);
            result.z = Mathf.PI / 2;
        }
        // singularity at south pole
        else if (test < -0.499)
        {
            result.x = 0;
            result.y = -2 * Mathf.Atan2(q.x, q.w);
            result.z = -Mathf.PI / 2;
        }
        else
        {
            result.x = Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
            result.y = Mathf.Rad2Deg * Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
            result.z = Mathf.Rad2Deg * Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);

            if (result.x < 0) result.x += 360;
            if (result.y < 0) result.y += 360;
            if (result.z < 0) result.z += 360;
        }
        return result;
    }
}
