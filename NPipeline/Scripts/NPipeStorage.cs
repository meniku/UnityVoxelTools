using UnityEngine;
using System;
using System.IO;

[System.Serializable]
public enum NPipeStorageMode
{
    MEMORY,
    RESOURCE_CACHE,
    ATTACHED
}

[Serializable]
public class NPipeStorage
{
    [SerializeField]
    private String OwnerUUID = "no_owner";

    [SerializeField]
    private String CacheUUID = Guid.NewGuid().ToString();

    [SerializeField]
    private UnityEngine.Object SelfObject = null;

    [SerializeField]
    private NPipeStorageMode storageMode = NPipeStorageMode.MEMORY;
    public NPipeStorageMode StorageMode
    {
        get {
            return storageMode;
        }
    }

    public UnityEngine.Object SwitchStorageMode(NPipeStorageMode storageMode, UnityEngine.Object instance)
    {
        if (this.storageMode != storageMode)
        {
            // Debug.Log( " Switch storage mode from " +this.storageMode + " to " + storageMode);
            Destroy(instance);
            this.storageMode = storageMode;
            return null;
        }
        return instance;
    }


    public void Store(UnityEngine.Object owner, UnityEngine.Object previousObject, UnityEngine.Object objectToStore, string filetype)
    {
#if UNITY_EDITOR
        OwnerUUID = UnityEditor.AssetDatabase.AssetPathToGUID( UnityEditor.AssetDatabase.GetAssetPath( owner ) );
        if (previousObject != null)
        {
            if (previousObject == objectToStore)
            {
                objectToStore.hideFlags = HideFlags.HideInHierarchy;
                // already stored, don't add this again ...
                // UnityEditor.Undo.RecordObject(objectToStore, "Store object");
                UnityEditor.EditorUtility.SetDirty(objectToStore);
                return;
            }
            Debug.LogWarning("Reused object was not used ... Destroying previous object ...");
            Destroy(previousObject);
            previousObject = null;
        }

        switch (storageMode)
        {
            case NPipeStorageMode.ATTACHED:
                string path = UnityEditor.AssetDatabase.GetAssetPath(owner);
                UnityEditor.AssetDatabase.AddObjectToAsset(objectToStore, path);
                objectToStore.hideFlags = HideFlags.HideInHierarchy;
                SelfObject = objectToStore;
                break;

            case NPipeStorageMode.RESOURCE_CACHE:
                string targetPath = Path.Combine(NPipeConstants.CACHE_DIR, OwnerUUID + "_" + CacheUUID + "." + filetype);
//                Debug.Log( OwnerUUID + " " + CacheUUID );
                if( ! File.Exists(NPipeConstants.CACHE_DIR))
                {
                    Directory.CreateDirectory(NPipeConstants.CACHE_DIR);
                }
                objectToStore.hideFlags = HideFlags.None;
                UnityEditor.AssetDatabase.CreateAsset(objectToStore, targetPath);
                break;

            case NPipeStorageMode.MEMORY:
                objectToStore.hideFlags = HideFlags.None;
                break;

            default:
                break;
        }
#endif
    }

    public UnityEngine.Object Load(Type type, string filetype)
    {
        switch (storageMode)
        {
            case NPipeStorageMode.ATTACHED:
                return SelfObject;

            case NPipeStorageMode.RESOURCE_CACHE:
                #if UNITY_EDITOR 
                string targetPath = Path.Combine(NPipeConstants.CACHE_DIR, OwnerUUID + "_" + CacheUUID + "." + filetype);
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(targetPath, type);
                #else
                string targetPath = Path.Combine(NPipeConstants.CACHE_DIR_FOR_LOAD, OwnerUUID + "_" + CacheUUID);
                UnityEngine.Object obj = Resources.Load(targetPath, type);
                #endif
//                if (obj == null)
//                {
                // that's just wrong there are different cases, for example when switching storage modes after cloning
//                    Debug.Log("Could not load resource from path (normal only on first time you import a RESOURCE_CACHE file) : " + targetPath);
//                }

                return obj;

            case NPipeStorageMode.MEMORY:
            default:
                return null;
        }
    }

    public void Destroy(UnityEngine.Object asset)
    {
        SelfObject = null;
        if (asset == null)
        {
            return;
        }

#if UNITY_EDITOR
        string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
#endif
        UnityEngine.Object.DestroyImmediate(asset, true);
#if UNITY_EDITOR
        if (storageMode == NPipeStorageMode.RESOURCE_CACHE)
        {
            UnityEditor.AssetDatabase.DeleteAsset(path);
        }
#endif
    }

    public void Independiaze()
    {
        // we want to destroy attached assets if they got copied over
        if (SelfObject != null)
        {
            // UnityEngine.Object.DestroyImmediate(SelfObject, true); (should not have be copied with the object)
            SelfObject = null;
        }

        // but for cached resources we only want to generate a new ID and not destroy the product of another storageMode 
        CacheUUID = Guid.NewGuid().ToString();
    }
}