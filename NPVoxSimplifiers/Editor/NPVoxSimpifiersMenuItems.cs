using UnityEngine;
using UnityEditor;

public class NPVoxSimpifiersMenuItems : ScriptableObject
{
    [MenuItem("Assets/Create/NPVox/Texture Atlas", false)]
    static void CreateTextureAtlas()
    {
        var path = NPipelineUtils.GetCreateScriptableObjectAssetPath<NPVoxTextureAtlas>();

        if (path.Length != 0)
        {
            NPVoxTextureAtlas textureAtlas = (NPVoxTextureAtlas)NPVoxTextureAtlas.CreateInstance(typeof(NPVoxTextureAtlas));
            AssetDatabase.CreateAsset(textureAtlas, path);
            AssetDatabase.SaveAssets();
            textureAtlas.InitAssets();
            AssetDatabase.SaveAssets();
            Selection.objects = new Object[] { textureAtlas };
        }
    }
}

