using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[System.Serializable]
public class NPVoxFrame
{
    [SerializeField]
    private float duration = 1.0f;
    public float Duration
    {
        get {
            return duration > 0.01f ? duration : 1.0f;
        }
        set
        {
            duration = value > 0.1f ? value : 1.0f;
        }
    }


    [SerializeField]
    private NPVoxAnimation animation;

    // ===================================================================================================
    // Preview Sockets
    // ===================================================================================================

    [SerializeField]
    private NPVoxSocketAttachment[] previewAttachments = new NPVoxSocketAttachment[]{};
   
    #if UNITY_EDITOR
    public NPVoxSocketAttachment GetPreviewAttachmentForTargetSocket(string targetSocketName, bool createIfNotExists = false)
    {
        if( previewAttachments == null ) previewAttachments = new NPVoxSocketAttachment[]{};
        foreach (NPVoxSocketAttachment previewSocket in previewAttachments)
        {
            if (previewSocket.targetSocketName == targetSocketName)
            {
                return previewSocket;
            }
        }

        if (createIfNotExists)
        {
            NPVoxSocketAttachment p = new NPVoxSocketAttachment();
            p.targetSocketName = targetSocketName;
            UnityEditor.ArrayUtility.Add(ref previewAttachments, p);
            return p;
        }
        return null;
    }

    public NPVoxSocketAttachment GetPreviewAttachmentForFactory(NPVoxIModelFactory factory)
    {
        if( previewAttachments == null ) previewAttachments = new NPVoxSocketAttachment[]{};
        foreach (NPVoxSocketAttachment previewSocket in previewAttachments)
        {
            if (previewSocket.meshFactory != null && previewSocket.meshFactory.Input == factory)
            {
                return previewSocket;
            }
        }
            
        return null;
    }
    #endif
    // ===================================================================================================
    // Frame Functionalty
    // ===================================================================================================

    // NEW API
    [SerializeField]
    public UnityEngine.Object source; // todo make private once updates done
    public NPVoxIModelFactory Source
    {
        get
        {
            return source as NPVoxIModelFactory;
        }
        set
        {
            source = value as UnityEngine.Object;
            if (transformers.Length > 0)
            {
                Transformers[0].Input = value;
            }
            else
            {
                preOutput.Input = value;
            }
            InvalidateFromStep(0);
        }
    }

    public void FixStuff()
    {
        #if UNITY_EDITOR
        if( !preOutput) 
        {
            Debug.LogWarning("Added Preoutput");
            preOutput = (NPVoxModelForwarder) NPVoxModelForwarder.CreateInstance<NPVoxModelForwarder>();

            NPipelineUtils.CreateAttachedPipe(AssetDatabase.GetAssetPath(animation), preOutput);
//            AssetDatabase.AddObjectToAsset(preOutput, AssetDatabase.GetAssetPath(animation));
            preOutput.Invalidate();
            EditorUtility.SetDirty(animation);
        }

        if (output.Input == null || output.Input != preOutput as NPipeIImportable)
        {
            Debug.LogWarning("Fixed output");
            output.Input = preOutput;
            output.Invalidate();
            EditorUtility.SetDirty(animation);
        }

        int invalidateFrom = -1;
        for(int i = 0; i < this.transformers.Length; i++)
        {
            if(i == 0)
            {
                if( ((NPipeIComposite) this.transformers[i] ).Input != Source)
                {
                    ((NPipeIComposite) this.transformers[i] ).Input = Source;
                    if(invalidateFrom == -1 || invalidateFrom > i) invalidateFrom = i;
                    Debug.LogWarning("Fixed source of transformer 0");
                }
            }
            else
            {
                if( ((NPipeIComposite) this.transformers[i] ).Input != ((NPipeIComposite) this.transformers[i - 1] ))
                {
                    ((NPipeIComposite) this.transformers[i] ).Input =  ((NPipeIComposite) this.transformers[i - 1] );
                    if(invalidateFrom == -1 || invalidateFrom > i) invalidateFrom = i;
                    Debug.LogWarning("Fixed source of transformer " + i);
                }
            }
        }

        if(transformers.Length > 0 )
        {
            if( preOutput.Input != ((NPipeIComposite) this.transformers[transformers.Length - 1]) )
            {
                if(invalidateFrom == -1 ) invalidateFrom = transformers.Length;
                preOutput.Input = ((NPipeIComposite) this.transformers[transformers.Length - 1] );
                Debug.LogWarning("Fixed source of preOutput");
            }
        }
        else if( preOutput.Input != Source )
        {
            preOutput.Input = Source;
            Debug.LogWarning("Fixed source of preOutput");
        }

        if(invalidateFrom > -1)
        {
            this.InvalidateFromStep(invalidateFrom);
        }
        #endif
    }

    [SerializeField]
    private UnityEngine.Object[] transformers = new UnityEngine.Object[0];

    [SerializeField]
    private NPVoxMeshOutput output;
    public NPVoxMeshOutput Output
    {
        get
        {
            return output;
        }
    }

    [SerializeField]
    private NPVoxModelForwarder preOutput;
    public NPVoxModelForwarder PreOutput
    {
        get
        {
            return preOutput;
        }
    }

    public NPVoxFrame(NPVoxAnimation animation)
    {
        this.animation = animation;

        output = (NPVoxMeshOutput)animation.MeshFactory.Clone();
        output.hideFlags = HideFlags.HideInHierarchy;

//        Debug.Log("invalidating");
#if UNITY_EDITOR
        AssetDatabase.AddObjectToAsset(output, AssetDatabase.GetAssetPath(animation));
        preOutput = (NPVoxModelForwarder) NPVoxModelForwarder.CreateInstance<NPVoxModelForwarder>();
        NPipelineUtils.CreateAttachedPipe(AssetDatabase.GetAssetPath(animation), preOutput);
        preOutput.Input = null;
#endif
        output.Input = preOutput;
        output.Invalidate();
        preOutput.Invalidate();
    }

#if UNITY_EDITOR

    public void PrependTransformer(NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> factory)
    {
        Undo.RecordObject(animation, "created a new transformer");

        factory.hideFlags = HideFlags.HideInHierarchy;

        AssetDatabase.AddObjectToAsset(factory, AssetDatabase.GetAssetPath(animation));
        ArrayUtility.Insert(ref this.transformers, 0, factory);

        Undo.RecordObject(factory, "created a new transformer");
        Undo.RecordObject(Transformers[1], "created a new transformer");
        factory.Input = Source;
        if (this.transformers.Length > 1)
        {
            Transformers[1].Input = factory;
        }

        Undo.RegisterCreatedObjectUndo(factory, "created a new transformer");

        Undo.RecordObject(output, "created a new transformer");
        preOutput.Input = Transformers[transformers.Length - 1];
        InvalidateFromStep(0);
    }

    public void AppendTransformer(NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> factory)
    {
        AddTransformerAt(factory, -1);
    }

    public int AddTransformerAt(NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> factory, int index)
    {
        Undo.RecordObject(animation, "created a new transformer");

        if (index == -1 || index >= transformers.Length)
        {
            index = transformers.Length;
        }
        else
        {
            index = index + 1;
        }

        factory.hideFlags = HideFlags.HideInHierarchy;

        AssetDatabase.AddObjectToAsset(factory, AssetDatabase.GetAssetPath(animation));
        ArrayUtility.Insert(ref transformers, index , factory);

//        Undo.RecordObject(factory, "created a new transformer");

        if (index == 0)
        {
            factory.Input = Source;
        }
        else
        {
            factory.Input = Transformers[index - 1];
        }

        if (index < Transformers.Length - 1)
        {
            Undo.RecordObject(Transformers[index + 1], "Moving a transformer");
            Transformers[index + 1].Input = factory;
        }

        Undo.RegisterCreatedObjectUndo(factory, "created a new transformer");

        Undo.RecordObject(preOutput, "created a new transformer");
        preOutput.Input = Transformers[transformers.Length - 1];
        InvalidateFromStep(transformers.Length);
        return index;
    }

    public void MoveTransformer(int index, int deltaIndex)
    {
        Undo.RecordObject(animation, "Moving a transformer");

        NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> target = GetTransformer(index);
        NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> swapWith = GetTransformer(index + deltaIndex);
        if (swapWith == null)
        {
            return;
        }
        if (Mathf.Abs(deltaIndex) > 1)
        {
            Debug.LogError("deltaIndex > 1 not supported yet");
            return;
        }

        if (index + deltaIndex < 0)
        {
            Debug.LogWarning("Cannot move beofre index 0");
            return;
        }
        if (index + deltaIndex >= transformers.Length)
        {
            Debug.LogWarning("Cannot move after end");
            return;
        }

        ArrayUtility.RemoveAt(ref transformers, index + deltaIndex);
        ArrayUtility.Insert(ref transformers, index, swapWith);

        Undo.RecordObject(swapWith, "Moving a transformer");
        Undo.RecordObject(target, "Moving a transformer");
        if (index + deltaIndex < index)
        {
            target.Input = swapWith.Input;
            swapWith.Input = target;
            if (Transformers.Length > index + 1)
            {
                Undo.RecordObject(Transformers[index + 1], "Moving a transformer");
                Transformers[index + 1].Input = swapWith;
            }
        }
        else
        {
            swapWith.Input = target.Input;
            target.Input = swapWith;
            if (Transformers.Length > index + deltaIndex + 1)
            {
                Undo.RecordObject(Transformers[index + deltaIndex + 1], "Moving a transformer");
                Transformers[index + deltaIndex + 1].Input = target;
            }
        }

        Undo.RecordObject(preOutput, "Moving a transformer");
        preOutput.Input = (NPipeIImportable) transformers[transformers.Length - 1];
        InvalidateFromStep(Mathf.Min(index + deltaIndex, index));
    }

    public void DestroyTransformer(int index)
    {
        Undo.RecordObject(animation, "Destroying a transformer");
        NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer = GetTransformer(index);
        if (!transformer)
        {
            Debug.LogError("Transformer not found");
            return;
        }
        ArrayUtility.RemoveAt(ref transformers, index);

        Undo.RecordObject(output, "Destroying a transformer");
        if (transformers.Length > 0)
        {
            preOutput.Input = Transformers[transformers.Length - 1];
        }
        else
        {
            preOutput.Input = Source;
        }
//        preOutput.Invalidate();

        if (index < Transformers.Length)
        {
            Undo.RecordObject(Transformers[index], "Destroying a transformer");
            if (index > 0)
            {
                Transformers[index].Input = Transformers[index - 1];
            }
            else
            {
                Transformers[index].Input = Source;
            }
        }

        transformer.Destroy();
        Undo.DestroyObjectImmediate(transformer);

        InvalidateFromStep(index + 1);
    }
#endif

    public NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>[] Transformers
    {
        get
        {
            NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>[] transformers = new
                NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>[this.transformers.Length];

            int i = 0;
            foreach (UnityEngine.Object obj in this.transformers)
            {
                transformers[i++] = obj as NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>;
            }

            return transformers;
        }
        set
        {
            transformers = value;
        }
    }

    public NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> GetTransformer(int index)
    {
        if (index < 0)
        {
            return Transformers[transformers.Length + index];
        }
        // Debug.Log("index: " + index);
        return Transformers.Length > index ? Transformers[index] : null;
    }

    public NPVoxIModelFactory GetModelFactory(int num) // 0 = source, 1 = transform 1 etc ...
    {
        if (transformers == null || transformers.Length == 0)
        {
            return Source;
        }

        if (num == 0)
        {
            return Source;
        }

        if (num < 0 || num >= transformers.Length)
        {
            return transformers[transformers.Length - 1] as NPVoxIModelFactory;
        }
        else
        {
            return transformers[num - 1] as NPVoxIModelFactory;
        }
    }

    public void InvalidateFromStep(int num) // 0 = source, 1 = transform 1 etc ...
    {
        // Debug.Log("invalidatefromstep : " + num + " l: " + transformers.Length);
        for (int i = num; i <= transformers.Length; i++)
        {
            if (i == 0)
            {
                if (Source != null)
                {
                    Source.Invalidate();
                }
            }
            else
            {
                Transformers[i - 1].Invalidate();
            }
        }
        preOutput.Invalidate();
        output.Invalidate();
    }

    public int NumTransformers
    {
        get
        {
            return transformers.Length;
        }
    }

    // [System.Obsolete]
    public Mesh GetTransformedMesh()
    {
        return output.GetProduct();
    }

#if UNITY_EDITOR

    public NPVoxFrame DeepCopy(NPVoxAnimation targetAnimation)
    {
        NPVoxFrame other = new NPVoxFrame(targetAnimation);
        other.source = this.source;
        other.preOutput.Input = other.Source;
        other.previewAttachments = new NPVoxSocketAttachment[this.previewAttachments.Length];

        for (int i = 0; i < other.previewAttachments.Length; i++)
        {
            other.previewAttachments[i] = new NPVoxSocketAttachment();
            other.previewAttachments[i].meshFactory = this.previewAttachments[i].meshFactory;
            other.previewAttachments[i].sourceSocketName = this.previewAttachments[i].sourceSocketName;
            other.previewAttachments[i].targetSocketName = this.previewAttachments[i].targetSocketName;
            other.previewAttachments[i].visible = this.previewAttachments[i].visible;
        }

        foreach (NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> modelFactory in this.transformers)
        {
            NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> clonedFactory = (NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>)modelFactory.Clone();
            other.AppendTransformer(clonedFactory);
        }
        return other;
    }

    public void Destroy()
    {
        foreach (NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> modelFactory in this.transformers)
        {
            modelFactory.Destroy(); // destroy the product
            Undo.DestroyObjectImmediate(modelFactory);
        }
        if (preOutput != null)
        {
            preOutput.Destroy(); // destroy the product
            Undo.DestroyObjectImmediate(preOutput);
        }
        output.Destroy(); // destroy the product
        Undo.DestroyObjectImmediate(output);
    }

    public void InvalidateTransformation(NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer)
    {
        if (transformer != null)
        {
            InvalidateFromStep(GetTransformerIndex(transformer) + 1);
        }
    }

    public int GetTransformerIndex(NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer)
    {
        return ArrayUtility.IndexOf(transformers, transformer);
    }


#endif
}
