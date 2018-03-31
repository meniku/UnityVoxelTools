using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[NPVoxAttributeNormalProcessorListItem( "User Override", typeof( NPVoxNormalProcessor_UserOverride ), NPVoxNormalProcessorType.Generator )]
public class NPVoxNormalProcessor_UserOverride : NPVoxNormalProcessor, ISerializationCallbackReceiver
{
    [SerializeField]
    private int[] m_overrideNormalIndices = null;

    [SerializeField]
    private Vector3[] m_overrideNormals = null;

    NPVoxNormalProcessorPass_ApplyNormals m_passApplyNormals;

    [NonSerialized]
    public Dictionary<int, Vector3> m_overrideNormalsRT;

    public void OnBeforeSerialize()
    {
        WriteBuffersForSerialization();
    }

    public void OnAfterDeserialize()
    {
        if ( m_overrideNormalsRT == null )
        {
            m_overrideNormalsRT = new Dictionary<int, Vector3>();
        }

        if ( m_overrideNormalIndices != null && m_overrideNormals != null )
        {
            int iLength = Mathf.Min( m_overrideNormalIndices.Length, m_overrideNormals.Length );
            for ( int i = 0; i < iLength; i++ )
            {
                m_overrideNormalsRT.Add( m_overrideNormalIndices[ i ], m_overrideNormals[ i ] );
            }
        }
    }

    private void WriteBuffersForSerialization()
    {
        if ( m_overrideNormalsRT == null )
        {
            m_overrideNormalsRT = new Dictionary<int, Vector3>();
        }

        m_overrideNormalIndices = new int[ m_overrideNormalsRT.Count ];
        m_overrideNormals = new Vector3[ m_overrideNormalsRT.Count ];

        int i = 0;
        foreach ( int key in m_overrideNormalsRT.Keys )
        {
            m_overrideNormalIndices[ i ] = key;
            m_overrideNormals[ i ] = m_overrideNormalsRT[ key ];
            i++;
        }
    }

    public override object Clone()
    {
        NPVoxNormalProcessor_UserOverride clone = ScriptableObject.CreateInstance<NPVoxNormalProcessor_UserOverride>();
        
        foreach ( int filter in m_voxelGroupFilter )
        {
            clone.m_voxelGroupFilter.Add( filter );
        }

        return clone;
    }

    protected override void OneTimeInit()
    {        m_passApplyNormals = AddPass<NPVoxNormalProcessorPass_ApplyNormals>();
    }

    protected override void OnGUIInternal()
    {
    }

    protected override void PerModelInit()
    {
        WriteBuffersForSerialization();
        m_passApplyNormals.m_indices = m_overrideNormalIndices;
        m_passApplyNormals.m_normals = m_overrideNormals;
    }

    public override void OnListChanged( NPVoxNormalProcessorList _list )
    {
        base.OnListChanged(_list);
    }

}
