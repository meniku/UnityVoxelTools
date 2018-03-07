using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NPVoxNormalProcessorPreview : EditorWindow
{
    GameObject gameObject;
    Editor gameObjectEditor;

    public static void ShowWindow()
    {
        GetWindow<NPVoxNormalProcessorPreview>("Preview");
    }

    void OnGUI()
    {
        gameObject = (GameObject)EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);

        if (gameObject != null)
        {
            if (gameObjectEditor == null)
            {
                gameObjectEditor = Editor.CreateEditor(gameObject);
            }

            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(500, 500), EditorStyles.whiteLabel);
            //gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(500, 500), EditorStyles.whiteLabel);
        }
    }
}
