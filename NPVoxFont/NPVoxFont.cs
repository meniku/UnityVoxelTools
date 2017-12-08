using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxFont : ScriptableObject, NPipeIImportable
{
    [System.Serializable]
    public struct Character
    {
        public NPVoxCoord Size;
        public Vector3 VoxelSize;
        public Mesh mesh;
        public Material material;
    }

    public GameObject CharacterPrefab;

    public Character[] Characters;

    public string FontFolder;


    [UnityEngine.SerializeField]
    private bool isValid = false;

    public Character GetCharacter(char c)
    {
        #if UNITY_EDITOR
        Import();
        #endif
        return Characters[(int)c];
    }

    public void Import()
    {
        if (!isValid)
        {
            UpdateCharacters();
            isValid = true;
        }
    }

    public void Invalidate(bool includeSources = false)
    {
        isValid = true;
    }

    public bool IsValid()
    {
        return isValid;
    }

    public bool IsTemplate()
    {
        return false;
    }

    public void Destroy()
    {
        Characters = new Character[]{ };
    }

    public string GetTypeName()
    {
        return "NPVox Font";
    }

    public string GetInstanceName()
    {
        return name;
    }

    public UnityEngine.Object Clone()
    {
        return Instantiate(this);
    }

    #if UNITY_EDITOR
    public double GetLastInvalidatedTime()
    {
        return 0.0f;
    }

    [UnityEditor.MenuItem("Assets/Create/NPVox/Font", false)]
    static void CreateNPVoxFont()
    {
        var path = NPipelineUtils.GetCreateScriptableObjectAssetPath<NPVoxFont>();
        if (path.Length != 0)
        {
            NPVoxFont npVoxFont = (NPVoxFont)NPipeContainer.CreateInstance(typeof(NPVoxFont));
            UnityEditor.AssetDatabase.CreateAsset(npVoxFont, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.Selection.objects = new Object[] { npVoxFont };
        }
    }

    private void UpdateCharacters() 
    {
        if (Characters.Length != 128)
        {
            Characters = new Character[128];
        }
        for (int i = 0; i < 128; i++)
        {
            Character character = Characters[i];
            Object[] allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(FontFolder + "/" + i + ".asset");
            NPVoxCubeSimplifier[] simplifier = NPipelineUtils.GetByType<NPVoxCubeSimplifier>(allAssets);
            if (simplifier.Length == 1)
            {
//                Debug.Log("found character: " + simplifier[0]);
                character.material = simplifier[0].GetAtlasMaterial();
                character.mesh = simplifier[0].GetProduct();
                if (simplifier[0].InputMeshFactory)
                {

                    character.VoxelSize = simplifier[0].InputMeshFactory.VoxelSize;
                    character.Size = simplifier[0].InputMeshFactory.GetVoxModel().Size;
                }
            }
            else
            {
                character.mesh = null;
                character.material = null;
            }

            Characters[i] = character;
        }
    }
    #endif
}
