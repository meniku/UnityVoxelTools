using UnityEngine;
using System.Collections.Generic;

[NPipeAppendableAttribute("Skeleton Transformer", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxSkeletonTransformer : NPVoxModelTransformerBase
{
    [SerializeField]
    private uint boneMask = 0;
    public uint BoneMask
    {
        get
        {
            return boneMask;
        }
        set
        {
            boneMask = value;
            regenerateName = true;
        }
    }

    [HideInInspector, SerializeField]
    private bool regenerateName = true;

//    public bool colorize = false;

    [HideInInspector]
    public Matrix4x4 Matrix = Matrix4x4.identity;

    [HideInInspector]
    public Vector3 PivotOffset = Vector3.zero; // offset from AffectedArea.SaveCenter

    [HideInInspector]
    public NPVoxModelTransformationUtil.ResolveConflictMethodType ResolveConflictMethod = NPVoxModelTransformationUtil.ResolveConflictMethodType.FILL_GAPS;

    override public string GetTypeName()
    {
        return "Skeleton Transformer";
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }

        NPVoxBoneModel model = ((NPVoxIModelFactory)Input).GetProduct() as NPVoxBoneModel;

        if (model == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "Can only transform bone models");
        }

        // hack to center pivot on selected bones
        if (regenerateName)
        {
            RecenterBonePivot(model);
        }

        RegenerateName(model);

        NPVoxBox affectedBox = GetAffectedBox();
        if (affectedBox.Equals(NPVoxBox.INVALID))
        {
            NPVoxModel newInstance = NPVoxModel.NewInstance(model, reuse);
            newInstance.CopyOver(model);
            newInstance.RecalculateNumVoxels(true);
            return newInstance;
        }
        else
        {
            reuse = NPVoxModelTransformationUtil.MatrixTransform(model, affectedBox, boneMask, Matrix, PivotOffset, ResolveConflictMethod, reuse);
            reuse.RecalculateNumVoxels(true);
            return reuse;
        }
    }



    #if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags & ~NPipeEditFlags.INPUT);

        GUILayout.BeginHorizontal();
        UnityEditor.EditorGUILayout.LabelField("Resolve Conflicts");

        NPVoxModelTransformationUtil.ResolveConflictMethodType newSelected = (NPVoxModelTransformationUtil.ResolveConflictMethodType) GUILayout.SelectionGrid((int)ResolveConflictMethod, new string[] { "NONE", "CLOSEST", "FILL_GAPS" }, 3);
        if (newSelected != this.ResolveConflictMethod)
        {
            changed = true; 
            ResolveConflictMethod = newSelected;
        }
        GUILayout.EndHorizontal();

        return changed;
    }

    #endif
      
    // ===================================================================================================
    // Tools
    // ===================================================================================================

    public void RecenterBonePivot(NPVoxBoneModel model)
    {
        NPVoxBone[] bones = NPVoxBone.GetRootBones(ref model.AllBones, NPVoxBone.GetBonesInMask(ref model.AllBones, boneMask) );
        if( bones.Length == 1)
        {
            Vector3 pivotOrigin = GetAffectedBox().SaveCenter;
            Vector3 pivotForSingleBone = model.GetAffectedArea( 1u << ( bones[0].ID -1 )).SaveCenter;
            PivotOffset = pivotForSingleBone - pivotOrigin;
        }
    }

    override public void SetTranslation(Vector3 translation)
    {
//        UnityEditor.Undo.RecordObject(this, "SetTranslation");
        if (float.IsNaN(translation.x) || float.IsNaN(translation.y) || float.IsNaN(translation.z))
        {
            return;
        }
        Matrix = Matrix4x4.TRS(translation, GetRotation(), GetScale());
    }

    override public Vector3 GetTranslation()
    {
        return Matrix4x4Util.GetPosition(Matrix);
    }

    override public void SetRotation(Quaternion quat)
    {
//        UnityEditor.Undo.RecordObject(this, "SetRotation");
        if (float.IsNaN(quat.x) || float.IsNaN(quat.y) || float.IsNaN(quat.z) || float.IsNaN(quat.w))
        {
            return;
        }
        Matrix = Matrix4x4.TRS(GetTranslation(), quat, GetScale());
    }

    override public Quaternion GetRotation()
    {
        return Matrix4x4Util.GetRotation(Matrix);
    }

    override public void SetScale(Vector3 scale)
    {
//        UnityEditor.Undo.RecordObject(this, "SetScale");
        if (float.IsNaN(scale.x) || float.IsNaN(scale.y) || float.IsNaN(scale.z))
        {
            return;
        }
        Matrix = Matrix4x4.TRS(GetTranslation(), GetRotation(), scale);
    }

    override public Vector3 GetScale()
    {
        return Matrix4x4Util.GetScale(Matrix);
    }

    override public void ResetSceneTools()
    {
        PivotOffset = Vector3.zero;
        Matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        if (Input != null)
        {
            NPVoxBoneModel model = ((NPVoxIModelFactory)Input).GetProduct() as NPVoxBoneModel;
            RecenterBonePivot(model);
        }
    }

    override public Vector3 GetPivot()
    {
        return GetAffectedBox().SaveCenter + PivotOffset;
    }

    override public void SetPivot(Vector3 pivot)
    {
        PivotOffset = pivot - GetAffectedBox().SaveCenter;
    }
      
    // ===================================================================================================
    // Affeted Area
    // ===================================================================================================
    private NPVoxBox lastAffectedArea = null;
    private uint lastAffectedAreaBoneMask = 0;
    private uint lastAffectedAreaSourceModelVersion;
    private NPVoxModel lastAffectedAreaSourceModel = null;

    public NPVoxBox GetAffectedBox()
    {
        if (Input == null)
        {
            Debug.Log("Input was NULL");
            return NPVoxBox.INVALID;
        }

        NPVoxBoneModel affectedAreaSourceModel = (Input as NPVoxIModelFactory).GetProduct() as NPVoxBoneModel;
        if (affectedAreaSourceModel == null)
        {
            Debug.Log("Input did not procue a bone model");
            return NPVoxBox.INVALID;
        }

        if (affectedAreaSourceModel == this.lastAffectedAreaSourceModel && this.lastAffectedAreaSourceModelVersion == affectedAreaSourceModel.GetVersion() && lastAffectedAreaBoneMask == this.boneMask)
        {
            return lastAffectedArea;
        }

        NPVoxBox affectedArea = affectedAreaSourceModel.GetAffectedArea(boneMask);

        this.lastAffectedArea = affectedArea;
        this.lastAffectedAreaSourceModelVersion = affectedAreaSourceModel.GetVersion();
        this.lastAffectedAreaSourceModel = affectedAreaSourceModel;
        this.lastAffectedAreaBoneMask = this.boneMask;

        if (lastAffectedArea == null)
        {
            lastAffectedArea = NPVoxBox.INVALID;
        }

//        Debug.Log("Recalculate Affected Area");

        return lastAffectedArea;
    }

      
    // ===================================================================================================
    // Name
    // ===================================================================================================
    protected void RegenerateName(NPVoxBoneModel model)
    {
        if (regenerateName)
        {
            regenerateName = false;
            NPVoxBone[] bones = NPVoxBone.GetRootBones(ref model.AllBones, NPVoxBone.GetBonesInMask(ref model.AllBones, boneMask) );
            string name = "";
            foreach (NPVoxBone bone in bones)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name += ", ";
                }
                name += bone.Name;
            }
            this.InstanceName = name;
        }
    }
}