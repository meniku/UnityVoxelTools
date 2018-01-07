using UnityEngine;
using System.Collections.Generic;

[NPipeAppendableAttribute("Mesh Output", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxMeshOutput : NPVoxCompositeProcessorBase<NPVoxIModelFactory, Mesh>, NPVoxIMeshFactory, NPipeIInstantiable
{
    public Vector3 VoxelSize = Vector3.one * 0.125f;
    public Vector3 NormalVariance = Vector3.zero;
    public int NormalVarianceSeed = -1;
    public NPVoxOptimization Optimization = NPVoxOptimization.PER_FACE;
    public NPVoxNormalMode NormalMode = NPVoxNormalMode.SMOOTH;
    public int BloodColorIndex = 0;
    public NPVoxFaces Loop = new NPVoxFaces();
    public NPVoxFaces Cutout = new NPVoxFaces();
    public NPVoxFaces Include = new NPVoxFaces(1, 1, 1, 1, 1, 1);
    public int MinVertexGroups = 1;
    public NPVoxNormalMode[] NormalModePerVoxelGroup = null;
    public NPVoxNormalProcessorList NormalProcessors = null;

    public void OnEnable()
    {
        if (NormalVarianceSeed == -1)
        {
            NormalVarianceSeed = Random.Range(0, int.MaxValue);
        }
        
        if (NormalProcessors == null)
        {
            NormalProcessors = ScriptableObject.CreateInstance<NPVoxNormalProcessorList>();
        }
    }

    public void OnDisable()
    {
        
    }

    override protected Mesh CreateProduct(Mesh reuse = null)
    {
        Mesh mesh = reuse != null ? reuse : new Mesh();
        if (Input == null)
        {
            mesh.Clear();
            // Debug.Log("Source is null for " + this);
            return mesh;
        }

        NPVoxModel model = GetVoxModel();
        if (model)
        {
            mesh.Clear();
            if( !model.IsValid)
            {
                Debug.LogWarning("Source Model is not valid");
                return mesh;
            }
            NPVoxMeshGenerator.CreateMesh(model, mesh, VoxelSize, NormalVariance, NormalVarianceSeed, Optimization, NormalMode, BloodColorIndex, Loop, Cutout, Include, MinVertexGroups, NormalModePerVoxelGroup, NormalProcessors);
            mesh.name = "zzz Mesh";
            return mesh;
        }
        else
        {
            // Debug.Log("Source Product is null for " + this);
            mesh.Clear();
            return mesh;
        }
    }

    public GameObject Instatiate()
    {
#if UNITY_EDITOR
        string gameobjectTemplatePath = UnityEditor.AssetDatabase.GUIDToAssetPath(NPVoxConstants.GAMEPOBJECT_TEMPLATE);
        if (gameobjectTemplatePath == null)
        {
            UnityEngine.Debug.LogWarning(
                "NPVox: Could not find the Gameobject Template with GUID '" + NPVoxConstants.GAMEPOBJECT_TEMPLATE +
                "', if you removed the Asset, please create a new Asset and set it's GUID in Preferences -> NPVox"
            );
            return null;
        }
        GameObject templatePrefab = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(gameobjectTemplatePath, typeof(GameObject));
        GameObject go = (GameObject)GameObject.Instantiate(templatePrefab);
        NPVoxMeshInstance instance = go.GetComponent<NPVoxMeshInstance>();
        if (!instance)
        {
            go.AddComponent<NPVoxMeshInstance>();
        }
        instance.MeshFactory = this;

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        Object mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
        if( mainAsset )
        {
            instance.name = mainAsset.name;
        }
        if ((instance.MeshFactory is NPVoxProcessorBase<Mesh>) && ((NPVoxProcessorBase<Mesh>)instance.MeshFactory).StorageMode == NPipeStorageMode.ATTACHED)
        {
            instance.SharedMash = instance.MeshFactory.GetProduct();
        }

        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Created a new NPVoxInstance");

        UnityEditor.Selection.objects = new Object[]{
            instance.gameObject
        };

        return instance.gameObject;
#else
        Debug.Log("Not yet supported");
        return null;
#endif
    }

    public NPVoxModel GetVoxModel()
    {
        return Input != null ? ((NPVoxIModelFactory)Input).GetProduct() : null;
    }

    public NPVoxToUnity GetNPVoxToUnity()
    {
        return new NPVoxToUnity(GetVoxModel(), VoxelSize);
    }
    
    public NPVoxCoord GetOutputSize()
    {
        NPVoxModel model = GetVoxModel();
        NPVoxCoord size = model.Size;
        if (Cutout != null)
        {
            size.X = (sbyte)( size.X - (sbyte)Mathf.Abs(Cutout.Left) - (sbyte)Mathf.Abs(Cutout.Right) );
            size.Y = (sbyte)( size.Y - (sbyte)Mathf.Abs(Cutout.Up) - (sbyte)Mathf.Abs(Cutout.Down) );
            size.Z = (sbyte)( size.Z - (sbyte)Mathf.Abs(Cutout.Back) - (sbyte)Mathf.Abs(Cutout.Forward) );
        }
        return size;
    }

    override public string GetTypeName()
    {
        return "Mesh Output";
    }


    virtual public UnityEngine.Object Clone()
    {
        NPVoxMeshOutput copy = (NPVoxMeshOutput)base.Clone();
        copy.NormalVarianceSeed =  Random.Range(0, int.MaxValue);
        
        copy.NormalProcessors = NormalProcessors.Clone() as NPVoxNormalProcessorList;
        copy.NormalProcessors.RequiresMigration = false;

        return copy;
    }


    public override void IncludeSubAssets(string path)
    {
        NormalProcessors.AddToAsset(path);
        NormalProcessors.RequiresMigration = false;
    }

    public override void Import()
    {
        base.Import();

        // Normal processor migration code
        if ( NormalProcessors && NormalProcessors.RequiresMigration )
        {
            NormalProcessors.hideFlags = HideFlags.HideInHierarchy;

            if ( NormalModePerVoxelGroup != null && NormalModePerVoxelGroup.Length > 0 )
            {
                for ( int i = 0; i < NormalModePerVoxelGroup.Length; i++ )
                {
                    NPVoxNormalProcessor_Voxel processorVoxel = NormalProcessors.AddProcessor<NPVoxNormalProcessor_Voxel>();
                    processorVoxel.NormalMode = NormalModePerVoxelGroup[ i ];
                    processorVoxel.AddVoxelGroupFilter( i );
                }
            }
            else
            {
                NPVoxNormalProcessor_Voxel processorVoxel = NormalProcessors.AddProcessor<NPVoxNormalProcessor_Voxel>();
                processorVoxel.NormalMode = NormalMode;
            }

            NPVoxNormalProcessor_Variance processorVariance = NormalProcessors.AddProcessor<NPVoxNormalProcessor_Variance>();
            processorVariance.NormalVarianceSeed = NormalVarianceSeed;
            processorVariance.NormalVariance = NormalVariance;

            NormalProcessors.AddToAsset( UnityEditor.AssetDatabase.GetAssetPath( this ) );

            UnityEditor.EditorUtility.SetDirty( NormalProcessors );

            NormalProcessors.RequiresMigration = false;
        }
    }
}