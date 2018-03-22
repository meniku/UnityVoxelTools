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

    private Vector3 m_normalField = Vector3.zero;

    private Dictionary<int, Vector3> m_normalStage = new Dictionary<int, Vector3>();

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
        GUILayoutOption widthSmallButton = GUILayout.Width( 65 );
        GUILayoutOption widthWideButton = GUILayout.Width( 203 );
        GUILayoutOption[] noFill = { GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( false ) };
        GUILayoutOption[] fill = { GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) };
        Color bgColorWarning = new Color( 0.8f, 0.3f, 0.3f );
        Color bgColorTarget = new Color( 0.9f, 0.4f, 0.4f );
        Color bgColorInput = new Color( 0.5f, 0.7f, 0.9f );

        if ( m_context.ViewedProcessor.IsOutputValid() )
        {
            GUILayout.Space( 12.0f );
            Color currentColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColorInput;
            GUILayout.Label( "INPUT:", noFill );
            GUILayout.Space( -3 );
            m_normalField = EditorGUILayout.Vector3Field( "", m_normalField, noFill );
            GUILayout.Space( -3 );
            GUILayout.BeginHorizontal( noStretch, noFill );
            if ( GUILayout.Button( "RIGHT", widthSmallButton ) ) { m_normalField = Vector3.right; }
            if ( GUILayout.Button( "UP", widthSmallButton ) ) { m_normalField = Vector3.up; }
            if ( GUILayout.Button( "FWD", widthSmallButton ) ) { m_normalField = Vector3.forward; }
            GUILayout.EndHorizontal();
            GUILayout.Space( -4 );
            GUILayout.BeginHorizontal( noStretch, noFill );
            if ( GUILayout.Button( "LEFT", widthSmallButton ) ) { m_normalField = Vector3.left; }
            if ( GUILayout.Button( "DOWN", widthSmallButton ) ) { m_normalField = Vector3.down; }
            if ( GUILayout.Button( "BACK", widthSmallButton ) ) { m_normalField = Vector3.back; }
            GUILayout.EndHorizontal();
            GUILayout.Space( -4 );
            GUILayout.BeginHorizontal( noStretch, noFill );
            if ( GUILayout.Button( "Normalize", widthWideButton ) ) { m_normalField = m_normalField.normalized; }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = currentColor;

            GUILayout.Space( 12.0f );
            currentColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColorTarget;
            if ( GUILayout.Button( "INPUT to TARGET", widthWideButton ) )
            {
                List<int> indices = GetSelectedIndices( SELECTED_TARGET );
                foreach ( int i in indices )
                {
                    m_normalStage.Add( i, m_normalField );
                }
                ApplyNormalStage();
                m_normalStage.Clear();
            }

            if ( GUILayout.Button( "SRC average to TARGET", widthWideButton ) )
            {
            }

            if ( GUILayout.Button( "Neighbor average to TARGET", widthWideButton ) )
            {
                // TODO: Apply normals
            }
            
            GUI.backgroundColor = currentColor;

            GUILayout.Space( 12.0f );
            if ( GUILayout.Button( "RESET SELECTION", widthWideButton ) )
            {
                ResetSelection();
            }
        }
        else
        {
            Color currentColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColorWarning;
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

    private void ApplyNormalStage()
    {
        NPVoxModel voxModel = m_context.MeshOutput.GetVoxModel();
        Vector3[] normals = m_context.PreviewMesh.normals;

        foreach ( NPVoxMeshData vox in m_context.MeshOutput.GetVoxMeshData() )
        {
            if ( !vox.isHidden )
            {
                NPVoxCoord coord = vox.voxCoord;
                if ( GetSelection( coord ) == SELECTED_TARGET )
                {
                    // TODO: Apply to normal processor

                    int index = voxModel.GetIndex( coord );

                    for ( int i = 0; i < vox.numVertices; i++ )
                    {
                        normals[ vox.vertexIndexOffsetBegin + i ] = m_normalStage[ index ];
                    }
                }
            }
        }

        m_context.PreviewMesh.normals = normals;
    }

    private List<int> GetSelectedIndices( sbyte _selectionType )
    {
        List<int> indices = new List<int>();
        foreach ( NPVoxMeshData vox in m_context.MeshOutput.GetVoxMeshData() )
        {
            if ( !vox.isHidden )
            {
                if ( GetSelection( vox.voxCoord ) == _selectionType )
                {
                    indices.Add( m_context.MeshOutput.GetVoxModel().GetIndex( vox.voxCoord ) );
                }
            }
        }
        return indices;
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
