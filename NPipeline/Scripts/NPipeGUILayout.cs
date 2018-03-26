using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPipeGUILayout
{
    #if UNITY_EDITOR
    public static T Popup<T>(string[] labels, T[] values, T selectedValue, bool autoSelectFirst = false)
    {
        int selectedIndex = -1;
        for (int i = 0; i < labels.Length; i++)
        {
            if (values[i].Equals(selectedValue))
            {
                selectedIndex = i;
            }
        }

        int newIndex = EditorGUILayout.Popup(selectedIndex, labels);
        if (newIndex != selectedIndex)
        {
            return values[newIndex];
        }

        if (newIndex == -1 && autoSelectFirst && values.Length > 0)
        {
            return values[0];
        }

        return selectedValue;
    }

    public static T Toolbar<T>(string[] labels, T[] values, T selectedValue)
    {
        int selectedIndex = -1;
        for (int i = 0; i < labels.Length; i++)
        {
            if (values[i].Equals(selectedValue))
            {
                selectedIndex = i;
            }
        }

        int newIndex = GUILayout.Toolbar(selectedIndex, labels);
        if (newIndex != selectedIndex)
        {
            return values[newIndex];
        }

        if (newIndex == -1 && values.Length > 0)
        {
            return values[0];
        }

        return selectedValue;
    }

    public static int Toolbar( string _label, int _selected, string[] _contents, params GUILayoutOption[] _options )
    {
        GUIStyle noStretch = new GUIStyle();
        noStretch.stretchWidth = false;
        noStretch.stretchHeight = false;

        GUILayout.BeginHorizontal( noStretch, GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( false ) );
        GUILayout.Label( _label, GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( false ) );
        _selected = GUILayout.Toolbar( _selected, _contents, _options );
        GUILayout.EndHorizontal();
        return _selected;
    }

    public static float HorizontalSlider( string _label, float _labelWidth, float _value, float _min, float _max, params GUILayoutOption[] _options )
    {
        GUIStyle noStretch = new GUIStyle();
        noStretch.stretchWidth = false;
        noStretch.stretchHeight = false;

        GUILayout.BeginHorizontal( noStretch, GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( false ) );
        GUILayout.Label( _label, GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( false ), GUILayout.Width( _labelWidth ) );
        float fNewValue = GUILayout.HorizontalSlider( _value, _min, _max, _options );
        GUILayout.EndHorizontal();
        return fNewValue;
    }

    public class TableColumn
    {
        public string m_content;
        public int m_width;

        public TableColumn( string _content, int _width )
        {
            m_content = _content;
            m_width = _width;
        }
    }

    public static void TableRow( string _label, int _labelWidth, int _columnSpace, params TableColumn[] _columns )
    {
        GUIStyle noStretch = new GUIStyle();
        noStretch.stretchWidth = false;

        GUILayout.BeginHorizontal( noStretch, GUILayout.ExpandWidth( false ) );

        GUILayout.Label( _label, GUILayout.ExpandWidth( false ), GUILayout.Width( _labelWidth ) );

        foreach ( TableColumn column in _columns )
        {
            GUILayout.Label( column.m_content, GUILayout.ExpandWidth( false ), GUILayout.Width( column.m_width ) );
        }

        GUILayout.EndHorizontal();

        GUILayout.Space( _columnSpace );
    }

#endif
}

