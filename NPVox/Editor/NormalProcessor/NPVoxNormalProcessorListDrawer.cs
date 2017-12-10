using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[CustomPropertyDrawer(typeof(NPVoxNormalProcessorList))]
public class NPVoxNormalProcessorListDrawer : PropertyDrawer
{
    // Constants
    private readonly Color s_colorGUI = new Color( 0.64f, 0.76f, 1.0f );
    private readonly float s_widthHeaderLabel = 150.0f;
    private readonly float s_widthExpandButton = 100.0f;
    private readonly float s_widthUpDownButton = 15.0f;
    private readonly float s_widthTab = 20.0f;
    private readonly float s_verticalSpacePerProcessorItem = 5.0f;
    private readonly float s_verticalSpaceEnd = 32.0f;

    // Members
    private bool m_expanded = false;
    private int m_indexPopupAddProcessor = 0;

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        // Customize gui style
        Color previousContainerColor = GUI.backgroundColor;
        GUI.backgroundColor = s_colorGUI;

        // Header + Expand / Collapse Button
        GUILayout.BeginHorizontal();
        GUILayout.Label( "Normal Processors", GUILayout.Width( s_widthHeaderLabel ) );
        
        if ( !m_expanded )
        {
            if ( GUILayout.Button( "Expand", GUILayout.Width( s_widthExpandButton ) ) )
            {
                m_expanded = true;
            }
        }
        else
        {
            if ( GUILayout.Button( "Collapse", GUILayout.Width( s_widthExpandButton ) ) )
            {
                m_expanded = false;
            }
        }

        GUILayout.EndHorizontal();

        if ( !m_expanded )
        {
            GUILayout.Space( 12.0f );
        }


        // List management
        if ( m_expanded )
        {
            NPVoxMeshOutput target = property.serializedObject.targetObject as NPVoxMeshOutput;
            NPVoxNormalProcessorList processorList = target.normalProcessors;
         
            Dictionary<string, System.Type> processorClasses = new Dictionary< string, System.Type >();
            processorClasses.Add( "<None>", null );
            List<System.Type> allTypes = new List<System.Type>( NPipeReflectionUtil.GetAllTypesWithAttribute( typeof( NPVoxAttributeNormalProcessorListItem ) ) );
            foreach ( System.Type factoryType in allTypes )
            {
                NPVoxAttributeNormalProcessorListItem attr = ( NPVoxAttributeNormalProcessorListItem ) factoryType.GetCustomAttributes( typeof( NPVoxAttributeNormalProcessorListItem ), true )[ 0 ];

                if ( attr.m_classType.BaseType != typeof( NPVoxNormalProcessor ) )
                {
                    continue;
                }

                processorClasses.Add( attr.m_editorName, factoryType );
            }

            string[] processorKeys = processorClasses.Keys.ToArray();

            GUILayout.BeginHorizontal();
            GUILayout.Space( s_widthTab );
            m_indexPopupAddProcessor = EditorGUILayout.Popup( m_indexPopupAddProcessor, processorKeys );
            bool optionAdded = GUILayout.Button( "Add" );
            GUILayout.EndHorizontal();

            if ( optionAdded )
            {
                System.Type processorClass = processorClasses[ processorKeys[ m_indexPopupAddProcessor ] ];
                if ( processorClass != null )
                {
                    processorList.AddProcessor( processorClass );
                }
            }

            foreach ( NPVoxNormalProcessor processor in processorList.GetProcessors() )
            {
                NPVoxAttributeNormalProcessorListItem attr = ( NPVoxAttributeNormalProcessorListItem ) processor.GetType().GetCustomAttributes( typeof( NPVoxAttributeNormalProcessorListItem ), true )[ 0 ];

                GUILayout.Space( s_verticalSpacePerProcessorItem );

                GUILayout.BeginHorizontal();
                GUILayout.Space( s_widthTab );
                GUILayout.Label( attr.m_editorName );

                GUILayout.Space( 20.0f );

                if ( GUILayout.Button( "View Output" ) )
                {

                }
                
                GUILayout.Space( 20.0f );

                if ( GUILayout.Button( "^", GUILayout.Width( s_widthUpDownButton ), GUILayout.ExpandWidth( true ) ) )
                {

                }

                if ( GUILayout.Button( "v", GUILayout.Width( s_widthUpDownButton ), GUILayout.ExpandWidth( true ) ) )
                {

                }

                GUILayout.Space( 20.0f );

                if ( GUILayout.Button( "Remove" ) )
                {
                    

                    processorList.RemoveProcessor( processor );
                    break;
                }

                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            GUILayout.Space( s_verticalSpaceEnd );
        }


        // Restore previous gui style
        GUI.backgroundColor = previousContainerColor;
    }
}

