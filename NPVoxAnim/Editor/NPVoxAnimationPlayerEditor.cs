using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(NPVoxAnimationPlayer))]
public class NPVoxAnimationPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NPVoxAnimationPlayer animationPlayer = (NPVoxAnimationPlayer)target;

        DrawDefaultInspector();
        
        
        NPVoxAnimation animation = (NPVoxAnimation)EditorGUILayout.ObjectField("Animation", animationPlayer.Animation, typeof(NPVoxAnimation), false);
        if (animation != animationPlayer.Animation)
        {
            animationPlayer.Animation = animation;
            // Repaint();
        }


        if (animation != null)
        {
            if (GUILayout.Button("Select Animation"))
            {
                Selection.objects = new Object[] { animation };
            }
            // if (GUILayout.Button("Edit Animation"))
            // {
            //     OpenAnimationScene(animationPlayer.animation);
            // }

            // if (GUILayout.Button("Set Mesh to animation mesh"))
            // {
            //     animationPlayer.GetComponent<MeshFilter>().sharedMesh = animation.GetOriginalFastestMesh();
            // }
        }
    }

    public static void OpenAnimationScene(NPVoxAnimation animation)
    {
        bool proceed = true;
        UnityEngine.SceneManagement.Scene previousScene = EditorSceneManager.GetActiveScene();
        if (previousScene.isDirty)
        {
            proceed = false;
            if (EditorUtility.DisplayDialog("Unsaved Changes", "You need to save any changes to your active scene before you can edit NPVox Animations", "Save Now", "Abort"))
            {
                proceed = true;
                EditorSceneManager.SaveScene(previousScene);
            }
        }

        if (proceed)
        {
            UnityEngine.SceneManagement.Scene editorScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject gameObject = new GameObject();
            gameObject.name = "Edit " + animation.name;
            NPVoxAnimationEditorSession session = gameObject.AddComponent<NPVoxAnimationEditorSession>();
            session.animation = animation;
            session.previousScene = previousScene;
            session.previousScenePath = previousScene.path;
            EditorSceneManager.CloseScene(previousScene, false);
            EditorSceneManager.SaveScene(editorScene, "_NPVOX_TMP.scene", false);
            session.editorScene = editorScene;
            gameObject.transform.position = Vector3.zero;
            Selection.objects = new Object[] { gameObject };
        }
    }

    public static void CloseAnimationEditor(NPVoxAnimationEditorSession session)
    {
        NPVoxAnimation animation = session.animation;
        string previousScenePath = session.previousScenePath;
        UnityEngine.SceneManagement.Scene editorScene = session.editorScene;

        GameObject.DestroyImmediate(session.gameObject);

        if (previousScenePath != null)
        {
            EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Additive);
        }
        else
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }
        EditorSceneManager.CloseScene(editorScene, true);

        File.Delete("_NPVOX_TMP.scene");

        Selection.objects = new Object[] { animation };
    }
}
