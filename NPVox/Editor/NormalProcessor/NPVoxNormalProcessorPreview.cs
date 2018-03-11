using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteInEditMode]
public class NPVoxNormalProcessorPreview : EditorWindow
{
    private NPVoxNormalProcessorPreviewContext m_context = null;

    private string m_title = "";

    Vector2 m_mousePosition = Vector2.zero;
    Vector2 m_mousePositionPrevious = Vector2.zero;

    PreviewRenderUtility m_renderer = null;
    MeshFilter m_meshFilter = null;
    MeshRenderer m_meshRenderer = null;

    bool[] m_mouseDown = { false, false, false };
    Vector2 m_mouseRotate = Vector2.zero;
    Vector2 m_mousePan = Vector2.zero;
    float m_mouseZoom = 0.0f;
    float m_sensitivityDrag = 0.1f;
    float m_sensitivityOrient = 0.5f;
    float m_sensitivityZoom = 0.1f;
    Rect m_sceneRect = new Rect();
    int m_cameraType = 0;

    Matrix4x4 m_meshTransform = Matrix4x4.identity;
    
    public static NPVoxNormalProcessorPreview ShowWindow()
    {
        return GetWindow<NPVoxNormalProcessorPreview>( "Normal Editor", true );
    }

    public void SetContext( NPVoxNormalProcessorPreviewContext _context )
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
        UpdateScene();
        Repaint();
    }

    void OnEnable()
    {
    }

    void InitScene()
    {
        m_renderer = new PreviewRenderUtility();
        m_renderer.camera.clearFlags = CameraClearFlags.SolidColor;
        m_renderer.camera.backgroundColor = new Color( 0.3f, 0.3f, 0.3f );
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

    void OnDestroy()
    {
        m_context.Invalidate();
        m_context = null;
        m_renderer.Cleanup();
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
            m_cameraType = NPipeGUILayout.Toolbar( "Camera: ", m_cameraType, new string[]{ "Free", "Orbit" }, noFill );
            GUILayout.Space( 8.0f );
            GUILayout.Label( "Sensitivity:", noFill );
            float fLabelWidthSliders = 50.0f;
            m_sensitivityOrient = NPipeGUILayout.HorizontalSlider( "Rotate:", fLabelWidthSliders, m_sensitivityOrient, 0.01f, 1.0f, GUILayout.Width( 100.0f ) ); GUILayout.Space( -6 );
            m_sensitivityDrag = NPipeGUILayout.HorizontalSlider( "Pan:", fLabelWidthSliders, m_sensitivityDrag, 0.01f, 1.0f, GUILayout.Width( 100.0f ) ); GUILayout.Space( -6 );
            m_sensitivityZoom = NPipeGUILayout.HorizontalSlider( "Zoom:", fLabelWidthSliders, m_sensitivityZoom, 0.01f, 1.0f, GUILayout.Width( 100.0f ) ); GUILayout.Space( -6 );
            GUILayout.Label( "______________________", noFill );
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
            // Why is the context invalid?
            if ( m_context != null )
            {

            }
            else
            {
                GUILayout.Label( "The preview context is not set!" );
            }
        }
    }

    void InitObjectForPreview( GameObject _object )
    {
        m_meshFilter = _object.GetComponent<MeshFilter>();
        m_meshRenderer = _object.GetComponent<MeshRenderer>();
        m_meshTransform = Matrix4x4.Rotate( Quaternion.Euler(90, 0, 0) );
    }

    private void InitCamera()
    {
        m_renderer.camera.transform.position = new Vector3( 0, 0, -10 );
        m_renderer.camera.transform.LookAt( m_context.PreviewObject.transform );
    }

    private void DrawScene( Rect _rect )
    {
        m_renderer.BeginPreview( _rect, GUIStyle.none );

        m_renderer.DrawMesh( m_meshFilter.sharedMesh, m_meshTransform, m_meshRenderer.sharedMaterial, 0 );

        m_renderer.camera.Render();

        GUI.DrawTexture( _rect, m_renderer.EndPreview() );
    }

    private void EnableSceneObjects( bool _bEnable )
    {
        //m_context.PreviewObject.SetActive( _bEnable );
        for ( int i = 0; i < m_renderer.camera.transform.childCount; i++ )
        {
            m_renderer.camera.transform.GetChild( i ).gameObject.SetActive( _bEnable );
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
    }

    void UpdateScene()
    {
        if ( m_cameraType == 0 )
        {
            Vector3 currentRotation = m_renderer.camera.transform.rotation.eulerAngles;

            m_renderer.camera.transform.rotation = Quaternion.Euler( new Vector3(
                currentRotation.x + m_mouseRotate.y * m_sensitivityOrient,
                currentRotation.y + m_mouseRotate.x * m_sensitivityOrient,
                currentRotation.z ) );

            m_renderer.camera.transform.position += m_renderer.camera.transform.forward * -m_mouseZoom * m_sensitivityZoom;
            m_renderer.camera.transform.position += m_renderer.camera.transform.right * -m_mousePan.x * m_sensitivityDrag;
            m_renderer.camera.transform.position += m_renderer.camera.transform.up * m_mousePan.y * m_sensitivityDrag;
        }
        else if ( m_cameraType == 1 )
        {
            Vector3 currentRotation = m_renderer.camera.transform.rotation.eulerAngles;
            float distance = m_renderer.camera.transform.position.magnitude;

            m_renderer.camera.transform.rotation = Quaternion.Euler( new Vector3(
                currentRotation.x + m_mouseRotate.y * m_sensitivityOrient,
                currentRotation.y + m_mouseRotate.x * m_sensitivityOrient,
                currentRotation.z ) );

            m_renderer.camera.transform.position = -m_renderer.camera.transform.forward * ( distance + m_mouseZoom * m_sensitivityZoom );
        }

        // Reset input states
        m_mouseZoom = 0.0f;
        m_mouseRotate = Vector2.zero;
        m_mousePan = Vector2.zero;
    }
}
