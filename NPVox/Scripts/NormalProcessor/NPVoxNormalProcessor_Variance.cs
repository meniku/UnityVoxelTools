using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class NPVoxNormalProcessorPass_Variance : NPVoxNormalProcessorPass
{
    public int m_normalVarianceSeed;
    public Vector3 m_normalVariance;

    public override void Process( NPVoxModel model, NPVoxMeshTempData tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        // Compute normal variance
        float rX = UnityEngine.Random.value;
        float rY = UnityEngine.Random.value;
        float rZ = UnityEngine.Random.value;

        for ( int t = 0; t < tempdata.numVertices; t++ )
        {
            Vector3 variance = Vector3.zero;
            if ( m_normalVariance.x != 0 || m_normalVariance.y != 0 || m_normalVariance.z != 0 )
            {
                variance.x = -m_normalVariance.x * 0.5f + 2 * rX * m_normalVariance.x;
                variance.y = -m_normalVariance.y * 0.5f + 2 * rY * m_normalVariance.y;
                variance.z = -m_normalVariance.z * 0.5f + 2 * rZ * m_normalVariance.z;
            }

            outNormals[tempdata.vertexIndexOffsetBegin + t ] = inNormals[tempdata.vertexIndexOffsetBegin + t];
            outNormals[tempdata.vertexIndexOffsetBegin + t ] += variance;
        }
    }
}


[NPVoxAttributeNormalProcessorListItem( "Variance", typeof( NPVoxNormalProcessor_Variance ), NPVoxNormalProcessorType.Modifier )]
public class NPVoxNormalProcessor_Variance : NPVoxNormalProcessor
{
    [SerializeField]
    private NPVoxNormalProcessorPass_Variance m_passVariance;

    [SerializeField]
    private NPVoxNormalProcessorPass_Normalize m_passNormalize;
    
    [SerializeField]
    private Vector3 m_normalVariance;

    [SerializeField]
    private int m_normalVarianceSeed;
    
    public override object Clone()
    {
        NPVoxNormalProcessor_Variance clone = ScriptableObject.CreateInstance<NPVoxNormalProcessor_Variance>();
        clone.m_normalVariance = m_normalVariance;
        clone.m_normalVarianceSeed = m_normalVarianceSeed;
        
        clone.m_passVariance.m_normalVariance = m_passVariance.m_normalVariance;
        clone.m_passVariance.m_normalVarianceSeed = m_passVariance.m_normalVarianceSeed;
        
        foreach ( int filter in m_voxelGroupFilter )
        {
            clone.m_voxelGroupFilter.Add(filter);
        }
        
        return clone;
    }

    // Processor parameters
    public Vector3 NormalVariance
    {
        get { return m_normalVariance; }
        set
        {
            m_normalVariance = value;
        }
    }

    public int NormalVarianceSeed
    {
        get { return m_normalVarianceSeed; }
        set
        {
            m_normalVarianceSeed = value;
        }
    }

    // c-tor
    public NPVoxNormalProcessor_Variance()
    {
        NormalVariance = Vector3.zero;
    }

    // functions
    protected override void OneTimeInit()
    {
        m_passVariance = AddPass<NPVoxNormalProcessorPass_Variance>();
        m_passNormalize = AddPass<NPVoxNormalProcessorPass_Normalize>();
    }

    protected override void PerModelInit()
    {
        UnityEngine.Random.InitState(m_normalVarianceSeed);

        m_passVariance.m_normalVariance = NormalVariance;
        m_passVariance.m_normalVarianceSeed = NormalVarianceSeed;
    }
    
    protected override void OnGUIInternal()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(GUITabWidth);
        NormalVariance = EditorGUILayout.Vector3Field("Normal Variance", NormalVariance);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(GUITabWidth);
        NormalVarianceSeed = EditorGUILayout.IntField("Normal Variance Seed", NormalVarianceSeed);
        GUILayout.EndHorizontal();
    }
}