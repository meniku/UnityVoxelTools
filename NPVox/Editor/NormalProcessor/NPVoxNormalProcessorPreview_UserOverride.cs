using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ NPVoxAttributeNormalProcessorPreview( typeof( NPVoxNormalProcessor_UserOverride ) ) ]
public class NPVoxNormalProcessorPreview_UserOverride : NPVoxNormalProcessorPreview
{
    private Ray m_rayA = new Ray( Vector3.zero, Vector3.zero );
    private Ray m_rayB = new Ray( Vector3.zero, Vector3.zero );
    private NPVoxCoord m_selected = new NPVoxCoord(-1, -1, -1);

    private sbyte[] m_selections;

    private const sbyte UNSELECTED = 0x00;
    private const sbyte SELECTED_TARGET = 0x01;
    private const sbyte SELECTED_SOURCE = 0x02;

    private static sbyte m_currentSelectionMode = UNSELECTED;

    public override void SetContext( NPVoxNormalProcessorPreviewContext _context )
    {
        base.SetContext( _context );

        NPVoxCoord size = m_context.MeshOutput.GetVoxModel().Size;
        m_selections = new sbyte[ size.X * size.Y * size.Z ];
        for ( int i = 0; i < m_selections.Length; i++ )
        {
            m_selections[ i ] = UNSELECTED;
        }
    }

    protected override void OnGUIInternal()
    {
        GUIStyle noStretch = new GUIStyle();
        noStretch.stretchWidth = false;
        noStretch.stretchHeight = false;
        GUILayoutOption[] noFill = { GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( false ) };
        GUILayoutOption[] fill = { GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) };
        Color backgroundColorWarning = new Color( 0.8f, 0.3f, 0.3f );
        


        if ( m_context.ViewedProcessor.IsOutputValid() )
        {

        }
        else
        {
            Color currentColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColorWarning;
            if ( GUILayout.Button( "Recalculate normals", noFill ) )
            {

            }
            GUI.backgroundColor = currentColor;
        }
    }

    protected override void DrawSceneInternal( Rect _rect )
    {
        if ( m_context.ViewedProcessor.IsOutputValid() )
        {
            Vector3 voxSize = m_context.MeshOutput.VoxelSize;
            Vector3 voxExtent = voxSize * 0.5f;
            Vector3 v1 = new Vector3( voxSize.x, 0, 0 );
            Vector3 v2 = new Vector3( 0, voxSize.y, 0 );
            Vector3 v3 = new Vector3( 0, 0, voxSize.z );

            //if ( m_selected.Valid )
            //{
            //    Vector3 voxPosition = m_context.VoxToUnity.ToUnityPosition( m_selected );
            //    NPipeGL.DrawParallelepiped( voxPosition - voxExtent, v1, v2, v3, Color.red );
            //}

            NPVoxMeshData[] voxMeshData = m_context.MeshOutput.GetVoxMeshData();

            foreach ( NPVoxMeshData vox in voxMeshData )
            {
                if ( !vox.isHidden )
                {
                    sbyte selection = GetSelection( vox.voxCoord );
                    if ( selection != UNSELECTED )
                    {
                        Vector3 voxPosition = new Vector3( vox.voxelCenter.x, vox.voxelCenter.y, vox.voxelCenter.z );
                        NPipeGL.DrawParallelepiped( voxPosition - voxExtent, v1, v2, v3, selection == SELECTED_TARGET ? Color.red : Color.green );
                    }
                }
            }
        }
    }

    protected override void UpdateInputInternal()
    {
        if ( m_context.ViewedProcessor.IsOutputValid() )
        {
            Event e = Event.current;

            switch ( e.type )
            {
                case EventType.MouseDown:
                    HandleRays( true );
                    break;

                case EventType.MouseDrag:
                    HandleRays( false );
                    break;

                case EventType.MouseUp:
                    m_currentSelectionMode = UNSELECTED;
                    break;
            }
        }
    }

    private void HandleRays( bool _begin )
    {
        if ( m_context.ViewedProcessor.IsOutputValid() )
        {
            Event e = Event.current;
            Vector2 rayScreenPosition = new Vector2( e.mousePosition.x - m_sceneRect.xMin, m_sceneRect.height - ( e.mousePosition.y - m_sceneRect.yMin ) );
            sbyte selectionMode = UNSELECTED;

            if ( e.shift )
            {
                selectionMode = SELECTED_TARGET;
            }
            else if ( e.control )
            {
                selectionMode = SELECTED_SOURCE;
            }
            else
            {
                m_currentSelectionMode = UNSELECTED;
            }

            if ( selectionMode != UNSELECTED )
            {
                Ray ray = m_context.m_camera.ScreenPointToRay( rayScreenPosition );
                NPVoxRayCastHit hit = m_context.VoxToUnity.Raycast( ray, m_context.PreviewObject.transform, 50 );
                if ( hit.IsHit )
                {
                    sbyte selection = GetSelection( hit.Coord );

                    if ( _begin )
                    {
                        switch ( selection )
                        {
                            case UNSELECTED:
                                m_currentSelectionMode = selectionMode;
                                break;
                            case SELECTED_TARGET:
                            case SELECTED_SOURCE:
                                if ( selectionMode == selection )
                                {
                                    m_currentSelectionMode = UNSELECTED;
                                }
                                else
                                {
                                    m_currentSelectionMode = selectionMode;
                                }
                                break;
                        }
                    }

                    SetSelection( hit.Coord, m_currentSelectionMode );
                }
            }
        }
    }

    protected override void UpdateSceneInternal()
    {
    }

    private void ResetSelection()
    {
        for ( int i = 0; i < m_selections.Length; i++ )
        {
            m_selections[ i ] = 0x00;
        }
    }

    private sbyte GetSelection( NPVoxCoord _coord )
    {
        int iIndex = m_context.MeshOutput.GetVoxModel().GetIndex( _coord );
        return m_selections[ iIndex ];
    }

    private void SetSelection( NPVoxCoord _coord, sbyte _selection )
    {
        int iIndex = m_context.MeshOutput.GetVoxModel().GetIndex( _coord );
        m_selections[ iIndex ] = _selection;
    }
}
