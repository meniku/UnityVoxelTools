using System;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxNormalProcessorPass_Normalize : NPVoxNormalProcessorPass
{
    public override void Process( NPVoxModel model, NPVoxMeshTempData tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        for ( int t = 0; t < tempdata.numVertices; t++ )
        {
            outNormals[tempdata.vertexIndexOffsetBegin + t ] = inNormals[tempdata.vertexIndexOffsetBegin + t ].normalized;
        }
    }
}