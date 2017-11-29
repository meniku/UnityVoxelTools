using UnityEngine;
using UnityEditor;

public class NPVoxMenuItems : ScriptableObject
{
    public const float zDist = 3.464f;
    public const float xDist = 4f;
    public const float xDiff = 2f;
    public const float yDist = 1f;

    // [MenuItem ("GameObject/NPVox/Align &a", false)]
    [MenuItem("NPVox/Align &a", false)]
    static void MenuAlign()
    {
        Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);
        foreach (Transform transform in transforms)
        {
            if (transform.GetComponentInChildren<NPVoxMeshInstance>())
            {
                NPVoxMeshInstanceEditor.Align(transform);
            }
        }
    }

    // Validate the menu item defined by the function above.
    // The menu item will be disabled if this function returns false.
    // [MenuItem ("GameObject/NPVox/Align &a", true)]
    [MenuItem("NPVox/Align &a", true)]
    static bool ValidateAlign()
    {
        Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        foreach (Transform transform in transforms)
        {
            if (transform.GetComponentInChildren<NPVoxMeshInstance>())
            {
                return true;
            }
        }

        return false;
    }

    [MenuItem("NPVox/New Mesh Instance &n", false)]
    static void MenuNew()
    {
        UnityEngine.Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            NPipeContainer o = objects[i] as NPipeContainer;
            if (o)
            {
                NPipeIImportable[] outputPipes = NPipelineUtils.FindOutputPipes(NPipelineUtils.GetImportables(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(o))));
                foreach (NPipeIImportable imp in outputPipes)
                {
                    if (imp is NPVoxMeshOutput)
                    {
                        ((NPVoxMeshOutput)imp).Instatiate();
                    }
                }
            }
            NPVoxMeshInstance inst = objects[i] as NPVoxMeshInstance;
            if (inst)
            {
                ((NPVoxMeshOutput)inst.meshFactory).Instatiate();
            }
        }

    }

    [MenuItem("NPVox/Invalidate and Import Selected", false)]
    static void MenuInvalidateAndImportSelected()
    {
        foreach( UnityEngine.Object target in Selection.objects) 
        {
            NPipeIImportable[] allImportables = NPipelineUtils.FindOutputPipes(NPipelineUtils.GetByType<NPipeIImportable>(target));
            NPipelineUtils.InvalidateAll(allImportables);
            EditorUtility.SetDirty(target as UnityEngine.Object);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("NPVox/Invalidate and Import Selected Deep", false)]
    static void MenuInvalidateAndImportSelectedDeep()
    {
        foreach( UnityEngine.Object target in Selection.objects) 
        {
            NPipeIImportable[] allImportables = NPipelineUtils.FindOutputPipes(NPipelineUtils.GetByType<NPipeIImportable>(target));
            NPipelineUtils.InvalidateAll(allImportables, true);
            EditorUtility.SetDirty(target as UnityEngine.Object);
        }
        AssetDatabase.SaveAssets();
    }

    // [MenuItem("NPVox/Select All Instances &s", false)]
    // static void MenuSelect()
    // {
    //     UnityEngine.Object[] objects = Selection.objects;
    //     for (int i = 0; i < objects.Length; i++)
    //     {
    //         NPVoxMetadata o = objects[i] as NPVoxMetadata;
    //         if (o)
    //         {
    //             NPVoxMetadataEditor.SelectAllInstances(o);
    //             break;
    //         }
    //     }
    //     for (int i = 0; i < Selection.gameObjects.Length; i++)
    //     {
    //         NPVoxInstance inst = Selection.gameObjects[i].GetComponent<NPVoxInstance>();
    //         if (inst)
    //         {
    //             NPVoxMetadataEditor.SelectAllInstances(inst.metadata);
    //             break;
    //         }
    //     }
    // }


    [MenuItem("NPVox/New Mesh Instance &n", true)]
    // [MenuItem("NPVox/Select All Instances &s", true)]
    static bool Validate()
    {
        UnityEngine.Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            NPipeContainer o = objects[i] as NPipeContainer;
            if (o)
            {
                NPipeIImportable[] outputPipes = NPipelineUtils.FindOutputPipes(NPipelineUtils.GetImportables(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(o))));
                foreach (NPipeIImportable imp in outputPipes)
                {
                    if (imp is NPVoxMeshOutput)
                    {
                        return true;
                    }
                }
            }
            NPVoxMeshInstance inst = objects[i] as NPVoxMeshInstance;
            if (inst)
            {
                return true;
            }
        }
        return false;
    }

    [MenuItem("Assets/Create/NPVox/Pipe Container", false)]
    static void CreatePipeContainer()
    {
        var path = NPipelineUtils.GetCreateScriptableObjectAssetPath<NPipeContainer>();
        if (path.Length != 0)
        {
            Selection.objects = new Object[] { NPipelineUtils.CreatePipeContainer<NPipeContainer>(path) };
        }
    }
}

