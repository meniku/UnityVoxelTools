using UnityEngine;

abstract public class NPVoxForwarderBase<SOURCE_FACTORY, PRODUCT> : ScriptableObject, NPipeIImportable, NPipeIComposite, NPipeIEditable where PRODUCT : Object where SOURCE_FACTORY : class, NPipeIImportable
{
    [SerializeField]
    private string InstanceName = "";

    [SerializeField, HideInInspector]
    private UnityEngine.Object input;

    #if UNITY_EDITOR
    private double lastInvalidatedAt = 0;
    #endif

    public NPipeIImportable[] GetAllInputs()
    {
        if (Input != null)
        {
            return new NPipeIImportable[] { (NPipeIImportable)Input };
        }
        else
        {
            return new NPipeIImportable[] { };
        }
    }

    public NPipeIImportable Input
    {
        get
        {
            return input as NPipeIImportable;
        }
        set
        {
            input = (UnityEngine.Object)value;
        }
    }

    virtual public void Import()
    {
        GetProduct();
    }

    public bool IsTemplate()
    {
        return false;
    }

    public void Invalidate(bool includeInputs = false)
    {
        if (includeInputs)
        {
            if (Input != null)
            {
                Input.Invalidate(true);
            }
        }

        #if UNITY_EDITOR
        lastInvalidatedAt = UnityEditor.EditorApplication.timeSinceStartup;
        #endif
    }

    public virtual void Destroy()
    {
    }

    public void OnDestroy()
    {
        Destroy();
    }

    protected string GetFileType()
    {
        return "asset";
    }

    abstract public PRODUCT GetProduct();

    public abstract string GetTypeName();

    public string GetInstanceName()
    {
        return this.InstanceName;
    }

    public bool IsValid()
    {
        return false;
    }

    virtual public UnityEngine.Object Clone()
    {
        NPVoxForwarderBase<SOURCE_FACTORY, PRODUCT> copy = Object.Instantiate(this);
        return copy;
    }

#if UNITY_EDITOR
    virtual public bool DrawInspector(NPipeEditFlags flags)
    {
        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(this);
        bool changed = editor.DrawDefaultInspector();
        if ((flags & NPipeEditFlags.INPUT) == NPipeEditFlags.INPUT)
        {
            SOURCE_FACTORY newSource = NPipelineUtils.DrawSourceSelector<SOURCE_FACTORY>("Input", input as SOURCE_FACTORY);
            if (newSource as  NPVoxForwarderBase<SOURCE_FACTORY, PRODUCT> == this)
            {
                return false;
            }
            changed = newSource != Input || changed;
            Input = (NPipeIImportable)newSource;
        }

        // if((flags & NPVoxEditFlags.TOOLS) == NPVoxEditFlags.TOOLS)
        // {
        //     if(GUILayout.Button("Invalidate & Reimport Mesh Output Deep"))
        //     {
        //         NPVoxPipelineUtils.InvalidateAndReimportDeep( this );
        //     }
        // }

        return changed;
    }

    public double GetLastInvalidatedTime()
    {
        return lastInvalidatedAt;
    }
#endif
}