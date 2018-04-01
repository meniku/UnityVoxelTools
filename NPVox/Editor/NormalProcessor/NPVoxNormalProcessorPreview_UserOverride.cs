using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ NPVoxAttributeNormalProcessorPreview( typeof( NPVoxNormalProcessor_UserOverride ) ) ]
public class NPVoxNormalProcessorPreview_UserOverride : NPVoxNormalProcessorPreview
{
    private sbyte[] m_selections;

    private NPVoxCoord m_lastSelected = NPVoxCoord.INVALID;
    private NPVoxMeshData m_lastSelectedData = null;

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

        InitMeshNormals();
    }

    private void InitMeshNormals()
    {
        NPVoxMeshData[] voxMeshData = m_context.MeshOutput.GetVoxMeshData();
        NPVoxNormalProcessor previousProcessor = m_context.MeshOutput.NormalProcessors.GetPreviousInList( m_context.ViewedProcessor );

        Vector3[] normals = null;

        if ( previousProcessor == null || !previousProcessor.IsOutputValid() )
        {
            normals = new Vector3[ m_context.PreviewMesh.normals.Length ];
            for ( int i = 0; i < normals.Length; i++ )
            {
                normals[ i ] = Vector3.zero;
            }

            if ( previousProcessor != null )
            {
                m_context.MeshOutput.NormalProcessors.Run( m_context.MeshOutput.GetVoxModel(), voxMeshData, normals, normals, previousProcessor );
            }
        }
        else
        {
            normals = previousProcessor.GetOutputCopy();
        }

        m_context.ViewedProcessor.InitOutputBuffer( normals.Length );
        m_context.ViewedProcessor.Process( m_context.MeshOutput.GetVoxModel(), voxMeshData, normals, normals );
        Vector3[] meshNormals = m_context.PreviewMesh.normals;
        for ( int i = 0; i < meshNormals.Length; i++ )
        {
            meshNormals[ i ] = normals[ i ];
        }
        m_context.PreviewMesh.normals = meshNormals;
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
            GUILayout.Label("APPLY TO INPUT:", noFill);
            if ( GUILayout.Button("Source (Q)", widthWideButton))
            {
                Handle_SourceToInput();
            }
            GUI.backgroundColor = currentColor;

            GUILayout.Space( 12.0f );
            currentColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColorTarget;
            GUILayout.Label( "APPLY TO TARGET:", noFill );

            if ( GUILayout.Button( "Input Field (W)", widthWideButton ) )  { Handle_InputToTarget(); }
            if ( GUILayout.Button( "Source (S)", widthWideButton ) ) { Handle_SourceToTarget(); }
            if ( GUILayout.Button( "Source + Input Field (X)", widthWideButton ) ) { Handle_SourceInputToTarget(); }
            
            GUI.backgroundColor = currentColor;
            GUILayout.Space( 12.0f );

            if ( GUILayout.Button( "RESET SELECTION", widthWideButton ) ) { ResetSelection(); }

            // Selected voxel info
            GUILayout.Space( 12.0f );
            string selectedCoord = "-";
            string selectedNormalX = "-";
            string selectedNormalY = "-";
            string selectedNormalZ = "-";
            string selectedVIndex = "-";

            if ( m_lastSelected.Valid && m_lastSelectedData != null )
            {
                selectedCoord = m_lastSelected.X + " " + m_lastSelected.Y + " " + m_lastSelected.Z;
                selectedVIndex = m_lastSelectedData.vertexIndexOffsetBegin.ToString();
                Vector3 normal = m_context.PreviewMesh.normals[ m_lastSelectedData.vertexIndexOffsetBegin ];
                selectedNormalX = normal.x.ToString();
                selectedNormalY = normal.y.ToString();
                selectedNormalZ = normal.z.ToString();
            }

            int labelWidth = 65, columnWidth = 150, columnSpace = -5;
            GUILayout.Label( "Last Selected:", noFill );
            NPipeGUILayout.TableRow( "Coord:", labelWidth, columnSpace, new NPipeGUILayout.TableColumn( selectedCoord, columnWidth ) );
            NPipeGUILayout.TableRow( "v-Index:", labelWidth, columnSpace, new NPipeGUILayout.TableColumn( selectedVIndex, columnWidth ) );
            NPipeGUILayout.TableRow( "Normal X:", labelWidth, columnSpace, new NPipeGUILayout.TableColumn( selectedNormalX, columnWidth ) );
            NPipeGUILayout.TableRow( "Normal Y:", labelWidth, columnSpace, new NPipeGUILayout.TableColumn( selectedNormalY, columnWidth ) );
            NPipeGUILayout.TableRow( "Normal Z:", labelWidth, columnSpace, new NPipeGUILayout.TableColumn( selectedNormalZ, columnWidth ) );

            // Reset override functions
            GUILayout.BeginVertical( GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( true ) );
            GUILayout.EndVertical();

            if ( GUILayout.Button( "RESET SELECTED OVERRIDES", widthWideButton ) )
            {
                NPVoxNormalProcessor_UserOverride processor = ( NPVoxNormalProcessor_UserOverride ) m_context.ViewedProcessor;
                
                foreach ( NPVoxMeshData vox in m_context.MeshOutput.GetVoxMeshData() )
                {
                    if ( !vox.isHidden )
                    {
                        sbyte selection = GetSelection( vox.voxCoord );
                        if ( selection == SELECTED_TARGET )
                        {
                            processor.m_overrideNormalsRT.Remove(vox.voxCoord);
                        }
                    }
                }

                ResetSelection();
                InitMeshNormals();
            }

            GUILayout.Space( 12.0f );

            if ( GUILayout.Button( "RESET ALL OVERRIDES", widthWideButton ) )
            {
                NPVoxNormalProcessor_UserOverride processor = ( NPVoxNormalProcessor_UserOverride ) m_context.ViewedProcessor;
                processor.m_overrideNormalsRT.Clear();
                InitMeshNormals();
            }

            GUILayout.Space( 12.0f );
        }
        else
        {
            Color currentColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColorWarning;
            if ( GUILayout.Button( "Recalculate normals", noFill ) )
            {
                InitMeshNormals();
            }
            GUI.backgroundColor = currentColor;
        }
    }

    #region BUTTON HANDLERS

    protected void Handle_SourceToInput()
    {
        Vector3 average = Vector3.zero;
        bool success = ComputeSourceAverage(ref average);
        if (average.sqrMagnitude > 0)
        {
            average = average.normalized;
        }

        if (success)
        {
            m_normalField = average;
            ResetSelection();
        }
    }

    protected void Handle_SourceToTarget()
    {
        Vector3 average = Vector3.zero;
        bool success = ComputeSourceAverage(ref average);
        if (average.sqrMagnitude > 0)
        {
            average = average.normalized;
        }

        if (success)
        {
            List<int> indices = GetSelectedIndices(SELECTED_TARGET);
            foreach (int i in indices)
            {
                m_normalStage.Add(i, average);
            }
            ApplyNormalStage();
            m_normalStage.Clear();
            ResetSelection();
        }
    }

    protected void Handle_InputToTarget()
    {
        List<int> indices = GetSelectedIndices(SELECTED_TARGET);
        foreach (int i in indices)
        {
            m_normalStage.Add(i, m_normalField);
        }
        ApplyNormalStage();
        m_normalStage.Clear();
        ResetSelection();
    }

    protected void Handle_SourceInputToTarget()
    {
        Vector3 average = Vector3.zero;
        bool success = ComputeSourceAverage(ref average, true);
        if (average.sqrMagnitude > 0)
        {
            average = average.normalized;
        }

        if (success)
        {
            List<int> indices = GetSelectedIndices(SELECTED_TARGET);
            foreach (int i in indices)
            {
                m_normalStage.Add(i, average);
            }
            ApplyNormalStage();
            m_normalStage.Clear();
            ResetSelection();
        }
    }

    #endregion

    protected override void DrawSceneInternal( Rect _rect )
    {
        if ( m_context.ViewedProcessor.IsOutputValid() )
        {
            Vector3 voxSize = m_context.MeshOutput.VoxelSize;
            Vector3 voxExtent = voxSize * 0.5f;
            Vector3 v1 = new Vector3( voxSize.x, 0, 0 );
            Vector3 v2 = new Vector3( 0, voxSize.y, 0 );
            Vector3 v3 = new Vector3( 0, 0, voxSize.z );

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

                    if ( vox.voxCoord.Equals( m_lastSelected ) )
                    {
                        m_lastSelectedData = vox;
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

                case EventType.KeyDown:
                    {
                        switch ( e.keyCode )
                        {
                            case KeyCode.Q:
                                Handle_SourceToInput();
                                break;

                            case KeyCode.W:
                                Handle_InputToTarget();
                                break;

                            case KeyCode.S:
                                Handle_SourceToTarget();
                                break;

                            case KeyCode.X:
                                Handle_SourceInputToTarget();
                                break;

                            default:
                                break;
                        }
                    }
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

        m_lastSelected = NPVoxCoord.INVALID;
        m_lastSelectedData = null;
    }

    private void ApplyNormalStage()
    {
        NPVoxModel voxModel = m_context.MeshOutput.GetVoxModel();
        Vector3[] normals = m_context.PreviewMesh.normals;

        NPVoxNormalProcessor_UserOverride processor = ( NPVoxNormalProcessor_UserOverride ) m_context.ViewedProcessor;

        foreach ( NPVoxMeshData vox in m_context.MeshOutput.GetVoxMeshData() )
        {
            if ( !vox.isHidden )
            {
                NPVoxCoord coord = vox.voxCoord;
                int index = voxModel.GetIndex( coord );

                if ( GetSelection( coord ) == SELECTED_TARGET )
                {
                    processor.m_overrideNormalsRT[vox.voxCoord] = m_normalStage[index];

                    for ( int i = 0; i < vox.numVertices; i++ )
                    {
                        normals[vox.vertexIndexOffsetBegin + i] = m_normalStage[index];
                    }
                }
            }
        }

        RemoveRedundantOverrides();

        UnityEditor.EditorUtility.SetDirty( processor );

        m_context.PreviewMesh.normals = normals;
    }

    private void RemoveRedundantOverrides()
    {
        List<NPVoxCoord> overridesToRemove = new List<NPVoxCoord>();

        NPVoxNormalProcessor_UserOverride processor = ( NPVoxNormalProcessor_UserOverride ) m_context.ViewedProcessor;
        NPVoxNormalProcessor previous = m_context.MeshOutput.NormalProcessors.GetPreviousInList( processor );

        foreach ( NPVoxMeshData vox in m_context.MeshOutput.GetVoxMeshData() )
        {
            if ( !vox.isHidden )
            {
                Vector3 normal = Vector3.zero;
                if (previous != null)
                {
                    normal = previous.GetOutput()[vox.vertexIndexOffsetBegin];
                }

                if (processor.m_overrideNormalsRT.ContainsKey(vox.voxCoord) && processor.m_overrideNormalsRT[vox.voxCoord] == normal)
                {
                    overridesToRemove.Add(vox.voxCoord);
                }
            }
        }

        foreach(NPVoxCoord i in overridesToRemove )
        {
            processor.m_overrideNormalsRT.Remove( i );
        }
    }

    private bool ComputeSourceAverage( ref Vector3 _out, bool _includeInputField = false )
    {
        NPVoxModel voxModel = m_context.MeshOutput.GetVoxModel();
        List<Vector3> normalsSum = new List<Vector3>();

        Vector3[] normalsMesh = m_context.PreviewMesh.normals;

        foreach ( NPVoxMeshData vox in m_context.MeshOutput.GetVoxMeshData() )
        {
            if ( !vox.isHidden )
            {
                NPVoxCoord coord = vox.voxCoord;
                int index = voxModel.GetIndex( coord );

                if ( GetSelection( coord ) == SELECTED_SOURCE )
                {
                    normalsSum.Add( normalsMesh[ vox.vertexIndexOffsetBegin ] );
                }
            }
        }

        if ( _includeInputField )
        {
            normalsSum.Add(m_normalField);
        }
        
        bool bResult = MathUtilities.Statistical.ComputeAverage( normalsSum, ref _out );
        return bResult;
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
        m_lastSelected = _coord;
    }
}
