using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxGeomUtil : MonoBehaviour 
{
    public static int DrawLine(NPVoxModel model, Vector3 P1, Vector3 P2, byte color, byte voxelGroup = 255, bool overwrite = false)
    {
        float lineDist = (P2 - P1).magnitude;
        Vector3 lineDir = (P2 - P1).normalized;
        Vector3 drawPoint = P1;
        int addedVoxels = 0;
        float currentLineDist = 0.0f;
//        bool setVoxelGroup = voxelGroup < 255;
        while (currentLineDist < lineDist)
        {
            NPVoxCoord coord = NPVoxCoordUtil.ToCoord(drawPoint);
            if(!model.HasVoxel(coord) || overwrite)
            {
                model.SetVoxel(coord, color);
                addedVoxels++;
                model.SetVoxelGroup(coord, voxelGroup);
            }
            drawPoint += lineDir * 0.5f;
            currentLineDist += 0.5f;
        }
//        model.NumVoxels += addedVoxels;
        return addedVoxels;
    }

    public static Vector3 GetBezierPoint(Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4, float t)
    {
        Vector3 A1 = Vector3.Lerp(P1, P2, t);
        Vector3 A2 = Vector3.Lerp(P2, P3, t);
        Vector3 A3 = Vector3.Lerp(P3, P4, t);
        Vector3 B1 = Vector3.Lerp(A1, A2, t);
        Vector3 B2 = Vector3.Lerp(A2, A3, t);
        return Vector3.Lerp(B1, B2, t);
    }
}
