using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxBoneModel : NPVoxModel 
{
    public NPVoxBone RootBone;
    public NPVoxBone[] AllBones;

    [SerializeField]
    public uint[] boneMasks = null;

    public new static NPVoxBoneModel NewInstance(NPVoxCoord size, NPVoxModel reuse = null)
    {
        NPVoxBoneModel VoxModel = reuse is NPVoxBoneModel ? reuse as NPVoxBoneModel : ScriptableObject.CreateInstance<NPVoxBoneModel>();
        VoxModel.name = "BoneModel";
        VoxModel.Initialize(size);
        return VoxModel;
    }

    public override void Initialize(NPVoxCoord size)
    {
        base.Initialize(size);
        boneMasks = new uint[size.X * size.Y * size.Z];
        RootBone = new NPVoxBone("Root", 0, null);
        AllBones = new NPVoxBone[]{ };
    }

    public override void CopyOver(NPVoxModel source)
    {
        base.CopyOver(source);
        if (source is NPVoxBoneModel)
        {
            NPVoxBoneModel boneModel = (source as NPVoxBoneModel);
            boneMasks = (uint[]) boneModel.boneMasks.Clone();
            this.AllBones = NPVoxBone.CloneBones(boneModel.AllBones);
        }
    }

    public NPVoxBone GetBoneByID(uint id)
    {
        if (id == 0)
        {
            return RootBone;
        }
        return AllBones[id-1];
    }

    public void SetBoneMask(NPVoxCoord coord, uint mask)
    {
        if (IsInside(coord))
        {
            boneMasks[GetIndex(coord)] = mask;
        }
    }

    public void AddBoneMask(NPVoxCoord coord, uint mask)
    {
        if (IsInside(coord))
        {
            boneMasks[GetIndex(coord)] |= mask;
        }
    }

    public uint GetBoneMask(NPVoxCoord coord)
    {
        if(HasVoxel(coord))
        {
            return boneMasks[GetIndex(coord)];
        }
        return 0;
    }

    public bool IsInBoneMask(NPVoxCoord coord, uint mask)
    {
        if (HasVoxel(coord))
        {
            return (boneMasks[GetIndex(coord)] & mask) != 0 || mask == 0;
        }
        return false;
    }

    public void SetVoxel(NPVoxCoord coord, byte color, uint mask)
    {
        SetVoxel(coord, color);
        SetBoneMask(coord, mask);
    }

    public NPVoxBox GetAffectedArea(uint boneMask)
    {
        NPVoxBox affectedArea = null;

        foreach (NPVoxCoord coord in Enumerate())
        {
            if (IsInBoneMask(coord, boneMask))
            {
                if (affectedArea == null)
                {
                    affectedArea = new NPVoxBox(coord, coord);
                }
                else
                {
                    affectedArea.EnlargeToInclude(coord);
                }
            }
        }

        if (affectedArea == null)
        {
            Debug.Log("Bone Mask did not produce any valid affected area");
            affectedArea = NPVoxBox.INVALID;
        }

        return affectedArea;
    }
}