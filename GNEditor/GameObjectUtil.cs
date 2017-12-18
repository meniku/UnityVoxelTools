using UnityEngine;
using System.Collections;

public class GameObjectUtil 
{
    public static T getComponentByTagName<T>(string tagName) 
    {
        GameObject go = GameObject.FindGameObjectWithTag(tagName);
        if(go != null) {
            return go.GetComponent<T>();
        }
        return default(T);
    }

    public static bool IsInLayerMask(GameObject obj, LayerMask mask) 
    {
        return ((mask.value & (1 << obj.layer)) > 0);
    }

    public static Transform GetChildByName(Transform parent, string childName, bool recursive = false)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            if(child.name.Equals(childName)) {
                return child;
            }
            if(recursive) {
                child = GetChildByName(child, childName, true);
                if(child != null) {
                    return child;
                }
            }
        }
        return null;
    }

    public static void DestroyAllChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            #if UNITY_EDITOR
            GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
            #else
            GameObject.Destroy(parent.GetChild(i).gameObject);
            #endif
        }
    }
}
