using UnityEngine;
using System.Collections.Generic;

[NPipeAppendableAttribute("Model Transformer", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelTransformer : NPVoxModelTransformerBase
{
    [HideInInspector]
    public NPVoxBox AffectedArea = new NPVoxBox(NPVoxCoord.ZERO, NPVoxCoord.ZERO);

    [HideInInspector]
    public Matrix4x4 Matrix = Matrix4x4.identity;

    [HideInInspector]
    public Vector3 PivotOffset = Vector3.zero; // offset from AffectedArea.SaveCenter

    [System.Obsolete, HideInInspector]
    public bool TryToResolveConflicts = true;

    [HideInInspector]
    public NPVoxModelTransformationUtil.ResolveConflictMethodType ResolveConflictMethod = NPVoxModelTransformationUtil.ResolveConflictMethodType.FILL_GAPS;

    [SerializeField, HideInInspector]
    protected NPVoxBox lastParentModelBounds = NPVoxBox.INVALID;

    override public string GetTypeName()
    {
        return "Model Transformer";
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct();

        // backwards compatibility
        if (TryToResolveConflicts == false)
        {
            TryToResolveConflicts = true;
            ResolveConflictMethod = NPVoxModelTransformationUtil.ResolveConflictMethodType.NONE;
        }

        // shift afftected area if the parent bounding box changed
        {
            NPVoxBox parentBounds = model.BoundingBox;
            if (!lastParentModelBounds.Equals(NPVoxBox.INVALID) && !lastParentModelBounds.Equals(parentBounds))
            {
                sbyte deltaX = (sbyte)((parentBounds.Right - lastParentModelBounds.Right) / 2);
                sbyte deltaY = (sbyte)((parentBounds.Up - lastParentModelBounds.Up) / 2);
                sbyte deltaZ = (sbyte)((parentBounds.Forward - lastParentModelBounds.Forward) / 2);
                NPVoxCoord delta = new NPVoxCoord(deltaX, deltaY, deltaZ);
                AffectedArea = new NPVoxBox(AffectedArea.LeftDownBack + delta, AffectedArea.RightUpForward + delta);
                // Debug.Log("Moving affected area by + " + deltaX + " " + deltaY + " " + deltaZ);
            }
            lastParentModelBounds = parentBounds;
        }

        return NPVoxModelTransformationUtil.MatrixTransform(model, AffectedArea, Matrix, PivotOffset, ResolveConflictMethod, reuse);
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

    public void MaximizeAffectedArea()
    {
        // affected area picker
        NPVoxIModelFactory modelFactory = Input as NPVoxIModelFactory;
        if (modelFactory != null)
        {
            ChangeAffectedArea(modelFactory.GetProduct().BoundingBox);
        }
    }

    public void ChangeAffectedArea(NPVoxBox newBox)
    {
        this.AffectedArea = newBox;
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
        Matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        PivotOffset = Vector3.zero;
    }

    override public Vector3 GetPivot()
    {
        return AffectedArea.SaveCenter + PivotOffset;
    }

    override public void SetPivot(Vector3 pivot)
    {
        PivotOffset = pivot - AffectedArea.SaveCenter;
    }

}