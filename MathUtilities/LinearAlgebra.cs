using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Credits: 
// https://answers.unity.com/questions/424974/nearest-point-on-mesh.html
// https://gamedev.stackexchange.com/questions/131851/clamp-point-to-triangle-for-sphere-collision

namespace MathUtilities
{
    public class LinearAlgebra
    {
        public const float THIRD = 1.0f / 3.0f;

        public static Vector3 ComputePlaneNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Vector3.Cross((p2 - p1).normalized, (p3 - p1).normalized).normalized;
        }

        // Projects given point (x) to plane (p1, p2, p3), resulting in point (q)
        public static void ProjectPointToPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 x, out Vector3 q)
        {
            Vector3 normal = ComputePlaneNormal(p1, p2, p3);
            ProjectPointToPlane(p1, normal, x, out q);
        }

        // Projects given point (x) to plane with point (p) and normal (n), resulting in point (q)
        public static void ProjectPointToPlane(Vector3 p, Vector3 n, Vector3 x, out Vector3 q)
        {
            q = x + Vector3.Dot(p - x, n) * n;
        }

        // Projects given point (x) to line with origin (p) and direction (v).
        public static Vector3 ProjectPointToLine(Vector3 p1, Vector3 p2, Vector3 x)
        {
            Vector3 vnorm = (p2 - p1).normalized;
            float s = Vector3.Dot(x - p1, vnorm);
            return p1 + s * vnorm;
        }

        // Projects given point (x) to line with origin (p) and direction (v). Also returns scalar s
        public static Vector3 ProjectPointToLine(Vector3 p1, Vector3 p2, Vector3 x, out float s)
        {
            Vector3 vnorm = (p2 - p1).normalized;
            s = Vector3.Dot(x - p1, vnorm);
            return p1 + s * vnorm;
        }

        public static Vector3 ClampToLine(Vector3 p1, Vector3 p2, Vector3 x)
        {
            float s;
            Vector3 xx = ProjectPointToLine(p1, p2, x, out s);
            if (s > 1.0f)
            {
                return p2;
            }
            if (s < 0.0f)
            {
                return p1;
            }

            return xx;
        }

        public static float ComputeArea(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p1;
            return 0.5f * Vector3.Cross(v1, v2).magnitude;
        }

        // Computes barycentric coordinates of point (q) in triangle(p1, p2, p3) in R2
        public static Vector3 WorldToBarycentric2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 q)
        {
            var t = ((p1.x * p2.y) - (p1.x * p3.y) - (p2.x * p1.y) + (p2.x * p3.y) + (p3.x * p1.y) - (p3.x * p2.y));
            var u = ((q.x * p2.y) - (q.x * p3.y) - (p2.x * q.y) + (p2.x * p3.y) + (p3.x * q.y) - (p3.x * p2.y)) / t;
            var v = ((p1.x * q.y) - (p1.x * p3.y) - (q.x * p1.y) + (q.x * p3.y) + (p3.x * p1.y) - (p3.x * q.y)) / t;
            var w = ((p1.x * p2.y) - (p1.x * q.y) - (p2.x * p1.y) + (p2.x * q.y) + (q.x * p1.y) - (q.x * p2.y)) / t;
            return new Vector3(u, v, w);
        }

        // Computes barycentric coordinates of point (q) in triangle(p1, p2, p3) in R3
        public static Vector3 WorldToBarycentric3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 q)
        {
            Vector3 v21 = p2 - p1;
            Vector3 v31 = p3 - p1;
            Vector3 v1 = q - p1;
            float dot00 = Vector3.Dot(v21, v21);
            float dot01 = Vector3.Dot(v21, v31);
            float dot11 = Vector3.Dot(v31, v31);
            float dot20 = Vector3.Dot(v1, v21);
            float dot21 = Vector3.Dot(v1, v31);
            float denom = dot00 * dot11 - dot01 * dot01;
            float v = (dot11 * dot20 - dot01 * dot21) / denom;
            float w = (dot00 * dot21 - dot01 * dot20) / denom;
            float u = 1.0f - v - w;
            return new Vector3(u, v, w);
        }

        public static Vector3 BarycentricToWorld(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 uvw)
        {
            return p1 * uvw.x + p2 * uvw.y + p3 * uvw.z;
        }

        // Computes the center of the given triangle (p1, p2, p3)
        public static Vector3 ComputeTriangleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return p1 * THIRD + p2 * THIRD + p3 * THIRD;
        }

        public static Vector3 MultiplyComponents(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);

        }

        public static Vector3 DivideComponents(Vector3 lh, Vector3 rh)
        {
            return new Vector3(lh.x / rh.x, lh.y / rh.y, lh.z / rh.z);
        }
    }
}