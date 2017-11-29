using UnityEngine;
using UnityEditor;
using System.IO;

public class GNMenuItems : ScriptableObject
{
    [MenuItem("Gaianigma/Flatten")]
    static void Flatten()
    {
        UnityEngine.GameObject gameObject = Selection.activeGameObject;

        // foreach(GameObject go in gameObjects)
        {
            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer mr in meshRenderers)
            {
                if(mr.transform.parent != gameObject.transform)
                {
                    mr.transform.parent = gameObject.transform;
                }
            }
        }
    }
    
    [MenuItem("Gaianigma/Make Vox Prefabs", false)]
    static void MakeVoxPrefabs()
    {
        var path = EditorUtility.SaveFolderPanel(
            "Select target folder for the generated Prefabs ",
            "Prefabs",
            "Select Path for the generated Prefabs");

        path = path.Remove(0, path.LastIndexOf("Assets"));

        if (path.Length != 0)
        {
            Object[] SelectedObjects = Selection.objects;
            int generatedCount = 0;
            foreach (Object o in SelectedObjects)
            {
                NPipeContainer container = o as NPipeContainer;
                if( ! container ) continue;
                NPVoxMeshOutput[] output = NPipelineUtils.GetByType<NPVoxMeshOutput>(container);
                foreach( NPVoxMeshOutput pipe in output)
                {
                    NPVoxMeshInstance instance = pipe.Instatiate().GetComponent<NPVoxMeshInstance>();

                    string prefabPath = Path.Combine(path, instance.name) + ".prefab";

                    PrefabUtility.CreatePrefab(prefabPath, instance.gameObject);
                    GameObject.DestroyImmediate(instance.gameObject);
                    generatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (generatedCount > 0)
            {
                Debug.Log("Generated " + generatedCount + " Prefabs");
            }
            else
            {
                Debug.LogWarning("No NPVoxMetadata selected");
            }
        }
    }
}

