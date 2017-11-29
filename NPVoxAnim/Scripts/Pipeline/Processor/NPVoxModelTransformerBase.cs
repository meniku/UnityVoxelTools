using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

abstract public class NPVoxModelTransformerBase : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxITransformable, NPVoxIModelFactory, NPVoxISceneEditable
{
    #if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags & ~NPipeEditFlags.INPUT);
        changed |= DrawTransformationMatrix();
        return changed;
    }

    private bool DrawTransformationMatrix()
    {
        bool changed = false;
        GUILayout.BeginVertical();
        Vector3 oldTranslation = GetTranslation();
        Vector3 newTranslation = UnityEditor.EditorGUILayout.Vector3Field("Translation", oldTranslation);
        if (/*!GUI.changed &&*/ !newTranslation.Equals(oldTranslation))
        {
            changed = true;
            UnityEditor.Undo.RecordObject(this, "SetTranslation");
            SetTranslation(newTranslation);
        }

        Vector3 oldRotation = GetRotation().eulerAngles;
        Vector3 newRotation = UnityEditor.EditorGUILayout.Vector3Field("Rotation", oldRotation);
        if (/*!GUI.changed && */!newRotation.Equals(oldRotation))
        {
            changed = true;
            UnityEditor.Undo.RecordObject(this, "SetRotation");
            SetRotation(Quaternion.Euler(newRotation));
        }

        Vector3 oldScale = GetScale();
        Vector3 newScale = UnityEditor.EditorGUILayout.Vector3Field("Scale", oldScale);
        if (/*!GUI.changed && */ !newScale.Equals(oldScale))
        {
            changed = true;
            UnityEditor.Undo.RecordObject(this, "SetScale");
            SetScale(newScale);
        }

        GUILayout.EndVertical();
        return changed;
    }

    public string[] GetSceneEditingTools()
    {
        return new string[] { "Move", "Rotate", "Scale", "Pivot" };
    }

    public System.Func<NPVoxISceneEditable, bool> DrawSceneTool(NPVoxToUnity npVoxToUnity, Transform transform, int tool)
    {
        if (Input == null)
        {
            return null;
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct() as NPVoxModel;
        if (!model)
        {
            return null;
        }

        Vector3 pivot = npVoxToUnity.ToUnityPosition(GetPivot());

        // Position handle
        if (tool == 0)
        {
            Vector3 oldTranslation = GetTranslation();
            Vector3 newTranslation = npVoxToUnity.ToSaveVoxDirection(
                Handles.PositionHandle(npVoxToUnity.ToUnityDirection(oldTranslation) + pivot, Quaternion.identity) - pivot
            );
            if (!newTranslation.Equals(oldTranslation))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxModelTransformerBase).SetTranslation(newTranslation);
                    return true;
                };
            }
        }

        // Quaternion Handle
        if (tool == 1)
        {
            Quaternion oldQuaternion = GetRotation();
            Quaternion newQuaternion = Handles.RotationHandle(oldQuaternion, pivot);
            if (!newQuaternion.Equals(oldQuaternion))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxModelTransformerBase).SetRotation(newQuaternion);
                    return true;
                };
            }
        }

        // Scale Handle
        if (tool == 2)
        {
            Vector3 oldScale = GetScale();
            Vector3 newScale = Handles.ScaleHandle(oldScale, pivot, GetRotation(), 1.0f);
            if (!newScale.Equals(oldScale))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxModelTransformerBase).SetScale(newScale);
                    return true;
                };
            }
        }

        // Pivot Handle
        if (tool == 3)
        {
            Vector3 newPivot = Handles.PositionHandle(pivot, Quaternion.identity);
            if (!newPivot.Equals(pivot))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxModelTransformerBase).SetPivot(newPivot);
                    return true;
                };
            }
        }
        return null;
    }

    #endif


    public abstract void SetTranslation(Vector3 translation);
    public abstract Vector3 GetTranslation();
    public abstract void SetRotation(Quaternion quat);
    public abstract Quaternion GetRotation();
    public abstract void SetScale(Vector3 scale);
    public abstract Vector3 GetScale();
    public abstract void ResetSceneTools();
    public abstract Vector3 GetPivot();
    public abstract void SetPivot(Vector3 pivot);
}
