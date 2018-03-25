using System;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxNormalProcessorPass_Normalize : NPVoxNormalProcessorPass
{
    public override void Process( NPVoxModel model, NPVoxMeshData tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        for ( int t = 0; t < tempdata.numVertices; t++ )
        {
            outNormals[tempdata.vertexIndexOffsetBegin + t ] = inNormals[tempdata.vertexIndexOffsetBegin + t ].normalized;
        }
    }
}

public class NPVoxNormalProcessorPass_ApplyNormals : NPVoxNormalProcessorPass
{
    public int[] m_indices;
    public Vector3[] m_normals;

    public override void Process( NPVoxModel model, NPVoxMeshData tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        for ( int t = 0; t < tempdata.numVertices; t++ )
        {
            outNormals[ tempdata.vertexIndexOffsetBegin + t ] = inNormals[ tempdata.vertexIndexOffsetBegin + t ];
        }

        for ( int i = 0; i < m_indices.Length; i++ )
        {
            if ( m_indices[ i ] >= 0 && m_indices[ i ] < outNormals.Length )
            {
                outNormals[ m_indices[ i ] ] = m_normals[ i ];
            }
        }
    }
}
