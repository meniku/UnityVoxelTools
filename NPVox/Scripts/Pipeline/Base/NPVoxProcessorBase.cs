using UnityEngine;

abstract public class NPVoxProcessorBase<PRODUCT> : ScriptableObject, NPipeIImportable, NPipeIEditable where PRODUCT : UnityEngine.Object
{
    [SerializeField, HideInInspector]
    private NPipeStorageMode storageMode = NPipeStorageMode.MEMORY;

    [SerializeField, HideInInspector]
    private NPipeStorage storage = new NPipeStorage();

    [SerializeField, HideInInspector]
    private bool isValid = false;

    [SerializeField, HideInInspector]
    public bool isTemplate = false;

    private PRODUCT instance = null;

    public string InstanceName = null; // TODO: doesn't every unity object already have a name?

#if UNITY_EDITOR
    private double lastInvalidatedAt = 0;
#endif

    public NPipeStorageMode StorageMode
    {
        get
        {
            return storageMode;
        }
        set
        {
            storageMode = value;
        }
    }

    virtual public void Import()
    {
        if (storageMode != storage.StorageMode || storageMode != NPipeStorageMode.MEMORY)
        {
            GetProduct();
        }
    }

    public bool IsTemplate()
    {
        return isTemplate;
    }

    public void Invalidate(bool includeInputs = false)
    {
        if (includeInputs)
        {
            NPipeIImportable[] sources = GetAllInputs();
            if (sources != null)
            {
                foreach (NPipeIImportable importable in sources)
                {
                    importable.Invalidate(true);
                }
            }
        }
        isValid = false;

        #if UNITY_EDITOR
        lastInvalidatedAt = UnityEditor.EditorApplication.timeSinceStartup;
        #endif
    }

    virtual public void Destroy()
    {
        storage.Destroy(GetProduct() as UnityEngine.Object);
        isValid = false;
        instance = null;
    }

    public void OnDestroy()
    {
        Destroy();
    }

    public PRODUCT GetProduct()
    {
        if (instance != null && IsValid())
        {
            return instance;
        }

        if (instance == null)
        {
            instance = storage.Load(typeof(PRODUCT), GetFileType()) as PRODUCT;
        }

        if (instance != null && IsValid())
        {
            // Debug.Log("Loaded already created Prodcut for : " + this + " (" + this.StorageMode+ ") " );
        }
        else
        {
            PRODUCT newInstance = storage.SwitchStorageMode(storageMode, instance as UnityEngine.Object) as PRODUCT;
            PRODUCT previousInstance = newInstance;
            newInstance = CreateProduct(newInstance);

            if (newInstance != null)
            {
                storage.Store(this, previousInstance as UnityEngine.Object, newInstance as UnityEngine.Object, GetFileType());
                // Debug.Log("Created a Product for : " + this + " (" + this.StorageMode+ ") " );
                // UnityEditor.EditorUtility.SetDirty(this);
            }
            else
            {
                // Debug.LogWarning("Cold not create Product for : " + this);
            }

            instance = newInstance;
            bool wasNotValid = !isValid;
            isValid = true;

#if UNITY_EDITOR
            if (wasNotValid && this)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        return instance;
    }

    abstract protected PRODUCT CreateProduct(PRODUCT reuse = null);

    protected string GetFileType()
    {
        return "asset";
    }

    public abstract string GetTypeName();

    public string GetInstanceName()
    {
        return this.InstanceName;
    }

    public virtual NPipeIImportable[] GetAllInputs()
    {
        return null;
    }

    public bool IsValid()
    {
        // NPVoxModelFactory[] sources = GetAllSources();
        // // TODO: what's the performance impact for this?
        // if(sources != null)
        // {
        //     foreach(NPVoxImportable importable in sources)
        //     {
        //         if(!importable.IsValid())
        //         {
        //             return false;
        //         }
        //     }
        // }
        return isValid;
    }

    virtual public UnityEngine.Object Clone()
    {
        NPVoxProcessorBase<PRODUCT> copy = Object.Instantiate(this);
        copy.storage.Independiaze();
        copy.isValid = false;
        copy.instance = null;
        copy.isTemplate = false;
        return copy;
    }

    virtual public void IncludeSubAssets(string path) {}

#if UNITY_EDITOR
    virtual public bool DrawInspector(NPipeEditFlags flags)
    {
        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(this);
        bool changed = editor.DrawDefaultInspector();

        if ((flags & NPipeEditFlags.STORAGE_MODE) == NPipeEditFlags.STORAGE_MODE)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Storage Mode");
            NPipeStorageMode newStorageMode = (NPipeStorageMode) GUILayout.Toolbar((int)storageMode, new string[]{ "RAM", "RESOURCE_CACHE", "ATTACHED" });

            if (newStorageMode != storageMode)
            {
                this.storageMode = newStorageMode;
                changed = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }

        return changed;
    }

    virtual public bool DrawMultiInstanceEditor(NPipeEditFlags flags, UnityEngine.Object[] objects)
    {
        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(objects);
        bool changed = editor.DrawDefaultInspector();

        if ((flags & NPipeEditFlags.STORAGE_MODE) == NPipeEditFlags.STORAGE_MODE)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Storage Mode");
            NPipeStorageMode newStorageMode = (NPipeStorageMode) GUILayout.Toolbar((int)storageMode, new string[]{ "RAM", "RESOURCE_CACHE", "ATTACHED" });

            foreach(UnityEngine.Object o in objects)
            {
                if (((NPVoxProcessorBase<PRODUCT>)o).storageMode != storageMode)
                {
                    ((NPVoxProcessorBase<PRODUCT>)o).storageMode = newStorageMode;
                    changed = true;
                }
            }
            if (newStorageMode != storageMode)
            {
                this.storageMode = newStorageMode;
                changed = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }

        return changed;
    }

    public double GetLastInvalidatedTime()
    {
        return lastInvalidatedAt;
    }
#endif
}