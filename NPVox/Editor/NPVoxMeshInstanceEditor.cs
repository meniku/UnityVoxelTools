using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(NPVoxMeshInstance))]
public class NPVoxMeshInstanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NPVoxMeshInstance instance = (NPVoxMeshInstance)target;
        string path = AssetDatabase.GetAssetPath(instance);
        bool isTemplate = path.Length > 0 && AssetDatabase.AssetPathToGUID(path) == NPVoxConstants.GAMEPOBJECT_TEMPLATE;

        if (isTemplate)
        {
            GUILayout.Label("This is the Prefab used to construct new instances of your models.\nAdjust as your liking, but don't move or rename it!");
            return;
        }

        DrawDefaultInspector();

        if (instance.MeshFactory == null)
        {
            GUILayout.Label("NPVox: No NP Vox Mesh Factory assigned.");

            return;
        }

        if ((instance.MeshFactory is NPVoxProcessorBase<Mesh>) && ((NPVoxProcessorBase<Mesh>)instance.MeshFactory).StorageMode == NPipeStorageMode.ATTACHED)
        {
            if (instance.SharedMash != instance.MeshFactory.GetProduct())
            {
                Undo.RecordObject(instance, "Updated shared mesh");
                instance.SharedMash = instance.MeshFactory.GetProduct();
            }
        }
        else
        {
            GUILayout.Label(
                "NPVox: The Storage Mode is not set to ATTACHED, thus you are not able to preview the item in the editor, sorry");
            GUILayout.Label(" to see any preview during Editor time.\n", new GUILayoutOption[] { });
            if (instance.SharedMash != null)
            {
                Undo.RecordObject(instance, "Unset shared mesh");
            }
            instance.SharedMash = null;
        }

        bool isPrefab = PrefabUtility.GetPrefabParent(target) == null && PrefabUtility.GetPrefabObject(target) != null;

        if (!isPrefab)
        {
            if (GUILayout.Button("Align (Shortcut ALT+a)"))
            {
                Align(instance.transform);
            }
        }
        
        NPVoxCubeSimplifier[] simplifiers = NPipelineUtils.FindNextPipeOfType<NPVoxCubeSimplifier>(NPipelineUtils.GetImportables(AssetDatabase.GetAssetPath(instance.MeshFactory as UnityEngine.Object)), instance.MeshFactory);
        
        if(simplifiers.Length > 0)
        {
            if(GUILayout.Button("Switch to Cube Simplifier instance"))
            {
                NPVoxCubeSimplifierInstance cubeSimplifier = instance.gameObject.AddComponent<NPVoxCubeSimplifierInstance>();
                cubeSimplifier.CubeSimplifier = simplifiers[0];
                cubeSimplifier.UpdateMesh();
                DestroyImmediate(instance, true);
                return;
            }
        } 
        else 
        {
            if(GUILayout.Button("Create Cube Simplifier"))
            {
                string assetPath = AssetDatabase.GetAssetPath(instance.MeshFactory as UnityEngine.Object);
                NPVoxCubeSimplifier simplifier = (NPVoxCubeSimplifier) NPipelineUtils.CreateAttachedPipe( assetPath, typeof(NPVoxCubeSimplifier), instance.MeshFactory );
                simplifier.TextureAtlas = (NPVoxTextureAtlas)UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath("b3ed00785c29642baae5806625c1d3c1"), typeof(NPVoxTextureAtlas));
                simplifier.SourceMaterial = instance.GetComponent<MeshRenderer>().sharedMaterial;
                AssetDatabase.SaveAssets();
            }
        }

        if (GUILayout.Button("Select Pipe Container (Edit Import Settings)"))
        {
            Selection.objects = new Object[] { AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(instance.meshFactory), typeof(NPipeContainer)) };
        }


        if (GUILayout.Button("Invalidate Pipe Container Deep "))
        {
            NPipelineUtils.InvalidateAndReimportAllDeep(AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(instance.meshFactory), typeof(NPipeContainer)));
        }
    }

    public static void Align(Transform transform)
    {
        Undo.RecordObject(transform, "Align Object");
        transform.GetComponentInChildren<NPVoxMeshInstance>().Align(transform);
    }
}
