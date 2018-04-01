using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[NPVoxAttributeNormalProcessorListItem( "User Override", typeof( NPVoxNormalProcessor_UserOverride ), NPVoxNormalProcessorType.Generator )]
public class NPVoxNormalProcessor_UserOverride : NPVoxNormalProcessor, ISerializationCallbackReceiver
{
    [SerializeField]
    private NPVoxCoord[] m_overrideCoords = null;

    [SerializeField]
    private Vector3[] m_overrideNormals = null;

    NPVoxNormalProcessorPass_ApplyNormals m_passApplyNormals;

    [NonSerialized]
    public Dictionary<NPVoxCoord, Vector3> m_overrideNormalsRT;

    public void OnBeforeSerialize()
    {
        WriteBuffersForSerialization();
    }

    public void OnAfterDeserialize()
    {
        if ( m_overrideNormalsRT == null )
        {
            m_overrideNormalsRT = new Dictionary<NPVoxCoord, Vector3>();
        }

        if (m_overrideCoords != null && m_overrideNormals != null )
        {
            int iLength = Mathf.Min(m_overrideCoords.Length, m_overrideNormals.Length );
            for ( int i = 0; i < iLength; i++ )
            {
                m_overrideNormalsRT.Add(m_overrideCoords[ i ], m_overrideNormals[ i ] );
            }
        }
    }

    private void WriteBuffersForSerialization()
    {
        if ( m_overrideNormalsRT == null )
        {
            m_overrideNormalsRT = new Dictionary<NPVoxCoord, Vector3>();
        }

        m_overrideCoords = new NPVoxCoord[ m_overrideNormalsRT.Count ];
        m_overrideNormals = new Vector3[ m_overrideNormalsRT.Count ];

        int i = 0;
        foreach (NPVoxCoord key in m_overrideNormalsRT.Keys )
        {
            m_overrideCoords[ i ] = key;
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

    protected override void OnBeforeProcess(NPVoxModel model, NPVoxMeshData[] tempdata)
    {
        WriteBuffersForSerialization();
        
        m_passApplyNormals.m_normalData = m_overrideNormalsRT;
    }

    public override void OnListChanged( NPVoxNormalProcessorList _list )
    {
        base.OnListChanged(_list);
    }

}
