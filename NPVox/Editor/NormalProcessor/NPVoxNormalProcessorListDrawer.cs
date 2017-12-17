﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[CustomPropertyDrawer(typeof(NPVoxNormalProcessorList))]
public class NPVoxNormalProcessorListDrawer : PropertyDrawer
{
    // Constants
    private readonly Color s_colorBackgroundGUI = new Color( 0.64f, 0.76f, 1.0f );
    private readonly Color s_colorForegroundGUI = new Color( 0.0f, 0.0f, 0.2f );
    private readonly float s_widthHeaderLabel = 150.0f;
    private readonly float s_widthExpandButton = 100.0f;
    private readonly float s_widthUpDownButton = 15.0f;
    private readonly float s_widthTab = 20.0f;
    private readonly float s_widthMinItemName = 200.0f;
    private readonly float s_verticalSpacePerItem = 5.0f;
    private readonly float s_verticalSpaceEnd = 32.0f;

    // Members
    private bool m_expanded = false;
    private int m_indexPopupAddProcessor = 0;

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        EditorGUI.BeginProperty( position, label, property );
        // Customize gui style
        Color previousBGColor = GUI.backgroundColor;
        Color previousFGColor = GUI.contentColor;
        GUI.backgroundColor = s_colorBackgroundGUI;
        // GUI.contentColor = s_colorForegroundGUI; // Doesn't seem to work

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
            allTypes = allTypes.OrderBy( x => ( ( NPVoxAttributeNormalProcessorListItem ) x.GetCustomAttributes( typeof( NPVoxAttributeNormalProcessorListItem ), true )[ 0 ] ).ListPriority ).ToList();
            foreach ( System.Type factoryType in allTypes )
            {
                NPVoxAttributeNormalProcessorListItem attr = ( NPVoxAttributeNormalProcessorListItem ) factoryType.GetCustomAttributes( typeof( NPVoxAttributeNormalProcessorListItem ), true )[ 0 ];

                if ( attr.ClassType.BaseType != typeof( NPVoxNormalProcessor ) )
                {
                    continue;
                }

                processorClasses.Add( attr.EditorName, factoryType );
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

                GUILayout.Space( s_verticalSpacePerItem );

                GUILayout.BeginHorizontal();
                GUILayout.Space( s_widthTab );
                GUILayout.Label( attr.EditorName, GUILayout.MinWidth( s_widthMinItemName ) );

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
                    processorList.DestroyProcessor( processor );
                    break;
                }

                GUILayout.EndHorizontal();
                
                processor.OnGUI();
                GUILayout.Space( 10.0f );
            }

            GUILayout.Space( s_verticalSpaceEnd );
        }


        // Restore previous gui style
        GUI.backgroundColor = previousBGColor;
        GUI.contentColor = previousFGColor;

        EditorGUI.EndProperty();
    }
}

