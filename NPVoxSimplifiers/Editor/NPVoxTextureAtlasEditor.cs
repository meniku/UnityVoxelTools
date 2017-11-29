using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(NPVoxTextureAtlas))]
public class NPVoxTextureAtlasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NPVoxTextureAtlas instance = (NPVoxTextureAtlas)target;

        // DrawDefaultInspector();

        EditorGUILayout.LabelField("NPVox Texture Atlas");
        GUILayout.Label("Allocated: " + instance.GetNumAllocatedFields() + " / " + instance.GetNumTotalFields());
        // GUILayout.Label("Token: " + instance.);

        if (GUILayout.Button("Clear"))
        {
            instance.Clear();
        }
    }
}
