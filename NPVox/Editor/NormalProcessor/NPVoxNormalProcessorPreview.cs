using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NPVoxNormalProcessorPreview : EditorWindow
{
    private static NPVoxNormalProcessorPreview s_editor = null;

    private Editor m_gameObjectEditor;
    private NPVoxNormalProcessorPreviewContext m_context = NPVoxNormalProcessorPreviewContext.Default;

    private string m_title = "";

    Vector2 m_mousePosition = Vector2.zero;
    Vector2 m_mousePositionPrevious = Vector2.zero;

    public static NPVoxNormalProcessorPreview ShowWindow()
    {
        s_editor = GetWindow<NPVoxNormalProcessorPreview>( "Normal Editor" );
        return s_editor;
    }

    public static bool IsShown()
    {
        return s_editor != null;
    }

    public void SetContext( NPVoxNormalProcessorPreviewContext _context )
    {
        m_context.Invalidate();
        m_gameObjectEditor = null;
        m_context = _context;

        NPVoxNormalProcessor processor = m_context.ViewedProcessor;
        NPVoxAttributeNormalProcessorListItem listItemAttribute = NPipeReflectionUtil.GetAttribute<NPVoxAttributeNormalProcessorListItem>( processor );
        m_title = listItemAttribute.EditorName;
    }

    void OnEnable()
    {
    }

    void OnDestroy()
    {
        m_context.Invalidate();
        m_context = NPVoxNormalProcessorPreviewContext.Default;
        m_gameObjectEditor = null;
        s_editor = null;
    }

    void OnGUI()
    {
        if ( m_context.IsValid )
        {
            m_context.PreviewObject.SetActive( true );
            
            if (m_gameObjectEditor == null)
            {
                m_gameObjectEditor = Editor.CreateEditor( m_context.PreviewObject );
            }

            // Handle mouse
            Event currentEvent = Event.current;
            if ( currentEvent.isMouse )
            {
                if ( currentEvent.isScrollWheel )
                {

                }
            }

            // GUI In / Out
            GUILayout.BeginHorizontal( GUILayout.ExpandWidth( false ) );
            GUILayout.Label( m_title, GUILayout.ExpandWidth( false ) );
            
            m_gameObjectEditor.OnPreviewGUI( GUILayoutUtility.GetRect( 640, 480 ), EditorStyles.whiteLabel );
            GUILayout.EndHorizontal();

            m_context.PreviewObject.SetActive( false );
        }
    }
}
