using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class NPVoxHotkey
{
    public static NPVoxHotkey NONE = new NPVoxHotkey(KeyCode.None, false, false, false);

    public KeyCode keyCode;
    public bool cmd;
    public bool shift;
    public bool alt;

    public NPVoxHotkey(KeyCode keyCode, bool cmd, bool shift, bool alt)
    {
        this.keyCode = keyCode;
        this.cmd = cmd;
        this.shift = shift;
        this.alt = alt;
    }

    public bool IsDown(Event e)
    {
        if (keyCode == KeyCode.None)
        {
            return false;
        }
        bool modifiersOk = (cmd == (e.command || e.control)) && (e.shift  == shift) && (e.alt == alt);
        return modifiersOk && e.isKey && e.type == EventType.KeyDown && e.keyCode == keyCode;
    }

    override public string ToString()
    {
        string str = "";
        if (cmd)
            str += "CTRL|CMD + ";
        if (shift)
            str += "SHIFT + ";
        if (alt)
            str += "ALT + ";

        if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
        {
            str += keyCode - KeyCode.Alpha0;
        }
        else
        {
            str += keyCode;
        }
        return str;
    }
}

public class NPVoxGUILayout
{
    
    public static T HotkeyToggleBar<T>(string[] labels, T[] values, KeyCode key, T selectedValue, bool noFocusCheck = false)
    {
        int selectedIndex = -1;
        bool focusOK = EditorGUIUtility.keyboardControl == 0 || noFocusCheck;
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
        
        if(key == KeyCode.None)
        {
            return selectedValue;
        }
        
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown && e.keyCode == key && focusOK)
        {
            e.Use();
            newIndex = (newIndex + 1) % values.Length;
            return values[newIndex];
        }

        return selectedValue;
    }


    public static T HotkeyToolbar<T>(string[] labels, T[] values, KeyCode[] keys, T selectedValue, bool showKey = true, bool cmdRequired = false, bool noFocusCheck = false)
    {
        int selectedIndex = -1;
        bool focusOK = EditorGUIUtility.keyboardControl == 0 || noFocusCheck;
        for (int i = 0; i < labels.Length; i++)
        {
            if (values[i].Equals(selectedValue))
            {
                selectedIndex = i;
            }
        }

        if (showKey)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (selectedValue.Equals(values[i]))
                {
                    selectedIndex = i;
                }
                labels[i] += " (" + keys[i] + ")";
            }
        }

        int newIndex = GUILayout.Toolbar(selectedIndex, labels);
        if (newIndex != selectedIndex)
        {
            return values[newIndex];
        }

        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (e.keyCode == keys[i] && keys[i] != KeyCode.None && focusOK)
                {
                    e.Use();
                    EditorWindow.focusedWindow.Repaint();
                    return values[i];
                }
            }
        }

        return selectedValue;
    }

    public static bool HotkeyButton(GUIContent guiContent, NPVoxHotkey hotkey = null, bool showKey = true, bool bold = false, bool hotcontrolCheck = false, GUIStyle style = null)
    {
        Event e = Event.current;
        bool focusOK = EditorGUIUtility.keyboardControl == 0 && (!hotcontrolCheck || GUIUtility.hotControl == 0);
        guiContent.text = guiContent.text + (showKey && hotkey != null ? " (" + hotkey + ")" : "");
        style = style == null ? GUI.skin.button : style;
        if (bold) style.fontStyle = FontStyle.Bold;
        if (GUILayout.Button(guiContent, style) || (focusOK && hotkey != null && hotkey.IsDown(e)))
        {
            e.Use();
            EditorWindow.focusedWindow.Repaint();
            return true;
        }
        return false;
    }

    public static bool HotkeyButton(string label, NPVoxHotkey hotkey, bool showKey = true, bool bold = false, bool hotcontrolCheck = false) //, bool showKey = true, bool cmdRequired = false, bool bold = false, bool shiftRequired = false, bool altRequired = false)
    {
        return HotkeyButton(new GUIContent(label), hotkey, showKey, bold, hotcontrolCheck);
    }

    public static bool MultiHotkeyButton(string label, NPVoxHotkey[] hotkeys, bool showKey = true, bool bold = false)
    {
        Event e = Event.current;
        bool focusOK = EditorGUIUtility.keyboardControl == 0;
        GUIStyle style = new GUIStyle(GUI.skin.button);
        if (bold) style.fontStyle = FontStyle.Bold;

        bool isHotkeyPressed = false;
        string keyCodeString = "";
        foreach (NPVoxHotkey keyCode in hotkeys)
        {
            if (keyCode.keyCode != KeyCode.None)
            {
                if (keyCode.IsDown(e))
                {
                    isHotkeyPressed = true;
                }
                if (keyCodeString == "")
                {
                    keyCodeString = " (" + keyCode;
                }
                else
                {
                    keyCodeString += "|" + keyCode;
                }
            }
        }

        if (keyCodeString != "")
        {
            keyCodeString += ")";
        }

        if (GUILayout.Button(label + (showKey ? keyCodeString : ""), style) || (focusOK && isHotkeyPressed))
        {
            e.Use();
            EditorWindow.focusedWindow.Repaint();
            return true;
        }
        return false;
    }

    public static string DrawSocketSelector(string label, string previousValue, NPVoxIModelFactory modelFactory)
    {
        if (modelFactory == null)
        {
            return previousValue;
        }

        NPVoxModel inputModel = modelFactory.GetProduct();
        string[] socketNames = inputModel.SocketNames;

        if (inputModel)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            if (socketNames.Length > 0)
            {
                int previousSelected = NPipeArrayUtil.GetElementIndex(socketNames, previousValue);
                int newSelected = GUILayout.SelectionGrid(previousSelected, socketNames, 4);
                GUILayout.EndHorizontal();
                if (newSelected < 0)
                {
                    newSelected = 0;
                }
                return socketNames[newSelected];
            }
            else
            {
                GUILayout.Label("-no sockets-");
                GUILayout.EndHorizontal();
            }
        }

        return previousValue;
    }

}
#endif
