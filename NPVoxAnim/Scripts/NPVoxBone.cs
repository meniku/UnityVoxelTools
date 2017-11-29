using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPVoxBone
{
    public int ID;
    public uint Mask;
    public int ParentID;
    public Vector3 Anchor;
    public Vector3 EulerAngles;
    public string Name;

    public NPVoxBone(string name, int id, NPVoxBone parent)
    {
        Name = name;
        ID = id;
        if (parent != null)
        {
            ParentID = parent.ID;
            Mask = parent.Mask | (1u << (id-1));
        }
        else
        {
            Mask = ID != 0 ? (1u << (id-1)) : 0;
            ParentID = 0;
        }
    }

    public NPVoxBone[] GetChildren(NPVoxBone[] allBones)
    {
        int numBones = 0;
        foreach (NPVoxBone bone in allBones)
        {
            if (bone != null && bone.ID > 0 && bone.ParentID == this.ID)
            {
                numBones++;
            }
        }

        NPVoxBone[] bones = new NPVoxBone[numBones];
        numBones = 0;
        foreach (NPVoxBone bone in allBones)
        {
            if (bone != null && bone.ID > 0 && bone.ParentID == this.ID)
            {
                bones[numBones++] = bone;
            }
        }

        return bones;
    }

    public NPVoxBone[] GetDescendants(NPVoxBone[] allBones, List<NPVoxBone> _internalAppend = null)
    {
        bool isCalled = false;
        if (_internalAppend == null)
        {
            _internalAppend = new List<NPVoxBone>();
            isCalled = true;
        }
        foreach (NPVoxBone bone in allBones)
        {
            if (bone != null && bone.ID > 0 && bone.ParentID == this.ID)
            {
                _internalAppend.Add(bone);
                bone.GetDescendants(allBones, _internalAppend);
            }
        }

        if (isCalled)
        {
            return _internalAppend.ToArray();
        }
        else
        {
            return null;
        }
    }


    public static NPVoxBone[] CloneBones(NPVoxBone[] allBones)
    {
        NPVoxBone[] bones = new NPVoxBone[allBones.Length];
        for (int i = 0; i < allBones.Length; i++)
        {
            if (allBones[i] != null && allBones[i].ID > 0)
            {
                NPVoxBone sourceBone = allBones[i];
                UnityEngine.Assertions.Assert.AreEqual(i, sourceBone.ID - 1);
                bones[i] = new NPVoxBone(sourceBone.Name, sourceBone.ID, sourceBone.ParentID > 0 ? bones[sourceBone.ParentID - 1] : null);
                bones[i].Anchor = sourceBone.Anchor;
                bones[i].EulerAngles = sourceBone.EulerAngles;
            }
        }

        return bones;
    }
    #if UNITY_EDITOR
    public static void DeleteBone(ref NPVoxBone[] allBones, NPVoxBone bone)
    {
        allBones[bone.ID - 1].ID = -1;
    }

    #endif

    public static int GetNextBoneID(NPVoxBone[] allBones, int parentID)
    {
        for (int i = parentID; i < 31; i++)
        {
            if(allBones.Length <= i || allBones[i] == null || allBones[i].ID < 1)
            {
                return i + 1;
            }
        }
        return -1;
    }

    #if UNITY_EDITOR
    public static NPVoxBone AddBone(ref NPVoxBone[] allBones, NPVoxBone parent)
    {
        int nextBoneID = GetNextBoneID(allBones, parent.ID);
        if (nextBoneID < 0)
        {
            return null;
        }
        NPVoxBone newBone = new NPVoxBone("new", nextBoneID, parent);
        if (allBones.Length > nextBoneID - 1)
        {
            allBones[nextBoneID - 1] = newBone;
        }
        else
        {
            UnityEditor.ArrayUtility.Add(ref allBones, newBone);
        }
        return newBone;
    }
    #endif
    public static NPVoxBone[] GetBonesInMask(ref NPVoxBone[] allBones, uint boneMask)
    {
        List<NPVoxBone> bones = new List<NPVoxBone>();
        foreach(NPVoxBone bone in allBones)
        {
            if (((1u << (bone.ID-1)) & boneMask) != 0)
            {
                bones.Add(bone);
            }
        }
        return bones.ToArray();
    }

    public static NPVoxBone[] GetRootBones(ref NPVoxBone[] allBones, NPVoxBone[] bones)
    {
        List<NPVoxBone> rootBones = new List<NPVoxBone>();
        foreach(NPVoxBone bone in bones)
        {
            bool parentFound = false;
            foreach(NPVoxBone parentBone in bones)
            {
                if (parentBone != bone && IsDescendant(ref allBones, parentBone, bone))
                {
                    parentFound = true;
                }
            }

            if (!parentFound)
            {
                rootBones.Add(bone);
            }
        }
        return rootBones.ToArray();
    }

    public static bool IsDescendant(ref NPVoxBone[] allBones, NPVoxBone parent, NPVoxBone descendant)
    {
        if (descendant.ParentID == parent.ID)
        {
            return true;
        }
        else if (descendant.ParentID == 0)
        {
            return false;
        }
        else
        {
            return IsDescendant(ref allBones, parent, GetBoneByID(ref allBones, descendant.ParentID));
        }
    }

    public static NPVoxBone GetBoneByID(ref NPVoxBone[] allBones, int id)
    {
        for (int i = 0; i < allBones.Length; i++)
        {
            if (allBones[i].ID == id)
            {
                return allBones[i];
            }
        }
        return null;
    }

    public static uint GetMaskWithDescendants(ref NPVoxBone[] allBones, uint mask)
    {
        uint completeMask = mask;
        for(int i = 0; i < 32; i++)
        {
            int id = i + 1;
            if (((mask >> i) & 0x1) != 0)
            {
                NPVoxBone bone = GetBoneByID(ref allBones, id);
                if (bone != null)
                {
                    NPVoxBone[] descendants = bone.GetDescendants(allBones);
                    foreach (NPVoxBone descendant in descendants)
                    {
                        completeMask |= (1u << (descendant.ID - 1));
                    }
                }
            }
        }

        return completeMask;
    }
}