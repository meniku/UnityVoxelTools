using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPVoxAnimation : NPipeContainer
{
    [SerializeField, HideInInspector, System.Obsolete]
    public string AssetUID;
    
    [HideInInspector]
    public NPVoxFrame[] Frames = new NPVoxFrame[0];
    
    [HideInInspector]
    public NPVoxMeshOutput MeshFactory = null;

    [HideInInspector]
    public Material[] PreviewMaterials = new Material[0];
    
    public int FPS = 8;
    public bool Loop = true;
    public bool PingPong = false;
    
    public void EnsureAllMeshesLoaded()
    {
        for(int i = 0; i < Frames.Length; i++)
        {
            Frames[i].GetTransformedMesh();
        }
    }

    public override void OnImport()
    {
        MeshFactory.isTemplate = true;
    }
    
    #if UNITY_EDITOR
    override public UnityEngine.Object[] GetAllSelectableFactories()
    {
        List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
        foreach (NPVoxFrame frame in Frames)
        {
            if (frame.PreOutput)
            {
                objects.Add(frame.PreOutput);
            }
        }
        return objects.ToArray();
    }

    override public void OnAssetCreated()
    {
        base.OnAssetCreated();
        NPVoxMeshOutput output = NPipelineUtils.GetByType<NPVoxMeshOutput>( NPVoxUtils.GetTemplatePipeline() )[0];
        MeshFactory = (NPVoxMeshOutput) output.Clone();
        MeshFactory.StorageMode = NPipeStorageMode.RESOURCE_CACHE;
        MeshFactory.Input = null;
        MeshFactory.InstanceName = "_TEMPLATE_";
        NPipelineUtils.CreateAttachedPipe(UnityEditor.AssetDatabase.GetAssetPath(this), MeshFactory);
        UnityEditor.AssetDatabase.SaveAssets();
    }

    public NPVoxFrame GetFrameAt(int index)
    {
        if (Frames.Length > index && index >= 0)
        {
            return Frames[index];
        }
        return null;
    }

    public int MoveFrame(int frameIndex, int deltaIndex)
    {
        NPVoxFrame[] frames = Frames;
        NPVoxFrame swapWith = GetFrameAt(frameIndex + deltaIndex);
        if (swapWith == null)
        {
            return -1;
        }
        if (Mathf.Abs(deltaIndex) > 1)
        {
            Debug.LogError("deltaIndex > 1 not supported yet");
            return -1;
        }

        ArrayUtility.RemoveAt(ref frames, frameIndex + deltaIndex);
        ArrayUtility.Insert(ref frames, frameIndex, swapWith);
        Frames = frames;
        EditorUtility.SetDirty(this);
        return frameIndex + deltaIndex;
    }

    public NPVoxFrame AppendFrame(NPVoxFrame template = null)
    {
        return AddFrameAt(Frames.Length, template);
    }

    public NPVoxFrame AddFrameAt(int frameIndex, NPVoxFrame template = null)
    {
        NPVoxFrame[] frames = Frames;
        NPVoxFrame newFrame = template != null ? template.DeepCopy(this) : new NPVoxFrame(this);
        // Debug.Log("adding frame at " + i);
        ArrayUtility.Insert(ref frames, frameIndex, newFrame);
        Frames = frames;
        EditorUtility.SetDirty(this);
        return newFrame;
    }

    public void RemoveFrameAt(int frameIndex)
    {
        NPVoxFrame[] frames = Frames;
        NPVoxFrame frame = frames[frameIndex];
        ArrayUtility.RemoveAt(ref frames, frameIndex);
        Frames = frames;
        EditorUtility.SetDirty(this);
        frame.Destroy();
    }

    #endif

    override public string GetDisplayName(NPipeIImportable importable)
    {
        int frameNum = 1;
        foreach (NPVoxFrame frame in Frames)
        {
            if (frame.PreOutput == importable as NPVoxModelForwarder)
            {
                return "Frame " + frameNum;
            }
            frameNum++;
        }
//        MeshFactory.InstanceName = "_TEMPLATE_";
        return "-Invalid-";
    }
}
