using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NPVoxNormalProcessorPass_ApplyNormals : NPVoxNormalProcessorPass
{
    public Dictionary<NPVoxCoord, Vector3> m_normalData;

    public override void Process(NPVoxModel model, NPVoxMeshData tempdata, Vector3[] inNormals, ref Vector3[] outNormals)
    {
        for (int t = 0; t < tempdata.numVertices; t++)
        {
            outNormals[tempdata.vertexIndexOffsetBegin + t] = inNormals[tempdata.vertexIndexOffsetBegin + t];
        }

        if ( m_normalData.ContainsKey( tempdata.voxCoord ) )
        {
            Vector3 normal = m_normalData[tempdata.voxCoord];
            for (int t = 0; t < tempdata.numVertices; t++ )
            {
                outNormals[tempdata.vertexIndexOffsetBegin + t] = normal;
            }
        }
    }
}
