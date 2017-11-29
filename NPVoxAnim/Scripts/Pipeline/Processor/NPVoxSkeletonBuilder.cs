using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[NPipeAppendableAttribute("Skeleton Builder", typeof(NPVoxIModelFactory), false, false)]
public class NPVoxSkeletonBuilder : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory, NPVoxISceneEditable
{
    [HideInInspector]
    public NPVoxBone RootBone = new NPVoxBone("Root", 0, null);

    [HideInInspector]
    public NPVoxBone[] AllBones = new NPVoxBone[]{ };

    [System.Serializable]
    public class BoxArray
    {
        public List<NPVoxBox> Boxes;
    }

    [HideInInspector, SerializeField]
    public BoxArray[] AllBoxes = new BoxArray[32];

    override public string GetTypeName()
    {
        return "Skeleton Builder";
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct();

        if (model is NPVoxBoneModel)
        {
            Debug.LogError("cannot create bone model on top of another bone model");
            return model;
        }

        NPVoxBoneModel newModel = NPVoxBoneModel.NewInstance(model.Size, reuse as NPVoxBoneModel);
        newModel.CopyOver(model);

        // setup bone masks
        newModel.AllBones = NPVoxBone.CloneBones(AllBones);
        for (int i = 0; i < AllBones.Length; i++)
        {
            NPVoxBone bone = AllBones[i];
            List<NPVoxBox> boxes = AllBoxes[i].Boxes;
            if (boxes != null)
            {
                foreach (NPVoxBox box in boxes)
                {
                    foreach (NPVoxCoord coord in box.Enumerate())
                    {
                        newModel.AddBoneMask(coord, 1u << (bone.ID - 1));
                    }
                }
            }
        }

        return newModel;
    }

    #if UNITY_EDITOR

    public NPVoxBone AddBone(NPVoxBone parent)
    {
        Undo.RecordObject(this, "Add Bone");
        NPVoxBone child = NPVoxBone.AddBone(ref AllBones, parent);
        if (child != null)
        {
            child.Name = parent.Name;
            if (Input != null)
            {
                NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct();
                child.Anchor = model.BoundingBox.SaveCenter;
            }
            return child;
        }
        return null;
    }

    public void DeleteBone(NPVoxBone bone)
    {
        Undo.RecordObject(this, "Delete Bone");
        NPVoxBone.DeleteBone(ref AllBones, bone);
    }

    public NPVoxBox AddBox(NPVoxBone bone)
    {
        Undo.RecordObject(this, "Add Box");
        List<NPVoxBox> boxes = GetBoxes(bone);
        boxes.Add(new NPVoxBox( NPVoxCoordUtil.ToCoord(bone.Anchor), NPVoxCoordUtil.ToCoord(bone.Anchor) + NPVoxCoord.ONE ));
        return boxes[boxes.Count - 1];
    }

    public void DeleteBox(NPVoxBone bone, NPVoxBox box)
    {
        Undo.RecordObject(this, "Delete Box");
        List<NPVoxBox> boxes = GetBoxes(bone);
        boxes.Remove(box);
    }

    public List<NPVoxBox> GetBoxes(NPVoxBone bone)
    {
        if (bone.ID == 0)
        {
            return new List<NPVoxBox>();
        }
            
        List<NPVoxBox> boxes = this.AllBoxes[bone.ID - 1].Boxes;
        if (boxes == null)
        {
            boxes = this.AllBoxes[bone.ID - 1].Boxes = new List<NPVoxBox>();
        }
        return boxes;
    }


    [System.NonSerialized]
    public NPVoxBox CurrentEditedBox = null;

    [System.NonSerialized]
    public NPVoxBone CurrentEditedBone = null;

    [System.NonSerialized]
    public NPVoxBox CurrentCopiedBox = null;

    public List<NPVoxBox> GetCurrentSelectedChildBoxes()
    {
        List<NPVoxBox> allBoxes = new List<NPVoxBox>();
        NPVoxBone[] bones = CurrentEditedBone.GetDescendants(AllBones);
        foreach(NPVoxBone bone in bones)
        {
            allBoxes.AddRange(GetBoxes(bone));
        }
        allBoxes.AddRange(GetBoxes(CurrentEditedBone));
        return allBoxes;
    }

    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags);
//        changed |= DrawTransformationMatrix();

        int currentLevel = 0;

        EditorGUILayout.BeginVertical();
        changed |= DrawBone(currentLevel, RootBone, true, 0, 0);

        EditorGUILayout.EndVertical();

        return changed;
    }

    private bool DrawBone(int currentLevel, NPVoxBone bone, bool isRoot, int numSiblings, float lastY)
    {
        EditorGUILayout.BeginHorizontal();

        NPVoxBone[] bones = bone.GetChildren(AllBones);

        if (CurrentEditedBone == bone)
        {
            if (GUILayout.Button("Close", GUILayout.Width(40)))
            {
                CurrentEditedBone = null;
                CurrentEditedBox = null;
                return true;
            }
        }
        else
        {
            if (GUILayout.Button("Edit", GUILayout.Width(40)))
            {
                CurrentEditedBone = bone;
                CurrentEditedBox = GetBoxes(bone).Count > 0 ? GetBoxes(bone)[0] : null;
                return true;
            }
        }

        GUILayout.Space(10f * currentLevel);

        Rect rect = GUILayoutUtility.GetLastRect();
        if(numSiblings > 1)
        {
            float currentX = rect.xMin + 10f * currentLevel;
            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(currentX, lastY + 18), new Vector2(currentX, rect.yMax + 10));
            Handles.DrawLine(new Vector2(currentX, rect.yMax + 10), new Vector2(currentX + 10, rect.yMax + 10));
        }

        if (bones.Length > 1)
        {
            lastY = rect.y;
        }

        EditorGUILayout.BeginVertical();
        string newName = EditorGUILayout.TextField(bone.Name);
        if (newName != bone.Name)
        {
            bone.Name = newName;
            return true;
        }

        if (CurrentEditedBone == bone)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10f);

            if (bone.ID != 0 && bones.Length == 0 && GUILayout.Button("Delete "))
            {
                DeleteBone(bone);
                CurrentEditedBox = null;
                CurrentEditedBone = null;
                return true;
            }

            if (GUILayout.Button("+ Bone"))
            {
                CurrentEditedBox = null;
                CurrentEditedBone = AddBone(bone);
                return true;
            }

            if (!isRoot)
            {
                if (GUILayout.Button("+ Box "))
                {
                    CurrentEditedBox = AddBox(bone);
                    return true;
                }

                if (CurrentCopiedBox != null && GUILayout.Button("+ Paste"))
                {
                    CurrentEditedBox = AddBox(bone);
                    CurrentEditedBox.LeftDownBack = CurrentCopiedBox.LeftDownBack;
                    CurrentEditedBox.RightUpForward = CurrentCopiedBox.RightUpForward;
                    return true;
                }

                List<NPVoxBox> boxes = GetBoxes(bone);
                int i = 0;
                foreach (NPVoxBox box in boxes)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10f);
                    EditorGUILayout.LabelField("(Box: " + i + ")", GUILayout.Width(100));

                    if (box != CurrentEditedBox)
                    {
                        if (GUILayout.Button("Edit"))
                        {
                            CurrentEditedBox = box;
                            return true;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Close"))
                        {
                            CurrentEditedBox = null;
                            return true;
                        }
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        DeleteBox(bone, box);
                        CurrentEditedBox = null;
                        return true;
                    }
                    if (GUILayout.Button("Copy"))
                    {
                        CurrentCopiedBox = box;
                        return true;
                    }
                    i++;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("(ID: " + bone.ID + ")", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        for(int i = 0; i < bones.Length; i++)
        {
            NPVoxBone child = bones[i];
            if (child != null && DrawBone(currentLevel + 1, child, false, bones.Length, lastY ))
            {
                return true;
            }
        }

        return false;
    }

    public string[] GetSceneEditingTools()
    {
        return new string[] { "Anchor Offset" };
    }

    private Vector3 currentPivot;
    private uint lastMask;

    public System.Func<NPVoxISceneEditable, bool> DrawSceneTool(NPVoxToUnity npVoxToUnity, UnityEngine.Transform transform, int tool)
    {
        // offset 

        if (CurrentEditedBone == null)
        {
            return null;
        }

        NPVoxBoneModel boneModel = GetProduct() as NPVoxBoneModel;

        if (boneModel == null)
        {
            return null;
        }

        if (lastMask != 1u << (CurrentEditedBone.ID -1 ) )
        {
            lastMask = 1u << (CurrentEditedBone.ID -1 );
            currentPivot = npVoxToUnity.ToUnityPosition( boneModel.GetAffectedArea(lastMask).SaveCenter );
        }

        Vector3 offset = npVoxToUnity.ToUnityDirection( CurrentEditedBone.Anchor );
        if (tool == 0)
        {
            offset = npVoxToUnity.ToSaveVoxDirection( Handles.PositionHandle(currentPivot + offset, Quaternion.identity) - currentPivot );
            if (offset != CurrentEditedBone.Anchor)
            {
                return (NPVoxISceneEditable t) =>
                {
                    NPVoxBone.GetBoneByID( ref ((NPVoxSkeletonBuilder)t).AllBones, CurrentEditedBone.ID ).Anchor = offset;
                    return true;
                };
            }
        }

        return null;
    }
    
    public void ResetSceneTools()
    {
        if (CurrentEditedBone != null)
        {
            CurrentEditedBone.Anchor = Vector3.zero;
        }
    }

    #endif
}