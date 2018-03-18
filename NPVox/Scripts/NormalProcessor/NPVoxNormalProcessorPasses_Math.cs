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
    public int[] m_iIndices;
    public Vector3[] m_iNormals;

    public override void Process( NPVoxModel model, NPVoxMeshData tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        for ( int t = 0; t < tempdata.numVertices; t++ )
        {
            outNormals[ tempdata.vertexIndexOffsetBegin + t ] = inNormals[ tempdata.vertexIndexOffsetBegin + t ];
        }

        for ( int i = 0; i < m_iIndices.Length; i++ )
        {
            if ( m_iIndices[ i ] >= 0 && m_iIndices[ i ] < outNormals.Length )
            {
                outNormals[ m_iIndices[ i ] ] = m_iNormals[ i ];
            }
        }
    }
}
