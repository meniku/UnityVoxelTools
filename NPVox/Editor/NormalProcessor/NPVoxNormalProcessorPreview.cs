using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    void OnDestroy()
    {
        m_context.Invalidate();
        m_context = null;
        m_renderer.Cleanup();
    }

    void OnGUI()
    {
        UpdateInput();

        if ( m_context != null && m_context.IsValid )
        {
            m_context.PreviewObject.SetActive( true );

            // Draw GUI
            GUILayout.BeginHorizontal( GUILayout.ExpandWidth( false ) );

            // Draw tool bar
            GUIStyle styleToolBox = new GUIStyle();
            styleToolBox.stretchWidth = false;
            GUILayout.BeginVertical( styleToolBox, GUILayout.ExpandWidth( false ) );
            GUILayout.Label( m_title, GUILayout.ExpandWidth( false ) );
            GUILayout.EndVertical();

            // Draw preview
            GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
            DrawPreviewObject( GUILayoutUtility.GetLastRect() );
            GUILayout.EndHorizontal();

            m_context.PreviewObject.SetActive( false );
        }
    }

    void InitScene()
    {
        m_renderer = new PreviewRenderUtility();
        m_renderer.camera.clearFlags = CameraClearFlags.SolidColor;
        m_renderer.camera.backgroundColor = new Color( 0.3f, 0.3f, 0.3f );
        m_renderer.cameraFieldOfView = 60.0f;
        m_renderer.camera.nearClipPlane = 0.3f;
        m_renderer.camera.farClipPlane = 1000.0f;
        m_renderer.camera.transform.position = new Vector3( 0, -10, 0 );
        m_renderer.camera.transform.LookAt( m_context.PreviewObject.transform );
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
    }

    void InitObjectForPreview( GameObject _object )
    {
        m_meshFilter = _object.GetComponent<MeshFilter>();
        m_meshRenderer = _object.GetComponent<MeshRenderer>();
    }

    void DrawPreviewObject( Rect _rect )
    {
        m_renderer.BeginPreview( _rect, GUIStyle.none );

        m_renderer.DrawMesh( m_meshFilter.sharedMesh, Matrix4x4.identity, m_meshRenderer.sharedMaterial, 0 );
        m_renderer.camera.Render();

        GUI.DrawTexture( _rect, m_renderer.EndPreview() );
    }


    void UpdateInput()
    {
        // Handle mouse
        Event currentEvent = Event.current;
        if ( currentEvent.isMouse )
        {
            if ( currentEvent.isScrollWheel )
            {

            }
        }

    }

    void UpdateScene()
    {
        m_renderer.camera.transform.RotateAround( Vector3.zero, Vector3.forward, Time.fixedDeltaTime * 10.0f );

    }
}
