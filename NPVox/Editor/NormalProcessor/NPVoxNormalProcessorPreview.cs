using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteInEditMode]
public class NPVoxNormalProcessorPreview : EditorWindow
{
    protected NPVoxNormalProcessorPreviewContext m_context = null;

    protected string m_title = "";

    protected Vector2 m_mousePosition = Vector2.zero;
    protected Vector2 m_mousePositionPrevious = Vector2.zero;

    protected PreviewRenderUtility m_renderer = null;
    protected MeshFilter m_meshFilter = null;
    protected MeshRenderer m_meshRenderer = null;

    protected bool[] m_mouseDown = { false, false, false };
    protected Vector2 m_mouseRotate = Vector2.zero;
    protected Vector2 m_mousePan = Vector2.zero;
    protected float m_mouseZoom = 0.0f;
    protected float m_sensitivityDrag = 0.05f;
    protected float m_sensitivityOrient = 0.5f;
    protected float m_sensitivityZoom = 0.1f;
    protected Rect m_sceneRect = new Rect();
    protected int m_cameraType = 0;

    protected bool m_previewGUIDrawOutlines = false;
    protected bool m_previewGUIDrawNormals = false;

    protected Matrix4x4 m_meshTransform = Matrix4x4.identity;

    protected static Material s_materialHandles = null;

    protected static readonly Color s_colorBG = new Color( 0.3f, 0.3f, 0.3f );
    protected static readonly Color s_colorBox = new Color( 0.5f, 0.5f, 0.5f );

    public static NPVoxNormalProcessorPreview ShowWindow( Type _processorType )
    {
        if ( s_materialHandles == null )
        {
            s_materialHandles = ( Material ) EditorGUIUtility.LoadRequired( "Assets/Vendor/UnityVoxelTools/NPVox/Materials/GL.mat" );
        }

        foreach ( Type type in NPipeReflectionUtil.GetAllTypesWithAttribute( typeof( NPVoxAttributeNormalProcessorPreview ) ) )
        {
            if ( type.BaseType == typeof( NPVoxNormalProcessorPreview ) )
            {
                NPVoxAttributeNormalProcessorPreview attr = NPipeReflectionUtil.GetAttribute<NPVoxAttributeNormalProcessorPreview>( type );
                if ( attr != null && attr.ProcessorType == _processorType )
                {
                    return ( NPVoxNormalProcessorPreview ) GetWindow( type, false, "Normal Editor", true );
                }
            }
        }
        
        return GetWindow<NPVoxNormalProcessorPreview>( "Normal Editor", true );
    }

    public static NPVoxNormalProcessorPreview ShowWindow()
    {
        if ( s_materialHandles == null )
        {
            s_materialHandles = ( Material ) EditorGUIUtility.LoadRequired( "Assets/Vendor/UnityVoxelTools/NPVox/Materials/GL.mat" );
        }

        return GetWindow<NPVoxNormalProcessorPreview>( "Normal Editor", true );
    }

    public virtual void SetContext( NPVoxNormalProcessorPreviewContext _context )
    {
        if ( m_context != null )
        {
            m_context.Invalidate();
        }

        m_context = _context;
        NPVoxNormalProcessor processor = m_context.ViewedProcessor;
        NPVoxAttributeNormalProcessorListItem listItemAttribute = NPipeReflectionUtil.GetAttribute<NPVoxAttributeNormalProcessorListItem>( processor );
        m_title = listItemAttribute.EditorName;
        
        InitScene();
    }

    void Update()
    {
        if ( m_context != null && m_context.IsValid )
        {
            UpdateScene();
            Repaint();
        }
    }

    void OnEnable()
    {
    }

    void OnDestroy()
    {
        if ( m_context != null )
        {
            m_context.Invalidate();
            m_context = null;
        }

        if ( m_renderer != null )
        {
            m_renderer.Cleanup();
        }
    }


    private void EnableSceneObjects( bool _bEnable )
    {
        //m_context.PreviewObject.SetActive( _bEnable );
        for ( int i = 0; i < m_renderer.camera.transform.childCount; i++ )
        {
            m_renderer.camera.transform.GetChild( i ).gameObject.SetActive( _bEnable );
        }
    }

    void InitScene()
    {
        m_renderer = new PreviewRenderUtility();
        m_renderer.camera.clearFlags = CameraClearFlags.SolidColor;
        m_renderer.camera.backgroundColor = s_colorBG;
        m_renderer.cameraFieldOfView = 60.0f;
        m_renderer.camera.nearClipPlane = 0.3f;
        m_renderer.camera.farClipPlane = 1000.0f;
        InitCamera();
        InitObjectForPreview( m_context.PreviewObject );

        // Disable basic lights
        for ( int i = 0; i < m_renderer.lights.Length; i++ )
        {
            m_renderer.lights[ i ].intensity = 0;
        }

        // Setup custom camera lights
        GameObject camLightObject = new GameObject( "keyLight" );
        camLightObject.transform.parent = m_renderer.camera.transform;
        camLightObject.transform.localPosition = new Vector3( 6.0f, 0.0f, 1.0f );
        Light camLight = camLightObject.AddComponent<Light>();
        camLight.intensity = 1.0f;
        camLight.range = 20.0f;

        camLightObject = new GameObject( "fillLight" );
        camLightObject.transform.parent = m_renderer.camera.transform;
        camLightObject.transform.localPosition = new Vector3( -10.0f, -1.0f, 0.0f );
        camLight = camLightObject.AddComponent<Light>();
        camLight.intensity = 3.0f;
        camLight.range = 20.0f;

        EnableSceneObjects( false );
    }


    void InitObjectForPreview( GameObject _object )
    {
        m_meshFilter = _object.GetComponent<MeshFilter>();
        m_meshRenderer = _object.GetComponent<MeshRenderer>();
    }

    private void InitCamera()
    {
        m_renderer.camera.transform.position = new Vector3( 0, 0, -10 );
        m_renderer.camera.transform.LookAt( m_context.PreviewObject.transform );
        m_context.m_camera = m_renderer.camera;
    }

    void OnGUI()
    {
        if ( m_context != null && m_context.IsValid )
        {
            EnableSceneObjects( true );

            // store previous UI states to process state switches later
            int cameraTypePrevious = m_cameraType;

            // Setup styles
            GUIStyle noStretch = new GUIStyle();
            noStretch.stretchWidth = false;
            noStretch.stretchHeight = false;
            GUILayoutOption[] noFill = { GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( false ) };
            GUILayoutOption[] fill = { GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) };

            // Draw GUI
            GUILayout.BeginHorizontal( GUILayout.ExpandWidth( false ) );

            // Draw navigation tool bar
            GUILayout.BeginVertical( noStretch, noFill );
            GUILayout.Label( m_title, noFill );
            GUILayout.Space( 8.0f );
            m_cameraType = NPipeGUILayout.Toolbar( "Camera: ", m_cameraType, new string[] { "Free", "Orbit" }, noFill );
            GUILayout.Space( 8.0f );
            GUILayout.Label( "Sensitivity:", noFill );
            float fLabelWidthSliders = 50.0f;
            m_sensitivityOrient = NPipeGUILayout.HorizontalSlider( "Rotate:", fLabelWidthSliders, m_sensitivityOrient, 0.01f, 1.0f, GUILayout.Width( 100.0f ) ); GUILayout.Space( -6 );
            m_sensitivityDrag = NPipeGUILayout.HorizontalSlider( "Pan:", fLabelWidthSliders, m_sensitivityDrag, 0.01f, 0.1f, GUILayout.Width( 100.0f ) ); GUILayout.Space( -6 );
            m_sensitivityZoom = NPipeGUILayout.HorizontalSlider( "Zoom:", fLabelWidthSliders, m_sensitivityZoom, 0.01f, 1.0f, GUILayout.Width( 100.0f ) ); GUILayout.Space( -6 );
            GUILayout.Label( "______________________", noFill );

            if ( GUILayout.Button( m_previewGUIDrawOutlines ? "Hide Outlines" : "Show Outlines", noFill ) )
            {
                m_previewGUIDrawOutlines = !m_previewGUIDrawOutlines;
            }

            if ( GUILayout.Button( m_previewGUIDrawNormals ? "Hide Normals" : "Show Normals", noFill ) )
            {
                m_previewGUIDrawNormals = !m_previewGUIDrawNormals;
            }

            GUILayout.Label( "______________________", noFill );
            OnGUIInternal();

            GUILayout.EndVertical();

            // Draw preview
            GUILayout.Box( "", fill );
            m_sceneRect = GUILayoutUtility.GetLastRect();
            DrawScene( m_sceneRect );
            GUILayout.EndHorizontal();

            EnableSceneObjects( false );

            UpdateInput();

            // Process GUI state switches
            if ( cameraTypePrevious != m_cameraType )
            {
                InitCamera();
            }
        }
        else
        {
            GUILayout.Label("Invalid preview context!");
        }
    }

    private void DrawScene( Rect _rect )
    {
        if ( Event.current.type == EventType.Repaint )
        {
            m_renderer.BeginPreview( _rect, GUIStyle.none );

            m_renderer.DrawMesh( m_context.PreviewMesh, m_meshTransform, m_meshRenderer.sharedMaterial, 0 );

            m_renderer.camera.Render();

            NPipeGL.PostRenderBegin( m_renderer.camera.projectionMatrix, m_renderer.camera.worldToCameraMatrix, s_materialHandles );

            Transform t = m_context.PreviewObject.transform;
            Bounds box = m_meshFilter.sharedMesh.bounds;
            Vector3 extent = box.extents;

            NPipeGL.DrawParallelepiped(
                t.position - new Vector3( extent.x, extent.y, extent.z ),
                new Vector3( box.size.x, 0, 0 ),
                new Vector3( 0, box.size.y, 0 ),
                new Vector3( 0, 0, box.size.z ),
                s_colorBox );

            NPVoxMeshData[] voxMeshData = m_context.MeshOutput.GetVoxMeshData();
            Vector3 voxSize = m_context.MeshOutput.VoxelSize;
            Vector3 voxExtent = voxSize * 0.5f;
            Color cOutline = new Color( 0.3f, 0.3f, 0.3f );
            Color cNormals = new Color( 0.4f, 0.4f, 0.4f );

            Vector3 v1 = new Vector3( voxSize.x, 0, 0 );
            Vector3 v2 = new Vector3( 0, voxSize.y, 0 );
            Vector3 v3 = new Vector3( 0, 0, voxSize.z );

            Vector3[] normals = m_context.PreviewMesh.normals;

            foreach ( NPVoxMeshData vox in voxMeshData )
            {
                if ( !vox.isHidden )
                {
                    if ( m_previewGUIDrawOutlines )
                    {
                        Vector3 voxPosition = new Vector3( vox.voxelCenter.x, vox.voxelCenter.y, vox.voxelCenter.z );
                        NPipeGL.DrawParallelepiped( voxPosition - voxExtent, v1, v2, v3, cOutline );
                    }

                    if ( m_previewGUIDrawNormals )
                    {
                        NPipeGL.DrawLine( vox.voxelCenter, vox.voxelCenter + normals[ vox.vertexIndexOffsetBegin ] * voxSize.x, cNormals );
                    }
                }
            }

            DrawSceneInternal( _rect );

            NPipeGL.PostRenderEnd();

            GUI.DrawTexture( _rect, m_renderer.EndPreview() );
        }
    }

    void UpdateInput()
    {
        // Handle mouse
        Event currentEvent = Event.current;

        switch ( currentEvent.type )
        {
            case EventType.MouseDown:
                if ( m_sceneRect.Contains( currentEvent.mousePosition ) )
                {
                    m_mouseDown[ currentEvent.button ] = true;
                }
                break;

            case EventType.MouseUp: m_mouseDown[ currentEvent.button ] = false;
                break;

            case EventType.MouseDrag:
                if ( !currentEvent.shift && !currentEvent.alt && !currentEvent.control )
                {
                    if ( m_mouseDown[ 0 ] || m_mouseDown[ 2 ] )
                    {
                        if ( m_mouseDown[ 1 ] )
                        {
                            m_mouseRotate += currentEvent.delta;
                        }
                        else
                        {
                            m_mousePan += currentEvent.delta;
                        }
                    }
                    else if ( m_mouseDown[ 1 ] )
                    {
                        m_mouseRotate += currentEvent.delta;
                    }
                }
                break;

            case EventType.ScrollWheel:
                if ( !currentEvent.shift && !currentEvent.alt && !currentEvent.control )
                {
                    m_mouseZoom += currentEvent.delta.y;
                }
                break;

            default:
                break;
        }

        UpdateInputInternal();
    }
    
    void UpdateScene()
    {
        if ( m_renderer != null )
        {
            if (m_cameraType == 0)
            {
                Vector3 currentRotation = m_renderer.camera.transform.rotation.eulerAngles;

                m_renderer.camera.transform.rotation = Quaternion.Euler(new Vector3(
                    currentRotation.x + m_mouseRotate.y * m_sensitivityOrient,
                    currentRotation.y + m_mouseRotate.x * m_sensitivityOrient,
                    currentRotation.z));

                m_renderer.camera.transform.position += m_renderer.camera.transform.forward * -m_mouseZoom * m_sensitivityZoom;
                m_renderer.camera.transform.position += m_renderer.camera.transform.right * -m_mousePan.x * m_sensitivityDrag;
                m_renderer.camera.transform.position += m_renderer.camera.transform.up * m_mousePan.y * m_sensitivityDrag;
            }
            else if (m_cameraType == 1)
            {
                Vector3 currentRotation = m_renderer.camera.transform.rotation.eulerAngles;
                float distance = m_renderer.camera.transform.position.magnitude;

                m_renderer.camera.transform.rotation = Quaternion.Euler(new Vector3(
                    currentRotation.x + m_mouseRotate.y * m_sensitivityOrient,
                    currentRotation.y + m_mouseRotate.x * m_sensitivityOrient,
                    currentRotation.z));

                m_renderer.camera.transform.position = -m_renderer.camera.transform.forward * (distance + m_mouseZoom * m_sensitivityZoom);
            }

            // Reset input states
            m_mouseZoom = 0.0f;
            m_mouseRotate = Vector2.zero;
            m_mousePan = Vector2.zero;

            UpdateSceneInternal();
        }
    }

    protected virtual void OnGUIInternal() { }
    protected virtual void DrawSceneInternal( Rect _rect ) { }
    protected virtual void UpdateInputInternal() { }
    protected virtual void UpdateSceneInternal() { }
}
