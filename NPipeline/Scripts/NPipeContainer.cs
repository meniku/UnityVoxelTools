using UnityEngine;

public class NPipeContainer : ScriptableObject
{
    #if UNITY_EDITOR
    virtual public UnityEngine.Object[] GetAllSelectableFactories()
    {
        return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));
    }

    virtual public void OnAssetCreated()
    {
    }

    virtual public void OnImport()
    {
    }

    #endif

    virtual public string GetDisplayName(NPipeIImportable importable)
    {
        if(importable.GetInstanceName() != null && importable.GetInstanceName().Length > 0)
        {
            return importable.GetInstanceName() + " (" + importable.GetTypeName() + ")";
        }
        else
        {
            return importable.GetTypeName();
        }
    }
}