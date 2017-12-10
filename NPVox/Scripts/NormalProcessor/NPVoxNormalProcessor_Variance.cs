using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NPVoxNormalProcessorPass_Variance : NPVoxNormalProcessorPass
{
    public int m_normalVarianceSeed;
    public Vector3 m_normalVariance;

    public override void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, out Vector3[] outNormals )
    {
        m_normalBuffer = new Vector3[ inNormals.Length ];

        UnityEngine.Random.InitState( m_normalVarianceSeed );

        foreach ( NPVoxMeshTempData data in tempdata )
        {
            // Compute normal variance
            float rX = UnityEngine.Random.value;
            float rY = UnityEngine.Random.value;
            float rZ = UnityEngine.Random.value;

            for ( int t = 0; t < data.numVertices; t++ )
            {
                Vector3 variance = Vector3.zero;
                if ( m_normalVariance.x != 0 || m_normalVariance.y != 0 || m_normalVariance.z != 0 )
                {
                    variance.x = -m_normalVariance.x * 0.5f + 2 * rX * m_normalVariance.x;
                    variance.y = -m_normalVariance.y * 0.5f + 2 * rY * m_normalVariance.y;
                    variance.z = -m_normalVariance.z * 0.5f + 2 * rZ * m_normalVariance.z;
                }

                m_normalBuffer[ data.vertexIndexOffsetBegin + t ] = inNormals[ data.vertexIndexOffsetBegin + t ];
                m_normalBuffer[ data.vertexIndexOffsetBegin + t ] += variance;
            }
        }

        outNormals = m_normalBuffer;
    }
}


[NPVoxAttributeNormalProcessorListItem( "Filter: Noise", typeof( NPVoxNormalProcessor_Variance ), NPVoxNormalProcessorType.Progressive )]
public class NPVoxNormalProcessor_Variance : NPVoxNormalProcessor
{
    private NPVoxNormalProcessorPass_Variance m_passVariance;

    private NPVoxNormalProcessorPass_Normalize m_passNormalize;

    // Processor parameters
    public Vector3 NormalVariance;
    public int NormalVarianceSeed;

    // c-tor
    public NPVoxNormalProcessor_Variance()
    {
        NormalVariance = Vector3.zero;

        m_passVariance = new NPVoxNormalProcessorPass_Variance();
        m_passNormalize = new NPVoxNormalProcessorPass_Normalize();

        m_passes.Add( m_passVariance );
        m_passes.Add( m_passNormalize );
    }


    // functions
    public override void OneTimeInit()
    {
    }

    protected override void PerModelInit()
    {
        m_passVariance.m_normalVariance = NormalVariance;
        m_passVariance.m_normalVarianceSeed = NormalVarianceSeed;
    }
}