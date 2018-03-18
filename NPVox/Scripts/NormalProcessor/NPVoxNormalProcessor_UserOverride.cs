using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[NPVoxAttributeNormalProcessorListItem( "User Override", typeof( NPVoxNormalProcessor_UserOverride ), NPVoxNormalProcessorType.Generator )]
public class NPVoxNormalProcessor_UserOverride : NPVoxNormalProcessor
{
    [SerializeField]
    private int[] m_overrideNormalIndices = null;

    [SerializeField]
    private Vector3[] m_overrideNormals = null;

    NPVoxNormalProcessorPass_ApplyNormals m_passApplyNormals;


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
        m_passApplyNormals.m_iIndices = m_overrideNormalIndices;
        m_passApplyNormals.m_iNormals = m_overrideNormals;
    }

    public override void OnListChanged( NPVoxNormalProcessorList _list )
    {
    }

    protected override void OnPreviewSceneInternal( NPVoxNormalProcessorPreviewContext _context, Mesh _previewMesh )
    {
    }

    public override void OnPreviewInput( NPVoxNormalProcessorPreviewContext _context, Event _event, Rect _rect )
    {
       
    }
}
