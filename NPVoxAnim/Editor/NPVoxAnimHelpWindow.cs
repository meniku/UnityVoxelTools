using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NPVoxAnimHelpWindow : EditorWindow 
{
    [MenuItem("NPVox/Show Anim Help")]
    public static void ShowWindow()
    {
        GetWindow().Show();
    }


    private static NPVoxAnimHelpWindow window; 
    public static NPVoxAnimHelpWindow GetWindow()
    {
        if (!window)
        {
            window = (NPVoxAnimHelpWindow)EditorWindow.GetWindow(typeof(NPVoxAnimHelpWindow), true  , "NPVoxAnimHelpWindow Window");
        }
        return window;
    }

    public enum Context
    {
        None,
        Bone,
        Box,
        Misc
    }

    Context context;

    public void SetContext(Context context)
    {
        if (this.context != context)
        {
            this.context = context;
            Repaint();
        }
    }

    public void OnGUI()
    {
        if (context == Context.Bone)
        {
            GUILayout.Label("Bone Selection", EditorStyles.boldLabel );
            PrintHotkey("Toggle Seleciton", "CMD/CTRL + Click");
            PrintHotkey("Select Descendants", "SHIFT + Click");
            PrintHotkey("Hide Selected Bones (press h without selection to undo)", NPVoxAnimationEditorView.HOTKEY_HIDE_SELECTED_BONES.ToString());
        }
        else if (context == Context.Box)
        {
            GUILayout.Label("Box Selection", EditorStyles.boldLabel );
            PrintHotkey("Select whole model", NPVoxAnimationEditorView.HOTKEY_AFFECTED_AREA_SELECT_ALL.ToString());
            PrintHotkey("Voxel Picker", "SHIFT + Drag");
        }

        FlushHotkeys();

        GUILayout.Label("Hotkeys", EditorStyles.boldLabel );

        PrintHotkey("Previous Frame", NPVoxAnimationEditorView.HOTKEY_PREVIOUS_FRAME.ToString());
        PrintHotkey("Next Frame", NPVoxAnimationEditorView.HOTKEY_NEXT_FRAME.ToString());
        PrintHotkey("n'th Frame", "ALT + (1-9)");
        PrintHotkey("Previous Transform", NPVoxAnimationEditorView.HOTKEY_PREVIOUS_TRANSFORMATION.ToString());
        PrintHotkey("Next Transform", NPVoxAnimationEditorView.HOTKEY_NEXT_TRANSFORMATION.ToString());
        PrintHotkey("n'th Transformation", "(1-9)");
        if (context != Context.None)
        {
            PrintHotkey("Move Transformation UP", NPVoxAnimationEditorView.HOTKEY_MOVE_TRANSFORMATION_UP.ToString());
            PrintHotkey("Move Transformation DOWN", NPVoxAnimationEditorView.HOTKEY_MOVE_TRANSFORMATION_DOWN.ToString());
            PrintHotkey("Duplicate Transformation", NPVoxAnimationEditorView.DUPLICATE_TRANSFORMATION.ToString());
        }
        PrintHotkey("Navigate Scene", "q");
        PrintHotkey("Unfocus", "Escape");
        FlushHotkeys();
    }

    private void FlushHotkeys()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        foreach (string label in labels)
            GUILayout.Label(label);
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        foreach (string label in keys)
            GUILayout.Label(label);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        labels.Clear();
        keys.Clear();
    }

    private List<string> labels = new List<string>();
    private List<string> keys = new List<string>();

    private void PrintHotkey(string label, string key)
    {
        labels.Add(label);
        keys.Add(key);
    }

}
