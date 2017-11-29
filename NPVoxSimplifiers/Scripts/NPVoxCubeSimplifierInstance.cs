using UnityEngine;
using System.Collections;

public class NPVoxCubeSimplifierInstance : MonoBehaviour
{
    [NPipeSelectorAttribute(typeof(NPVoxCubeSimplifier))]
    public NPVoxCubeSimplifier CubeSimplifier;


    public void UpdateMesh()
    {
        Mesh mesh = CubeSimplifier ? CubeSimplifier.GetProduct() : null;
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogWarning("NPVox: Could not find MeshFilter, Mesh was not set");
        }
        else
        {
            meshFilter.sharedMesh = mesh;
        }
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning("NPVox: Could not find MeshRenderer, Mesh was not set");
        }
        else
        {
            meshRenderer.sharedMaterial = mesh ? CubeSimplifier.GetAtlasMaterial() : null;
        }
    }

    public Vector3 VoxelSize
    {
        get
        {
            if (CubeSimplifier && CubeSimplifier.InputMeshFactory)
            {
                return CubeSimplifier.InputMeshFactory.VoxelSize;
            }
            else
            {
                return Vector3.one * 0.1f;
            }
        }
    }

    // public NPVoxModel VoxModel
    // {
    //     get
    //     {
    //         return MeshFactory ? MeshFactory.Source.GetProduct() : null;
    //     }
    // }

    // public Mesh Mesh
    // {
    //     get
    //     {
    //         return MeshFactory != null ? MeshFactory.GetProduct() : null;
    //     }
    // }

    public void Align(Transform transform = null)
    {
        UpdateMesh();
        if (transform == null)
        {
            transform = this.transform;
        }
        if (CubeSimplifier == null || CubeSimplifier.InputMeshFactory == null)
        {
            return;
        }
        transform.localPosition = new Vector3(
            Mathf.Round(transform.localPosition.x / (VoxelSize.x * 0.5f)) * VoxelSize.x * 0.5f,
            Mathf.Round(transform.localPosition.y / (VoxelSize.y * 0.5f)) * VoxelSize.y * 0.5f,
            Mathf.Round(transform.localPosition.z / (VoxelSize.z * 0.5f)) * VoxelSize.z * 0.5f
        );
        transform.localRotation = Quaternion.Euler(
            Mathf.Round(transform.localRotation.eulerAngles.x / 15f) * 15f,
            Mathf.Round(transform.localRotation.eulerAngles.y / 15f) * 15f,
            Mathf.Round(transform.localRotation.eulerAngles.z / 15f) * 15f
        );
    }
}
