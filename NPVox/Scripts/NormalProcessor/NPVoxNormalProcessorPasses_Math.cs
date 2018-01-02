using System;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxNormalProcessorPass_Normalize : NPVoxNormalProcessorPass
{
    public override void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        foreach ( NPVoxMeshTempData data in tempdata )
        {
            for ( int t = 0; t < data.numVertices; t++ )
            {
                outNormals[ data.vertexIndexOffsetBegin + t ] = inNormals[ data.vertexIndexOffsetBegin + t ].normalized;
            }
        }
    }
}