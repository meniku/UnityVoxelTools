using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(NPVoxCubeSimplifierInstance))]
public class NPVoxCubeSimplifierInstanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NPVoxCubeSimplifierInstance instance = (NPVoxCubeSimplifierInstance)target;
        string path = AssetDatabase.GetAssetPath(instance);
        bool isTemplate = path.Length > 0 && AssetDatabase.AssetPathToGUID(path) == NPVoxConstants.GAMEPOBJECT_TEMPLATE;

        if (isTemplate)
        {
            GUILayout.Label("This is the Prefab used to construct new instances of your models.\nAdjust as your liking, but don't move or rename it!");
            return;
        }

        instance.UpdateMesh();

        DrawDefaultInspector();

        if (instance.CubeSimplifier == null)
        {
            GUILayout.Label("NPVox: No NP Vox Cube Simplifier assigned.");
            return;
        }

        bool isPrefab = PrefabUtility.GetPrefabParent(target) == null && PrefabUtility.GetPrefabObject(target) != null;

        if (!isPrefab)
        {
            if (GUILayout.Button("Align (Shortcut ALT+a)"))
            {
                Align(instance.transform);
            }
        }

        // NPVoxMeshOutput[] meshOutputs = NPipelineUtils.GetByType<NPVoxMeshOutput>(instance.CubeSimplifier as UnityEngine.Object);

        // if (meshOutputs.Length > 0)
        {
            if (GUILayout.Button("Switch to Mesh Output Instance"))
            {
                NPVoxMeshInstance meshOutputInstance = instance.gameObject.AddComponent<NPVoxMeshInstance>();
                meshOutputInstance.MeshFactory = (NPVoxMeshOutput)instance.CubeSimplifier.InputMeshFactory;
                meshOutputInstance.UpdateMesh();
                instance.gameObject.GetComponent<MeshRenderer>().sharedMaterial = instance.CubeSimplifier.SourceMaterial;
                DestroyImmediate(instance, true);
                return;
            }
        }

        if (GUILayout.Button("Select Pipe Container (Edit Import Settings)"))
        {
            Selection.objects = new Object[] { AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(instance.CubeSimplifier), typeof(NPipeContainer)) };
        }

        if (GUILayout.Button("Invalidate Pipe Container Deep "))
        {
            NPipelineUtils.InvalidateAndReimportAllDeep(AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(instance.CubeSimplifier), typeof(NPipeContainer)));
        }
    }

    public static void Align(Transform transform)
    {
        Undo.RecordObject(transform, "Align Object");
        transform.GetComponentInChildren<NPVoxCubeSimplifierInstance>().Align(transform);
    }

}
