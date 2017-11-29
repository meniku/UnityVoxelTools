using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

[NPipeStartingAttribute("Magicka Source", true)]
public class NPVoxMagickaSource : NPVoxProcessorBase<NPVoxModel>, NPVoxIModelFactory
{
    [HideInInspector]
    public string VoxModelUUID;

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
#if !UNITY_EDITOR
            Debug.LogWarning("Cannot create Voxel Models during runtime right now");
            // NOTE: To make that compatible we would have to make the .vox-file available at runtime. 
            //       It is waaaayyyyy more feasable to just set the source to be stored as RESOURCE right now.
            return null;
#else        
        string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(VoxModelUUID);
        if( string.IsNullOrEmpty( this.InstanceName ))
        {
            this.InstanceName = assetPath.Substring(assetPath.LastIndexOf("/") + 1);
        }

        if (!File.Exists(assetPath))
        {
            Debug.LogWarning("Could not find file for UUID '" + VoxModelUUID + "' in " + AssetDatabase.GetAssetPath(this) + "");
            return NPVoxModel.NewInvalidInstance(reuse, "The source file didn't exist");
        }

        FileStream fs = File.OpenRead(assetPath);
        BinaryReader reader = new BinaryReader(fs);
        NPVoxModel voxModel = NPVoxReader.Read(reader, reuse);
        fs.Close();
        if (voxModel)
        {
            voxModel.name = "zzz Magicka Model";
            return voxModel;
        }
        else
        {
            return NPVoxModel.NewInvalidInstance(reuse, "Voxmodel could not be parsed");
        }
#endif
    }

#if UNITY_EDITOR
    public override bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags);
        string voxSourcePath = AssetDatabase.GUIDToAssetPath(VoxModelUUID);

        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(voxSourcePath, typeof(UnityEngine.Object));
        UnityEngine.Object obj2 = EditorGUILayout.ObjectField("Magicka .vox file", obj, typeof(Object), false);
        if (obj != obj2)
        {
            VoxModelUUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj2));
            EditorUtility.SetDirty(this);
            changed = true;
        }
        return changed;
    }
#endif

    override public string GetTypeName()
    {
        return "Magicka Source";
    }
}