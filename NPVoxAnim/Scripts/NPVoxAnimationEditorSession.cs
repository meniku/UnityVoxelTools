using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
public class NPVoxAnimationEditorSession : MonoBehaviour
{
    public new NPVoxAnimation animation;
    public Scene previousScene;
    public Scene editorScene;
    public string previousScenePath;
    public string editorScenePath;
    public MeshFilter previewFilter;
    public MeshFilter groundFilter;
    public Dictionary<string, MeshFilter> socketPreviewFilters = new Dictionary<string, MeshFilter>();
    public float groundplateOffset = 0;

    public MeshFilter GetSocketPreviewFilter(string key)
    {
        if (socketPreviewFilters.ContainsKey(key))
        {
            return socketPreviewFilters[key];
        }
        GameObject gameObject = new GameObject();
        gameObject.name = "Preview Socket: " + key;
        socketPreviewFilters[key] = gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        return socketPreviewFilters[key];
    }

    public void WipeSocketPreviewFilters()
    {
        foreach (MeshFilter filter in socketPreviewFilters.Values)
        {
            GameObject.DestroyImmediate(filter.gameObject);
        }
        socketPreviewFilters = new Dictionary<string, MeshFilter>();
    }
}
#endif