using UnityEngine;
using System.Collections;

public class NPVoxToUnity
{
    private Vector3 unitySize;
    private Vector3 voxelSize;
    private Vector3 voxelOffset;
    private NPVoxModel voxModel;

    public Vector3 UnityVoxModelSize
    {
        get
        {
            return unitySize;
        }
    }

    public Vector3 VoxeSize
    {
        get
        {
            return voxelSize;
        }
    }


    public NPVoxToUnity(NPVoxCoord voxModelSize, Vector3 voxelSize)
    {
        this.voxModel = null;
        this.voxelSize = voxelSize;
        this.voxelOffset = Vector3.zero;
        this.unitySize = new Vector3(voxelSize.x * voxModelSize.X, voxelSize.y * voxModelSize.Y, voxelSize.z * voxModelSize.Z);
    }

    public NPVoxToUnity(Vector3 unityModelSize, Vector3 voxelSize)
    {
        this.voxModel = null;
        this.voxelSize = voxelSize;
        this.voxelOffset = Vector3.zero;
        this.unitySize = unityModelSize;
    }

    public NPVoxToUnity(NPVoxModel voxModel, Vector3 voxelSize)
    {
        this.voxModel = voxModel;
        this.voxelSize = voxelSize;
        this.voxelOffset = Vector3.zero;

        if (voxModel == null)
        {
            this.unitySize = Vector3.zero;
        }
        else
        {
            this.unitySize = new Vector3(voxelSize.x * voxModel.SizeX, voxelSize.y * voxModel.SizeY, voxelSize.z * voxModel.SizeZ);
        }
    }

    public NPVoxToUnity(NPVoxModel voxModel, Vector3 voxelSize, Vector3 voxelOffset)
    {
        this.voxModel = voxModel;
        this.voxelSize = voxelSize;
        this.voxelOffset = voxelOffset;

        if (voxModel == null)
        {
            this.unitySize = Vector3.zero;
        }
        else
        {
            this.unitySize = new Vector3(voxelSize.x * voxModel.SizeX, voxelSize.y * voxModel.SizeY, voxelSize.z * voxModel.SizeZ);
        }
    }

    public Vector3 ToUnityDirection(NPVoxCoord voxCoord)
    {
        return new Vector3(
            voxCoord.X * voxelSize.x,
            voxCoord.Y * voxelSize.y,
            voxCoord.Z * voxelSize.z
        );
    }

    public Vector3 ToUnityPosition(NPVoxCoord voxCoord)
    {
        return new Vector3(
            this.voxelOffset.x - unitySize.x * 0.5f + voxCoord.X * voxelSize.x + voxelSize.x * 0.5f,
            this.voxelOffset.y - unitySize.y * 0.5f + voxCoord.Y * voxelSize.y + voxelSize.y * 0.5f,
            this.voxelOffset.z - unitySize.z * 0.5f + voxCoord.Z * voxelSize.z + voxelSize.z * 0.5f
        );
    }

    public NPVoxCoord ToVoxCoord(Vector3 unityPosition)
    {
        return new NPVoxCoord(
            (sbyte)(Mathf.Round((unityPosition.x - this.voxelOffset.x + unitySize.x * 0.5f - voxelSize.x * 0.5f) / voxelSize.x)),
            (sbyte)(Mathf.Round((unityPosition.y - this.voxelOffset.y + unitySize.y * 0.5f - voxelSize.y * 0.5f) / voxelSize.y)),
            (sbyte)(Mathf.Round((unityPosition.z - this.voxelOffset.z + unitySize.z * 0.5f - voxelSize.z * 0.5f) / voxelSize.z))
        );
    }

    public NPVoxCoord ToVoxDirection(Vector3 unityDirection)
    {
        return new NPVoxCoord(
            (sbyte)(Mathf.Round((unityDirection.x) / voxelSize.x)),
            (sbyte)(Mathf.Round((unityDirection.y) / voxelSize.y)),
            (sbyte)(Mathf.Round((unityDirection.z) / voxelSize.z))
        );
    }

    public NPVoxRayCastHit Raycast(Ray ray, Transform transform, float distance = 10f)
    {
        Vector3 transformedPoint = transform != null ? transform.InverseTransformPoint(ray.origin) : ray.origin;
        Vector3 transformedDirection = transform != null ? transform.InverseTransformDirection(ray.direction) : ray.direction;
        float travelledDistance = 0f;

        // TODO walking there is really stupid ^^ find way to project the transformPoint onto the VoxModel-s boundaries as a start position
        while (travelledDistance < distance)
        {
            NPVoxCoord coord = ToVoxCoord(transformedPoint);
            if (!this.voxModel.IsInside(coord) || !this.voxModel.HasVoxel(coord))
            {
                transformedPoint += transformedDirection * voxelSize.x;
                travelledDistance += voxelSize.x;
            }
            else
            {
                return new NPVoxRayCastHit(true, coord);
            }
        }

        return new NPVoxRayCastHit(false, NPVoxCoord.INVALID);
    }

    public Vector3 ToUnityPosition(Vector3 saveVoxCoord)
    {
        return new Vector3(
            this.voxelOffset.x - unitySize.x * 0.5f + saveVoxCoord.x * voxelSize.x + voxelSize.x * 0.5f,
            this.voxelOffset.y - unitySize.y * 0.5f + saveVoxCoord.y * voxelSize.y + voxelSize.y * 0.5f,
            this.voxelOffset.z - unitySize.z * 0.5f + saveVoxCoord.z * voxelSize.z + voxelSize.z * 0.5f
        );
    }

    public Vector3 ToSaveVoxCoord(Vector3 voxelCenter)
    {
        return new Vector3(
            ((voxelCenter.x - this.voxelOffset.x + unitySize.x * 0.5f - voxelSize.x * 0.5f) / voxelSize.x),
            ((voxelCenter.y - this.voxelOffset.y + unitySize.y * 0.5f - voxelSize.y * 0.5f) / voxelSize.y),
            ((voxelCenter.z - this.voxelOffset.z + unitySize.z * 0.5f - voxelSize.z * 0.5f) / voxelSize.z)
        );
    }

    public Vector3 ToUnityDirection(Vector3 voxCoord)
    {
        return new Vector3(
            voxCoord.x * voxelSize.x,
            voxCoord.y * voxelSize.y,
            voxCoord.z * voxelSize.z
        );
    }

    public Vector3 ToSaveVoxDirection(Vector3 unityDir)
    {
        return new Vector3(
            (unityDir.x / voxelSize.x),
            (unityDir.y / voxelSize.y),
            (unityDir.z / voxelSize.z)
        );
    }
}
