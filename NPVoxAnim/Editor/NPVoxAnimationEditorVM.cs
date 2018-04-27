using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public delegate void NPVoxAnimationEditorVMChangedHandler();

public class NPVoxAnimationEditorVM : ScriptableObject
{
    public NPVoxAnimationEditorVMChangedHandler OnMeshChange;
    public NPVoxAnimationEditorVMChangedHandler OnCheckForInvalidation;

    public enum Tool
    {
        NONE,
        AREA,
        CUSTOM1,
        CUSTOM2,
        CUSTOM3,
        CUSTOM4
    }

    public enum BrightenMode
    {
        SELECTED,
        // CURRENT,
        OFF
    }
    private static System.Object ClipboardContent = null;

    [SerializeField]
    private NPVoxAnimation animation;

    public NPVoxAnimation Animation
    {
        get
        {
            return animation;
        }
    }

    [SerializeField]
    private Tool currentTool;
    public Tool CurrentTool
    {
        get
        {
            return currentTool;
        }
    }

    public int CurrentCustomTool
    {
        get 
        {
            switch(CurrentTool)
            {
                case Tool.CUSTOM1: return 0;
                case Tool.CUSTOM2: return 1;
                case Tool.CUSTOM3: return 2;
                case Tool.CUSTOM4: return 3;
                default: return -1;
            }
        }
    }

    [SerializeField]
    private BrightenMode currentBrightenMode = BrightenMode.SELECTED;
    public BrightenMode CurrentBrightenMode
    {
        get
        {
            return currentBrightenMode;
        }
    }

    public NPVoxIModelFactory TotalModelFactory
    {
        get
        {
            NPVoxFrame frame = SelectedFrame;
            if (frame != null && frame.Source != null)
            {
                return SelectedFrame.GetModelFactory(-1);
            }
            return null;
        }
    }

    private NPVoxModelMarker modelMarker = null;
    public NPVoxModelMarker ModelMarker
    {
        get
        {
            if (modelMarker == null)
            {
                modelMarker = (NPVoxModelMarker) NPVoxModelMarker.CreateInstance<NPVoxModelMarker>();
            }

            return modelMarker;
        }
    }

    private uint hiddenBonesMask = 0;

    private NPVoxIModelFactory editorModelFactory = null;
    private NPVoxIModelFactory editorModelFactorySource = null;
    private int editorModelType = -1;
    public NPVoxIModelFactory EditorModelFactory
    {
        get
        {
            // TODO add a proper strategy pattern for this shit

            // set correct editorModelType
            bool forceRecreateEditorModelFactory = false;
            int newEditorModelType = 0;
            bool doMark = this.currentBrightenMode == BrightenMode.SELECTED && ( SelectedTransformer is NPVoxSkeletonTransformer || SelectedTransformer is NPVoxModelTransformer);
            bool putMarkerBeforeModel = false;
            NPVoxIModelFactory newSource = null;

            // find ocrrect source for our model factory
            {
                if (PreviousModelFactory != null && SelectedTransformer != null && (CurrentTool == NPVoxAnimationEditorVM.Tool.AREA))
                {
                    newSource = PreviousModelFactory;
                    putMarkerBeforeModel = false;
                    newEditorModelType |= 2;
                }

                // in all other tools we show the mesh with the current transformation applied
                else if (CurrentModelFactory != null && SelectedTransformer != null)
                {
                    newSource = CurrentModelFactory;
                    putMarkerBeforeModel = true;
                    newEditorModelType |= 4;
                }

                // when there is no transformation selected, we just show the final transformation result
                else if (TotalModelFactory != null)
                {
                    newSource = TotalModelFactory;
                    newEditorModelType |= 8;
                    doMark = false;
                }

                // I don't know, can this happen?
                else
                {
                    newSource = null;
                    doMark = false;
                }
            }

            // ensure our marker is correctly setup
            NPVoxModelMarker marker = ModelMarker;
            if (doMark)
            {
                if (putMarkerBeforeModel)
                {
                    if (marker.Input != ((NPipeIComposite)newSource).Input)
                    {
                        forceRecreateEditorModelFactory = true;
                    }

                    // ensure we regenerate the model in case the editorModelfactory has outdated input set
                    if (editorModelFactory != null && ((NPipeIComposite)editorModelFactory).Input != marker as NPipeIImportable)
                    {
                        forceRecreateEditorModelFactory = true;
                    }
                }
                else
                {
                    if (editorModelFactory != null && marker.Input != editorModelFactory )
                    {
                        forceRecreateEditorModelFactory = true;
                    }

                    // ensure we regenerate the model in case the editorModelfactory has outdated input set
                    if (editorModelFactory != null && ((NPipeIComposite)editorModelFactory).Input == marker as NPipeIImportable)
                    {
                        forceRecreateEditorModelFactory = true;
                    }
                }

                // update parameters for our marker
                if (SelectedTransformer is NPVoxSkeletonTransformer)
                {
                    if (!marker.AffectedArea.Equals(SelectedTransformer.GetProduct().BoundingBox))
                    {
                        marker.AffectedArea = SelectedTransformer.GetProduct().BoundingBox;
                        marker.Invalidate();
                        editorModelFactory.Invalidate();
                    }

                    if (marker.boneMask != ((NPVoxSkeletonTransformer)SelectedTransformer).BoneMask)
                    {
//                        Debug.Log("Setting Marker Bones Mask: " + ((NPVoxSkeletonTransformer)SelectedTransformer).BoneMask);
                        marker.boneMask = ((NPVoxSkeletonTransformer)SelectedTransformer).BoneMask;
                        marker.Invalidate();
                        editorModelFactory.Invalidate();
                    }

                    if (marker.hiddenBonesMask != hiddenBonesMask)
                    {
//                        Debug.Log("Setting Hidden Bones Mask: " + hiddenBonesMask);
                        marker.hiddenBonesMask = hiddenBonesMask;
                        marker.Invalidate();
                        editorModelFactory.Invalidate();
                    }
                }
                else if (SelectedTransformer is NPVoxModelTransformer)
                {
                    if (!(marker.AffectedArea.Equals(((NPVoxModelTransformer)SelectedTransformer).AffectedArea)) || marker.boneMask != 0)
                    {
                        marker.AffectedArea = ((NPVoxModelTransformer)SelectedTransformer).AffectedArea;
                        marker.boneMask = 0;
                        marker.hiddenBonesMask = 0;
                        marker.Invalidate();
                        editorModelFactory.Invalidate();
                    }
                }
            }
            else
            {
                // ensure we regenerate the model in case the editorModelfactory has outdated input set
                if (editorModelFactory != null && ((NPipeIComposite)editorModelFactory).Input == marker as NPipeIImportable)
                {
                    forceRecreateEditorModelFactory = true;
                }
            }

            // regenerate our editor model factory when needed
            if (forceRecreateEditorModelFactory || newEditorModelType != editorModelType || newSource != editorModelFactorySource)
            {
                editorModelType = newEditorModelType;
                editorModelFactorySource = newSource;

                if (editorModelFactory != null)
                {
                    DestroyImmediate(editorModelFactory as UnityEngine.Object);
                    editorModelFactory = null;
                }

                if (newSource != null)
                {
                    editorModelFactory = (NPVoxIModelFactory)newSource.Clone();

                    var a = editorModelFactory as NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>;
                    if (a != null)
                    {
                        a.StorageMode = NPipeStorageMode.MEMORY;
                    }

                    if (doMark)
                    {
                        if (putMarkerBeforeModel)
                        {
                            a.Input = marker;
                            marker.Input = ((NPipeIComposite)newSource).Input;
                        }
                        else
                        {
                            marker.Input = editorModelFactory;
                        }
                        marker.Invalidate();
                    }
                }
            }

            return editorModelFactory;
        }
    }

    public NPVoxIModelFactory PreviewModelFactory
    {
        get
        {
            NPVoxIModelFactory editorModelFactory = EditorModelFactory;

            // in case the EditorModelFactory set the modelmarker as last element, we retrn this for the preview
            if (ModelMarker.Input == editorModelFactory)
            {
                return ModelMarker;
            }
            else
            {
                return EditorModelFactory;
            }
        }
    }

    public NPVoxIModelFactory CurrentModelFactory
    {
        get
        {
            NPVoxFrame frame = SelectedFrame;
            if (frame != null && frame.Source != null)
            {
                if (selectedTransformationIndex > -1)
                {
                    // Debug.Log("yehaw");
                    return SelectedFrame.GetTransformer(selectedTransformationIndex) as NPVoxIModelFactory;
                }
                else
                {
                    // Debug.Log("fail");
                    return TotalModelFactory;
                }
            }
            return null;
        }
    }

    public NPVoxIModelFactory PreviousModelFactory
    {
        get
        {
            NPVoxFrame frame = SelectedFrame;
            if (frame != null && frame.Source != null && selectedTransformationIndex > -1)
            {
                return frame.GetModelFactory(selectedTransformationIndex);
            }
            return null;
        }
    }

    private NPVoxCompositeProcessorBase<NPVoxIModelFactory, Mesh> previewMeshOutput;
    public NPVoxCompositeProcessorBase<NPVoxIModelFactory, Mesh> PreviewMeshOutput
    {
        get
        {
            if (previewMeshOutput == null)
            {
                previewMeshOutput = (NPVoxCompositeProcessorBase<NPVoxIModelFactory, Mesh>)Animation.MeshFactory.Clone();

                // enable migration in clone in case the source requires migration too.
                if ( Animation.MeshFactory.NormalProcessors && Animation.MeshFactory.NormalProcessors.RequiresMigration )
                {
                    ( ( NPVoxMeshOutput ) previewMeshOutput ).NormalProcessors.RequiresMigration = true;
                    ( ( NPVoxMeshOutput ) previewMeshOutput ).Import();
                }

                previewMeshOutput.StorageMode = NPipeStorageMode.MEMORY;
            }

            if (previewMeshOutput.Input != PreviewModelFactory)
            {
                previewMeshOutput.Input = PreviewModelFactory;
                previewMeshOutput.Invalidate();
            }
            return previewMeshOutput;
        }
    }

    private Mesh cachedMesh;
//    private double lastMeshUpdateTime;
    public Mesh Mesh
    {
        get
        {
//            lastMeshUpdateTime = EditorApplication.timeSinceStartup;

            // when previewing, just take the generated mesh for performance
            if (isPlaying)
            {
                // Debug.Log("Using selecrted Mesh");
                cachedMesh = SelectedFrame.GetTransformedMesh();
            }
            else
            {
                cachedMesh = PreviewMeshOutput.GetProduct();
            }
            return cachedMesh;
        }
    }


    [SerializeField]
    private int selectedFrameIndex = -1;

    public int SelectedFrameIndex
    {
        get
        {
            return selectedFrameIndex;
        }

    }

    public NPVoxFrame SelectedFrame
    {
        get
        {
            if (animation == null)
            {
                return null;
            }

            if (selectedFrameIndex < 0 || (animation != null && selectedFrameIndex >= animation.Frames.Length))
            {
                return null;
            }

            return animation.Frames[selectedFrameIndex];
        }
    }

    public NPVoxFrame[] Frames
    {
        get
        {
            return animation && animation.Frames != null ? animation.Frames : new NPVoxFrame[0];
        }
    }

    public NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>[] Transformers
    {
        get
        {
            if (SelectedFrame == null)
            {
                return null;
            }
            return SelectedFrame.Transformers;
        }
    }

    [SerializeField]
    private int selectedTransformationIndex = -1;

    public int SelectedTransformationIndex
    {
        get
        {
            return selectedTransformationIndex;
        }
    }

    private bool previewIsPingPongBack = false;
    private bool isPlaying = false;
    private int beforePreviewFrame;
    public bool IsPlaying
    {
        get
        {
            return isPlaying;
        }
    }

    public void Preview()
    {
        AssetDatabase.SaveAssets();
        animation.EnsureAllMeshesLoaded();
        this.isPlaying = true;
        this.previewIsPingPongBack = false;
        this.beforePreviewFrame = this.SelectedFrameIndex;
        SelectFrame(0, true);
    }

    public void StopPreview()
    {
        this.isPlaying = false;
        SelectFrame(this.beforePreviewFrame, true);
    }

    public NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> SelectedTransformer
    {
        get
        {
            if (SelectedFrame == null)
            {
                return null;
            }
            if (selectedTransformationIndex < 0 || selectedTransformationIndex >= SelectedFrame.Transformers.Length)
            {
                return null;
            }

            return SelectedFrame.Transformers[selectedTransformationIndex];
        }
    }

    public NPVoxFrame GetFrameAt(int index)
    {
        if (Frames.Length > index && index >= 0)
        {
            return Frames[index];
        }
        return null;
    }

    public NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> GetTransformater(int index)
    {
        if (SelectedFrame != null && SelectedFrame.Transformers.Length > index && index >= 0)
        {
            return SelectedFrame.Transformers[index];
        }
        return null;
    }

    public void SelectAnimation(NPVoxAnimation animation)
    {
        if (this.animation != animation)
        {
            this.animation = animation;
            DestroyImmediate(previewMeshOutput as UnityEngine.Object);
            previewMeshOutput = null;
            DestroyImmediate(editorModelFactory as UnityEngine.Object);
            editorModelFactory = null;
            DestroyImmediate(modelMarker);
            modelMarker = null;
            cachedMesh = null;
            SetCurrentTool(Tool.NONE);
            InvalidateOutputMeshes();
            wipePreviewMeshFactories();
            SelectFrame(0, true);
            

        }
    }

    private void EnsureFrameIndexInBounds()
    {
        if (SelectedFrameIndex >= Frames.Length)
        {
            SelectFrame(SelectedFrameIndex - 1, true);
        }
        else if (SelectedFrameIndex < 0 && Frames.Length > 0)
        {
            SelectFrame(0, true);
        }
    }

    public int GetPreviousFrameIndex()
    {
        return (SelectedFrameIndex > 0 ? (selectedFrameIndex - 1) : Frames.Length -1);
    }

    public int GetNextFrameIndex()
    {
        return (SelectedFrameIndex < Frames.Length ? (selectedFrameIndex + 1) : 0);
    }

    public void SelectFrame(int frameIndex, bool noUndoRecord = false)
    {
        if (!noUndoRecord)
        {
            Undo.RecordObjects(new Object[] { this }, "Select Frame");
        }
        if (frameIndex < 0)
            frameIndex += Frames.Length;
        if (frameIndex >= Frames.Length)
            frameIndex -= Frames.Length;
        selectedFrameIndex = frameIndex;
        if (SelectedFrame != null)
            SelectedFrame.FixStuff();
        // InvalidateOutputMeshes();
        selectedTransformationIndex = -1;

        FireOnMeshChange();
    }

    public int GetPreviousTransformationIndex()
    {
        return (selectedTransformationIndex > 0 ? (selectedTransformationIndex - 1) : Transformers.Length -1);
    }

    public int GetNextTransformationIndex()
    {
        return (selectedTransformationIndex < Transformers.Length ? (selectedTransformationIndex + 1) : 0);
    }

    public void SelectTransformation(int transformationIndex, bool noUndoRecord = false)
    {
        hiddenBonesMask = 0;
        if (currentTool == Tool.NONE)
        {
            SetCurrentTool(Tool.NONE);
        }
        if (!noUndoRecord)
        {
            Undo.RecordObjects(new Object[] { this }, "Select Transformation");
        }
        if (selectedTransformationIndex < 0)
            selectedTransformationIndex += Transformers.Length;
        if (selectedTransformationIndex >= Transformers.Length)
            selectedTransformationIndex -= Transformers.Length;
        selectedTransformationIndex = transformationIndex;

        if (SelectedFrame != null)
            SelectedFrame.FixStuff();
        // InvalidateOutputMeshes();
//        SceneView.RepaintAll();

        FireOnMeshChange();
    }

    public void SetTransformationName(int transformationIndex, string newName)
    {
        if (this.Transformers.Length > transformationIndex)
        {
            Undo.RecordObjects(new Object[] { this, this.Transformers[transformationIndex] }, "Change Name");
            this.Transformers[transformationIndex].InstanceName = newName;
        }
    }

    public void MoveFrame(int frameIndex, int deltaIndex)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Move Frame " + frameIndex + " Left");
        if (animation.MoveFrame(frameIndex, deltaIndex) >= 0)
        {
            this.SelectFrame(frameIndex + deltaIndex, true);
        }
    }

    public void MoveTransformation(int transformationIndex, int deltaIndex)
    {
        if (SelectedFrame == null)
        {
            Debug.LogError("No Frame selected");
            return;
        }

        Undo.RecordObjects(new Object[] { this, animation }, "Move Transformation " + transformationIndex + " Up");
        SelectedFrame.MoveTransformer(transformationIndex, deltaIndex);
        EditorUtility.SetDirty(animation);

        if (selectedTransformationIndex == transformationIndex)
        {
            SelectTransformation(transformationIndex + deltaIndex, true);
        }
        else if (selectedTransformationIndex > -1)
        {
            SelectTransformation(-1, true);
        }
    }


    public void AddFrameAt(int frameIndex)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add frame at " + frameIndex);
        animation.AddFrameAt(frameIndex, this.SelectedFrame);
        this.SelectFrame(frameIndex, true);
    }

    public void SetFrameModel(int frameIndex, NPVoxIModelFactory source)
    {
        Undo.RecordObject(animation, "change model");
        NPVoxFrame frame = GetFrameAt(frameIndex);
        frame.Source = source;
        EditorUtility.SetDirty(Animation);
        InvalidateOutputMeshes();
    }

    public void RemoveFrameAt(int frameIndex)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Remove frame at " + frameIndex);
        animation.RemoveFrameAt(frameIndex);
        EnsureFrameIndexInBounds();
        InvalidateOutputMeshes();
    }

    public void RemoveTransformationAt(int index)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Remove Transformation at " + index);
        SelectedFrame.DestroyTransformer(index);
        InvalidateOutputMeshes();
        selectedTransformationIndex = -1;
    }

    public void AddTransformation(NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> template = null)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add Transformation ");
        NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer;
        if (template != null)
        {
            transformer = (NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>)template.Clone();
        }
        else
        {
            transformer = (NPVoxModelTransformer)NPVoxModelTransformer.CreateInstance(typeof(NPVoxModelTransformer));
            if (this.CurrentModelFactory != null && ((NPVoxModelTransformer)transformer))
            {
                NPVoxModel model = this.CurrentModelFactory.GetProduct();
                UnityEngine.Assertions.Assert.IsNotNull(model);
                ((NPVoxModelTransformer)transformer).AffectedArea = NPVoxBox.FromCenterSize(model.BoundingBox.RoundedCenter, new NPVoxCoord(3, 3, 3));
            }
        }
        SelectedFrame.AppendTransformer(transformer);
        InvalidateOutputMeshes();
        SelectTransformation(Transformers.Length - 1, true);
        this.SetCurrentTool(Tool.AREA);
    }

      
    // ===================================================================================================
    // Transformation Editing
    // ===================================================================================================

    public bool IsAreaSelectionActive()
    {
        if ((SelectedTransformer is NPVoxSkeletonBuilder) && ((NPVoxSkeletonBuilder)SelectedTransformer).CurrentEditedBox != null)
        {
            return true;
        }

        if (CurrentTool != NPVoxAnimationEditorVM.Tool.AREA)
        {
            return false;
        }

        if (!(SelectedTransformer is NPVoxModelTransformer))
        {
            return false;
        }
        return true;
    }

    public bool IsBoneSelectionActive()
    {
        if ((SelectedTransformer is NPVoxSkeletonTransformer) && CurrentTool == NPVoxAnimationEditorVM.Tool.AREA)
        {
            return true;
        }
        return false;
    }

    public bool IsSceneEditToolsActive()
    {
        if (!(SelectedTransformer is NPVoxISceneEditable) || (PreviousModelFactory == null))
        {
            return false;
        }
        return true;
    }

    public void MaximizeAffectedArea()
    {
        // affected area picker
        NPVoxIModelFactory modelFactory = SelectedFrame.Source;
        if (modelFactory != null)
        {
            ChangeAffectedBox(modelFactory.GetProduct().BoundingBox);
        }
    }

    public NPVoxBox GetAffectedBox()
    {
        if (SelectedTransformer is NPVoxSkeletonBuilder)
        {
            if (((NPVoxSkeletonBuilder)SelectedTransformer).CurrentEditedBox != null)
            {
                return ((NPVoxSkeletonBuilder)SelectedTransformer).CurrentEditedBox;
            }
            else
            {
                return NPVoxBox.INVALID;
            }
        }
        else
        {
            return ((NPVoxModelTransformer)SelectedTransformer).AffectedArea;
        }
    }

    public void ChangeAffectedBox(NPVoxBox newBox)
    {
        ChangeTransformation((NPVoxISceneEditable transformer) =>
            {
                if (transformer is NPVoxSkeletonBuilder)
                {
                    if( ((NPVoxSkeletonBuilder)transformer).CurrentEditedBox != null)
                    {
                        ((NPVoxSkeletonBuilder)transformer).CurrentEditedBox.LeftDownBack = newBox.LeftDownBack;
                        ((NPVoxSkeletonBuilder)transformer).CurrentEditedBox.RightUpForward = newBox.RightUpForward;
                    }
                    return false;
                    
                }
                else
                {
                    ((NPVoxModelTransformer)transformer).AffectedArea = newBox;
                    return true;
                }
            }
        );
    }


    public void SetBoneMask(uint mask, bool includingDescendants)
    {
        NPVoxBoneModel transformedModel = CurrentModelFactory.GetProduct() as NPVoxBoneModel;
        NPVoxBone[] allBones = ((NPVoxBoneModel)transformedModel).AllBones;
        SetBoneMask(includingDescendants ? NPVoxBone.GetMaskWithDescendants(ref allBones, mask) : mask);
    }

    public void ToggleBoneMask(uint mask, bool includingDescendants)
    {
        NPVoxSkeletonTransformer t = ((NPVoxSkeletonTransformer)SelectedTransformer);
        NPVoxBoneModel transformedModel = CurrentModelFactory.GetProduct() as NPVoxBoneModel;
        NPVoxBone[] allBones = ((NPVoxBoneModel)transformedModel).AllBones;
        uint toggleMask = includingDescendants ? NPVoxBone.GetMaskWithDescendants(ref allBones, mask) : mask;
        if ((mask & t.BoneMask) != 0)
        {
            SetBoneMask(t.BoneMask & ~toggleMask);
        }
        else
        {
            SetBoneMask(t.BoneMask | toggleMask);
        }
    }

    public void SetBoneMask(uint mask)
    {
//        Debug.Log("Setting Bone Mask: " + mask);

        ChangeTransformation((NPVoxISceneEditable transform) => 
            {
                if (transform is NPVoxSkeletonTransformer)
                {
                    ((NPVoxSkeletonTransformer)transform).BoneMask = mask;
                    return true;
                }
                return false;
            }
        );
    }
    
    public List<NPVoxBox> GetNonEditableBoxes()
    {
        var t = (SelectedTransformer as NPVoxSkeletonBuilder);
        if(t != null && t.CurrentEditedBone != null)
        {
            List<NPVoxBox> boxes = t.GetCurrentSelectedChildBoxes();
            boxes.Remove(t.CurrentEditedBox);
            return boxes;
        }
        return null;
    }

    public void ResetTransformation()
    {
        ChangeTransformation((NPVoxISceneEditable transformer) => 
            {
                transformer.ResetSceneTools();
                return true;
            }
        );
    }

    public void ChangeTransformation(System.Func<NPVoxISceneEditable, bool> apply)
    {
        Undo.RecordObjects(new Object[] { this, SelectedTransformer }, "Apply Transformation Change with Preview");
        if (editorModelFactorySource == SelectedTransformer as NPVoxIModelFactory)
        {
            Undo.RecordObjects(new Object[] { EditorModelFactory as UnityEngine.Object }, "Apply Transformation Change with Preview");
            if (apply(EditorModelFactory as NPVoxISceneEditable))
            {
                SelectedTransformerChanged();
            }
        }
        if (apply(SelectedTransformer as NPVoxISceneEditable))
        {
            SelectedTransformerChanged();
        }
    }

    // ===================================================================================================
    // Preview Meshes
    // ===================================================================================================

    private double invalidationRequestedAt = 0.0;
    private NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> invalidatedTransformation = null;
    private NPVoxFrame frame = null;

    public bool DelayedRefreshEnabled = true;

    public void SelectedTransformerChanged()
    {
        EditorUtility.SetDirty(Animation);
        EditorUtility.SetDirty(SelectedTransformer);
        InvalidateOutputMeshes(SelectedTransformer);
    }

    public void InvalidateOutputMeshes(NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transform = null, bool immediateRefresh = false)
    {
        if (frame != null && frame != SelectedFrame)
        {
            ProcessInvalidations(true);
        }
        if (invalidatedTransformation != null && invalidatedTransformation != transform)
        {
            ProcessInvalidations(true);
        }

        invalidatedTransformation = transform;
        frame = SelectedFrame;
        invalidationRequestedAt = invalidationRequestedAt < 1 ? EditorApplication.timeSinceStartup : invalidationRequestedAt;
        ProcessInvalidations(!DelayedRefreshEnabled || immediateRefresh);
    }

    public void ProcessInvalidations(bool force = false)
    {
        if (force || (invalidationRequestedAt > 0 && EditorApplication.timeSinceStartup - invalidationRequestedAt > 0.1f))
        {
            frame = null;
            invalidationRequestedAt = 0;
            if (invalidatedTransformation != null)
            {
                if (editorModelFactorySource == invalidatedTransformation as NPVoxIModelFactory)
                {
                    EditorModelFactory.Invalidate();
                }

                SelectedFrame.InvalidateTransformation(invalidatedTransformation);
                invalidatedTransformation = null;
            }
            if (ModelMarker != null)
            {
                ModelMarker.Invalidate();
            }
            PreviewMeshOutput.Invalidate();
            invalidatePreviewMeshFactories();
            FireOnMeshChange();
        }
        else
        {
            if (invalidationRequestedAt > 0)
            {
                if( OnCheckForInvalidation != null) OnCheckForInvalidation();
            }
        }
    }

    public void RegeneratePreview()
    {
        editorModelType = -1;
    }

    // ===================================================================================================
    // Animation Editing
    // ===================================================================================================

    public void SetFPS(int newValue)
    {
        if (newValue != Animation.FPS)
        {
            Undo.RecordObject(animation, "change animation params");
            Animation.FPS = newValue;
            EditorUtility.SetDirty(animation);
        }
    }

    public void SetPingPong(bool newValue)
    {
        if (newValue != Animation.PingPong)
        {
            Undo.RecordObject(animation, "change animation pingpong");
            Animation.PingPong = newValue;
            EditorUtility.SetDirty(animation);
        }
    }

    public void SetLoop(bool newValue)
    {
        if (newValue != Animation.Loop)
        {
            Undo.RecordObject(animation, "change animation params");
            Animation.Loop = newValue;
            EditorUtility.SetDirty(animation);
        }
    }

    public string[] GetTools()
    {
        if (SelectedTransformer is NPVoxISceneEditable)
        {
            string[] tools =  ((NPVoxISceneEditable)(SelectedTransformer)).GetSceneEditingTools();
            while (tools.Length < 4)
            {
                ArrayUtility.Add(ref tools, "");
            }
            ArrayUtility.Add(ref tools, "Selection");
            return tools;
        }
        return new string[] { "Tool 1", "Tool 2", "Tool 3", "Tool 4", "Selection" };
    }

    public bool SetCurrentTool(Tool tool)
    {
        bool changed = this.currentTool != tool;
        this.currentTool = tool;

        if (this.currentTool != Tool.NONE)
        {
            // Reset unity tool to remove drawing of that damn default gizmo
            Tools.current = UnityEditor.Tool.None;
        }

        if (changed)
        {
            FireOnMeshChange();
        }

        return changed;
    }

    public bool UpdateCurrentToolFromSceneView(UnityEditor.Tool tool)
    {
        switch (tool)
        {
            case UnityEditor.Tool.Move: return SetCurrentTool(Tool.CUSTOM1);
            case UnityEditor.Tool.Rotate: return SetCurrentTool(Tool.CUSTOM2);
            case UnityEditor.Tool.Scale: return SetCurrentTool(Tool.CUSTOM3);
            case UnityEditor.Tool.Rect: return SetCurrentTool(Tool.AREA);
            default: return SetCurrentTool(Tool.NONE);
        }
    }

    public bool SetCurrentBrightenMode(BrightenMode brightenMode)
    {
        bool changed = this.currentBrightenMode != brightenMode;
        this.currentBrightenMode = brightenMode;

        if (changed)
        {
            InvalidateOutputMeshes();
        }

        return changed;
    }

    public void Copy()
    {
        if (this.SelectedTransformer != null)
        {
            ClipboardContent = this.SelectedTransformer;
        }
    }


    public void CopyFrame()
    {
        if (this.SelectedFrame != null)
        {
            ClipboardContent = this.SelectedFrame;
        }
    }

    public void CopyTransformation(int index)
    {
        ClipboardContent = this.Transformers[index];
    }

    public void Paste()
    {
        if (HasTransformationToPaste())
        {
            NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer = ClipboardContent as NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>;
            this.AddTransformation(transformer);
        }

        if (HasFrameToPaste())
        {
            NPVoxFrame frame = ClipboardContent as NPVoxFrame;
            Undo.RecordObjects(new Object[] { this, animation }, "Add frame from clipboard at " + selectedFrameIndex);
            animation.AddFrameAt(selectedFrameIndex, frame);
            SelectFrame(selectedFrameIndex, true);
        }
    }

    public void DuplicateTransformation(int index)
    {
        NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer = this.Transformers[index] as NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>;
        this.AddTransformation(transformer);
        int iIndex = this.Transformers.Length - 1;
        while ( iIndex != index + 1 )
        {
            MoveTransformation( iIndex, -1 );
            iIndex--;
        }
        SelectTransformation( iIndex, true );
    }

    public bool HasTransformationToPaste()
    {
        return (ClipboardContent is NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>) && SelectedFrame != null;
    }

    public bool HasFrameToPaste()
    {
        return ClipboardContent is NPVoxFrame;
    }

    public void UpdatePreview()
    {
        if (!isPlaying)
        {
            return;
        }
        if (SelectedFrameIndex == Frames.Length - 1)
        {
            if (Animation.PingPong)
            {
                previewIsPingPongBack = true;
                SelectFrame(SelectedFrameIndex - 1, true);
            }
            else if (Animation.Loop)
            {
                SelectFrame(0, true);
            }
            else
            {
                StopPreview();
            }
        }
        else if (SelectedFrameIndex == 0 && previewIsPingPongBack)
        {
            if (Animation.Loop)
            {
                previewIsPingPongBack = false;
                SelectFrame(0, true);
            }
            else
            {
                StopPreview();
            }
        }
        else
        {
            if (previewIsPingPongBack)
            {
                SelectFrame(SelectedFrameIndex - 1, true);
            }
            else
            {
                SelectFrame(SelectedFrameIndex + 1, true);
            }
        }
    }

    public void OnUndoPerformed()
    {
//        if (SelectedFrame != null && SelectedTransformer != null)
//        {
//            SelectedFrame.InvalidateTransformation(SelectedTransformer);
//        }
//        if (EditorModelFactory != null)
//        {
//            EditorModelFactory.Invalidate();
//        }
        InvalidateOutputMeshes(SelectedTransformer, true);
        if (ModelMarker != null)
        {
            ModelMarker.Invalidate();
        }
    }

    public void ApplyHiddenBoneMask()
    {
        uint newMask = 0;
        if (SelectedTransformer is NPVoxSkeletonTransformer)
        {
            newMask = ((NPVoxSkeletonTransformer)SelectedTransformer).BoneMask;
        }

        if (newMask != hiddenBonesMask)
        {
            if (SelectedTransformer is NPVoxSkeletonTransformer)
            {
                SetBoneMask( 0 );
                this.hiddenBonesMask = newMask;
                SelectedTransformerChanged();
            }
            else
            {
                Undo.RecordObjects(new Object[] { this }, "change hidden bonemask");
                this.hiddenBonesMask = newMask;
                if (this.ModelMarker)
                {
                    this.ModelMarker.Invalidate();
                }
                InvalidateOutputMeshes();
            }
        }
    }

    //======================================================================================================================================
    // Tools
    //======================================================================================================================================

    public void AddMirrorTransformators()
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add Mirror Transformations ");
        NPVoxModelFlipper transformer;

        // flip
        transformer = (NPVoxModelFlipper)NPVoxModelTransformer.CreateInstance(typeof(NPVoxModelFlipper));
        transformer.XAxis = new NPVoxCoord(-1, 0, 0);
        SelectedFrame.PrependTransformer(transformer);

        // flip back
        transformer = (NPVoxModelFlipper)NPVoxModelTransformer.CreateInstance(typeof(NPVoxModelFlipper));
        transformer.XAxis = new NPVoxCoord(-1, 0, 0);
        SelectedFrame.AppendTransformer(transformer);

        InvalidateOutputMeshes();
    }

    public void AddSocketCombiner()
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add Socket Combiner ");

        NPVoxModelSocketCombiner combiner = (NPVoxModelSocketCombiner)NPVoxModelTransformer.CreateInstance(typeof(NPVoxModelSocketCombiner));
        int index = SelectedFrame.AddTransformerAt(combiner, selectedTransformationIndex);
        InvalidateOutputMeshes();
        SelectTransformation(index, true);
    }

    public void AddSocketTransformer()
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add Socket Transformer ");

        NPVoxSocketTransformer transformer = (NPVoxSocketTransformer)NPVoxSocketTransformer.CreateInstance(typeof(NPVoxSocketTransformer));
        int index = SelectedFrame.AddTransformerAt(transformer, selectedTransformationIndex);
        InvalidateOutputMeshes();
        SelectTransformation(index, true);
    }

    public void AddTransformer(System.Type type)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add Misc Transformer ");

        NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer = UnityEngine.ScriptableObject.CreateInstance(type) as NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>;
        int index = SelectedFrame.AddTransformerAt(transformer, selectedTransformationIndex);
        InvalidateOutputMeshes();
        SelectTransformation(index, true);
    }

    public bool IsSkeletonBuilderAppendable()
    {
        NPVoxIModelFactory factory = EditorModelFactory as NPVoxIModelFactory;
        return factory != null && !(factory.GetProduct() is NPVoxBoneModel);
    }

    public bool IsSkeletonTransformerAppendable()
    {
        NPVoxIModelFactory factory = EditorModelFactory as NPVoxIModelFactory;
        return factory != null && (factory.GetProduct() is NPVoxBoneModel);
    }

    public void AddSkeletonBuilder()
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add Skeleton Builder");

        NPVoxSkeletonBuilder transformer = (NPVoxSkeletonBuilder)NPVoxSocketTransformer.CreateInstance(typeof(NPVoxSkeletonBuilder));
        int index = SelectedFrame.AddTransformerAt(transformer, selectedTransformationIndex);
        InvalidateOutputMeshes();
        SelectTransformation(index, true);
    }

    public void AddSkeletonTransformer()
    {
        Undo.RecordObjects(new Object[] { this, animation }, "Add Skeleton Transformer");

        NPVoxSkeletonTransformer transformer = (NPVoxSkeletonTransformer)NPVoxSocketTransformer.CreateInstance(typeof(NPVoxSkeletonTransformer));
        int index = SelectedFrame.AddTransformerAt(transformer, selectedTransformationIndex);
        InvalidateOutputMeshes();
        SelectTransformation(index, true);
    }

    //======================================================================================================================================
    // Socket Preview Functionality
    //======================================================================================================================================

    protected bool drawSockets = true;

    public bool DrawSockets
    {
        get
        {
            return drawSockets;
        }
    }

    public void SetDrawSockets(bool value)
    {
        Undo.RecordObjects(new Object[] { this }, "set draw Sockets enabled");
        drawSockets = value;
        FireOnMeshChange();
    }

    public void SetPreviewSocketEnabled(string targetSocketName, bool val)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "set preview socket enabled");
        NPVoxSocketAttachment p = SelectedFrame.GetPreviewAttachmentForTargetSocket(targetSocketName, true);
        p.visible = val;
        FireOnMeshChange();
    }

    public bool GetPreviewSocketEnabled(string targetSocketName)
    {
        NPVoxSocketAttachment p = SelectedFrame.GetPreviewAttachmentForTargetSocket(targetSocketName, false);
        return p == null || p.visible;
    }

    public string[] GetPreviewTargetSocketNames()
    {
        if (this.SelectedFrame == null || SelectedFrame.Source == null)
        {
            return new string[]{};
        }

        NPVoxModel model = SelectedFrame.Source.GetProduct();
        if(model != null && model.SocketNames.Length > 0)
        {
            return model.SocketNames;
        }

        return new string[]{};
    }

    public NPVoxIMeshFactory GetPreviewFactoryForTargetSocket(string targetSocketName)
    {
        NPVoxSocketAttachment p = SelectedFrame.GetPreviewAttachmentForTargetSocket(targetSocketName, false);
        if (p != null)
        {
            return p.meshFactory;
        }
        return null;
    }

    public void SetPreviewFactoryForTargetSocket(string targetSocketName, NPVoxIMeshFactory meshFactory)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "set preview socket factory enabled");
        NPVoxSocketAttachment p = SelectedFrame.GetPreviewAttachmentForTargetSocket(targetSocketName, true);
        p.meshFactory = meshFactory as NPVoxMeshOutput;
        wipePreviewMeshFactories(targetSocketName);
    }

    public string[] GetSourceSocketsForTargetSocket(string targetSocketName)
    {
        NPVoxIModelFactory modelFactory = GetPreviewModelFactoryForTargetSocket(targetSocketName);
        if (modelFactory != null)
        {
            NPVoxModel model = modelFactory.GetProduct();
            return model.SocketNames;
        }
        return null;
    }

    protected NPVoxIModelFactory GetPreviewModelFactoryForTargetSocket(string targetSocketName)
    {
        NPVoxIMeshFactory meshFactory = GetPreviewFactoryForTargetSocket(targetSocketName);
        NPipeIComposite composite = meshFactory as NPipeIComposite;
        if(composite != null && composite.Input is NPVoxIModelFactory)
        {
            return composite.Input as NPVoxIModelFactory;
        }
        return null;
    }
    
    public string GetSourceSocketForTargetSocket(string targetSocketName)
    {
        NPVoxSocketAttachment p = SelectedFrame.GetPreviewAttachmentForTargetSocket(targetSocketName, false);
        if (p != null)
        {
            return p.sourceSocketName;
        }
        return null;
    }

    public void SetSourceSocketForTargetSocket(string targetSocketName, string sourceSocketName)
    {
        Undo.RecordObjects(new Object[] { this, animation }, "set preview source socket");
        NPVoxSocketAttachment p = SelectedFrame.GetPreviewAttachmentForTargetSocket(targetSocketName, true);
        p.sourceSocketName = sourceSocketName;
        FireOnMeshChange();
    }

    public string[] GetSocketPreviewTargetNames()
    {
        List<string> names = new List<string>();
        foreach (string targetSocketName in GetPreviewTargetSocketNames())
        {
            names.Add(targetSocketName);
        }
        return names.ToArray();
    }

    public NPVoxIMeshFactory GetSocketPreviewMeshFactoryForCurrentFrame(string targetSocketName)
    {
        NPVoxSocketAttachment p = SelectedFrame.GetPreviewAttachmentForTargetSocket(targetSocketName);

        if (this.selectedFrameIndex < 0 || p == null)
        {
            return null;
        }

        NPVoxIModelFactory modelFactory = GetPreviewModelFactoryForTargetSocket(targetSocketName);

        string inputSocketName = p.sourceSocketName;

        if (p.outputMeshFactory == null)
        {
            NPVoxIMeshFactory meshFactory = GetPreviewFactoryForTargetSocket(targetSocketName);

            if (meshFactory == null)
            {
                return null;
            }

            NPVoxModelSocketCombiner combiner = NPVoxModelCombiner.CreateInstance<NPVoxModelSocketCombiner>();
            combiner.Input = modelFactory;
            combiner.InputSocketName = inputSocketName;
            combiner.TargetSocketName = targetSocketName;
            combiner.ResolveConflictMethod = NPVoxModelTransformationUtil.ResolveConflictMethodType.FILL_GAPS;
            combiner.Target = (UnityEngine.Object)SelectedFrame.PreOutput.Input;
            combiner.StorageMode = NPipeStorageMode.MEMORY;

            NPVoxMeshOutput previewFactory = (NPVoxMeshOutput)meshFactory.Clone();
            previewFactory.StorageMode = NPipeStorageMode.MEMORY;
            previewFactory.Input = combiner;

            p.outputMeshFactory = previewFactory;
        }
        else
        {
            NPVoxModelSocketCombiner combiner = ((NPVoxModelSocketCombiner)((NPVoxMeshOutput)p.outputMeshFactory).Input);
            // check if something changed in the meanwhipe
            if (combiner.Target != TotalModelFactory as UnityEngine.Object || combiner.InputSocketName != inputSocketName)
            {
                p.outputMeshFactory.Invalidate();
                combiner.Target = (UnityEngine.Object) TotalModelFactory;
                combiner.InputSocketName = inputSocketName;
                combiner.Invalidate();
            }
        }

        return p.outputMeshFactory;
    }

    private void invalidatePreviewMeshFactories()
    {
        var tsn = GetPreviewTargetSocketNames();
        foreach(NPVoxFrame f in Frames)
        {
            foreach (string socket in tsn)
            {
                var p = f.GetPreviewAttachmentForTargetSocket(socket, false);
                if (p != null && p.outputMeshFactory != null)
                {
                    ((NPipeIComposite)p.outputMeshFactory).Input.Invalidate();
                    p.outputMeshFactory.Invalidate();
                }
            }
        }
    }

    private void wipePreviewMeshFactories()
    {
        var tsn = GetPreviewTargetSocketNames();
        foreach (string socket in tsn)
        {
            wipePreviewMeshFactories(socket);
        }
    }

    private void wipePreviewMeshFactories(string targetSocketName )
    {
//        var tsn = GetPreviewTargetSocketNames();
        foreach (NPVoxFrame f in Frames)
        {
            var p = f.GetPreviewAttachmentForTargetSocket(targetSocketName, false);
            if (p != null && p.outputMeshFactory != null)
            {
                ((NPipeIComposite)p.outputMeshFactory).Input.Invalidate();
                p.outputMeshFactory.Invalidate();

                NPipeIImportable combiner = ((NPipeIComposite)p.outputMeshFactory).Input;
                combiner.Destroy();  // TODO: isn't this called automatically?
                Object.DestroyImmediate((UnityEngine.Object)combiner);

                p.outputMeshFactory.Destroy(); // TODO: isn't this called automatically?
                Object.DestroyImmediate( (UnityEngine.Object) p.outputMeshFactory );
                p.outputMeshFactory = null;
            }
        }
    }

      
    // ===================================================================================================
    // Misc
    // ===================================================================================================

    protected void FireOnMeshChange()
    {
        if(OnMeshChange != null) OnMeshChange();
    }

    public void DebugButton()
    {
        if (PreviewMeshOutput == null)
        {
            Debug.LogError("No PreviewMeshOutput!!!");
        }
        Debug.Log("PreviewMeshOutput: " + NPipelineUtils.GetPipelineDebugString(PreviewMeshOutput, true));
    }
}