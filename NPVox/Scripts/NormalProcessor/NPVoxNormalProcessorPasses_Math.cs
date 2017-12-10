using System;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxNormalProcessorPass_Normalize : NPVoxNormalProcessorPass
{
    public override void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, out Vector3[] outNormals )
    {
        m_normalBuffer = new Vector3[ inNormals.Length ];

        foreach ( NPVoxMeshTempData data in tempdata )
        {
            for ( int t = 0; t < data.numVertices; t++ )
            {
                m_normalBuffer[ data.vertexIndexOffsetBegin + t ] = inNormals[ data.vertexIndexOffsetBegin + t ].normalized;
            }
        }

        outNormals = m_normalBuffer;
    }
}