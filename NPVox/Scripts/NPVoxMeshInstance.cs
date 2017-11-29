using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPVoxMeshInstance : MonoBehaviour
{
    [NPipeSelectorAttribute(typeof(NPVoxIMeshFactory))]
    public UnityEngine.Object meshFactory;

    public NPVoxIMeshFactory MeshFactory
    {
        get
        {
            return meshFactory as NPVoxIMeshFactory;
        }
        set
        {
            meshFactory = (UnityEngine.Object)value;
        }
    }

    public void UpdateMesh()
    {
        this.SharedMash = MeshFactory.GetProduct();
    }

    public Mesh SharedMash
    {
        get
        {
            MeshFilter meshFilter = this.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogWarning("NPVox: Could not find MeshFilter, Mesh was not set");
            }
            return meshFilter.sharedMesh;
        }
        set
        {
            MeshFilter meshFilter = this.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogWarning("NPVox: Could not find MeshFilter, Mesh was not set");
            }
            meshFilter.sharedMesh = value;
        }
    }
    public Vector3 VoxelSize
    {
        get
        {
            if (MeshFactory is NPVoxMeshOutput)
            {
                return ((NPVoxMeshOutput)MeshFactory).VoxelSize;
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

    public Mesh Mesh
    {
        get
        {
            return MeshFactory != null ? MeshFactory.GetProduct() : null;
        }
    }

    public void Align(Transform transform = null)
    {
        if (transform == null)
        {
            transform = this.transform;
        }
        if (MeshFactory == null)
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


    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
//        if (Selection.activeGameObject != this.gameObject)
//        {
//            return;
//        }

        NPVoxMeshOutput MeshOutput = MeshFactory as NPVoxMeshOutput;

        if (MeshOutput)
        {
            NPVoxToUnity npVoxToUnity = MeshOutput.GetNPVoxToUnity();
            NPVoxModel model = MeshOutput.GetVoxModel();
            if (model)
            {
                foreach (NPVoxSocket socket in model.Sockets)
                {
                    Vector3 anchorPos = npVoxToUnity.ToUnityPosition(socket.Anchor);
                    Quaternion rotation = Quaternion.Euler(socket.EulerAngles);
                    Vector3 anchorRight = npVoxToUnity.ToUnityDirection(rotation * Vector3.right);
                    Vector3 anchorUp = npVoxToUnity.ToUnityDirection(rotation  * Vector3.up);
                    Vector3 anchorForward = npVoxToUnity.ToUnityDirection(rotation  * Vector3.forward);

                    Gizmos.color = new Color(0.5f, 1.0f, 0.1f, 0.75f);
                    Gizmos.DrawCube(transform.position + anchorPos, Vector3.one * 0.4f);

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position + anchorPos, transform.position + anchorPos + anchorRight * 10.0f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position + anchorPos, transform.position + anchorPos + anchorUp * 10.0f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position + anchorPos, transform.position + anchorPos + anchorForward * 10.0f);
                }
            }
        }
    }
    #endif
}
