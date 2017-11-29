using UnityEngine;
using UnityEditor;

public class NPVoxAnimMenuItems : ScriptableObject
{
    [MenuItem("Assets/Create/NPVox/Animation", false)]
    static void CreateAnimation()
    {
        var path = NPipelineUtils.GetCreateScriptableObjectAssetPath<NPVoxAnimation>();
        if (path.Length != 0)
        {
            Selection.objects = new Object[] { NPipelineUtils.CreatePipeContainer<NPVoxAnimation>(path) };
        }
    }
}

