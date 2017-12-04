using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections.Generic;

public class NPVoxTextureAtlas : ScriptableObject
{
    private const int fieldSize = 18;
    // private const int size = 2048;
    // private const int cols = 113;
    // private const int rows = 113;
    private const int size = 512;
    private const int cols = 28;
    private const int rows = 28;

    [SerializeField]
    private int curX = 0;
    [SerializeField]
    private int curY = 0;

    [SerializeField]
    private int token = -1;

    [SerializeField]
    private bool[] fields = new bool[cols * rows];

    [SerializeField]
    private Texture2D albedoTexture;
    [SerializeField]
    private Texture2D normalTexture;

    [System.Serializable]
    public class MaterialEntry
    {
        public Material sourceMaterial;
        public Material material;
    }

    [SerializeField]
    public bool UpdateMateral = true;

    [SerializeField]
    private List<MaterialEntry> materials = new List<MaterialEntry>();

    [System.Serializable]
    public class Slot
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public int token;

        public Vector2 GetUVmin(int border = 0)
        {
            return new Vector2(
                (float)(this.x + border) / size,
                (float)(this.y + border) / size
            );
        }

        public Vector2 GetUVmax(int border = 0)
        {
            return new Vector2(
                (float)(this.x + this.width - border) / size,
                (float)(this.y + this.height - border) / size
            );
        }
    }

    public void InitAssets()
    {
        #if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);

        // if( albedoTexture)
        // {
        //     DestroyImmediate(albedoTexture, true);
        // }

        // if( normalTexture)
        // {
        //     DestroyImmediate(normalTexture, true);
        // }

        if (!albedoTexture)
        {
            albedoTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            albedoTexture.filterMode = FilterMode.Point;
            AssetDatabase.AddObjectToAsset(albedoTexture, path);
        }
        albedoTexture.name = "zzz Albedo Texture";

        if( normalTexture != null && normalTexture.format != TextureFormat.RGB24 )
        {
            DestroyImmediate(normalTexture, true);
            normalTexture = null;
        }

        if (!normalTexture)
        {
            normalTexture = new Texture2D(size, size, TextureFormat.RGB24, false);
            normalTexture.filterMode = FilterMode.Point;
            AssetDatabase.AddObjectToAsset(normalTexture, path);
        }
        normalTexture.name = "zzz Normal Texture";

        #endif
    }

    public Texture2D GetAlbedoTexture()
    {
        InitAssets();
        return albedoTexture;
    }
    public Texture2D GetNormalTexture()
    {
        InitAssets();
        return normalTexture;
    }

    public Material GetMaterial(Material sourceMaterial)
    {
        InitAssets();

        MaterialEntry entry = null;
        foreach (MaterialEntry cur in materials)
        {
            if (cur.sourceMaterial == sourceMaterial)
            {
                entry = cur;
                break;
            }
        }

        if (entry == null)
        {
            entry = new MaterialEntry();
            entry.sourceMaterial = sourceMaterial;
            entry.material = new Material(Shader.Find("Standard"));
            entry.material.name = "zzz " + sourceMaterial.name;
            #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(entry.material, AssetDatabase.GetAssetPath(this));
            materials.Add(entry);
            EditorUtility.SetDirty(this);
            #endif
        }

        Material material = entry.material;

        if (UpdateMateral)
        {
            material.SetColor("_Color", sourceMaterial.GetColor("_Color"));
            material.SetColor("_EmissionColor", sourceMaterial.GetColor("_Emission"));
            material.SetFloat("_Metallic", sourceMaterial.GetFloat("_Metallic"));
            material.SetFloat("_Glossiness", sourceMaterial.GetFloat("_Smoothness"));
            material.SetTexture("_MainTex", albedoTexture);
            material.SetTexture("_BumpMap", normalTexture);
            // material.SetTexture("_ParallaxMap", heighmapTexture);
            material.SetFloat("_BumpScale", 1.0f);
            material.name = "zzz Cube Material";
        }
        return material;
    }

    public void Clear()
    {
        this.fields = new bool[cols * rows];
        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            normalTexture.SetPixel(x, y, Color.white);
            albedoTexture.SetPixel(x, y, Color.white);
        }
        normalTexture.Apply();
        albedoTexture.Apply();
        curX = 0;
        curY = 0;
        this.token = -1;
    }


    public Slot AllocateSlot(int width, int height)
    {
        if( token < 0 )
        {
            token = Random.Range(0, int.MaxValue);
        }

        int numCols = (int)Mathf.Ceil((float)width / (float)fieldSize);
        int numRows = (int)Mathf.Ceil((float)height / (float)fieldSize);

        int xFound = 0;
        int yFound = 0;

        bool found = false;

        for (int j = 0; j < rows && !found; j++)
        {
            for (int i = 0; i < cols && !found; i++)
            {
                int x = (curX + i) % cols;
                int y = (curY + j) % rows;

                // if(isAvailable(x, y))
                {
                    bool notFound = false;
                    for (int xx = x; xx < x + numCols && !notFound; xx++)
                    {
                        for (int yy = y; yy < y + numRows && !notFound; yy++)
                        {
                            if (!isAvailable(xx, yy))
                            {
                                notFound = true;
                            }
                        }
                    }
                    if (!notFound)
                    {
                        xFound = x;
                        yFound = y;
                        found = true;
                    }
                }
            }
        }

        // allocate the space

        for (int j = 0; j < numRows; j++)
        {
            for (int i = 0; i < numCols; i++)
            {
                int x = xFound + i;
                int y = yFound + j;
                fields[x + y * cols] = true;
            }
        }

        curX = xFound + numCols;
        curY = yFound; // we want to keep the line
        // Debug.Log(" xFound:" + xFound );
        Slot slot = new Slot();
        slot.width = width;
        slot.height = height;
        slot.token = token;
        slot.x = xFound * fieldSize;
        slot.y = yFound * fieldSize;
        // EditorUtility.SetDirty(this);
        DebugMarkSlot(slot, Color.yellow);
        // Debug.Log("Allocated a new Texture Atlas Slot");
        return slot;
    }

    public Vector2 GetUVPerPixel()
    {
        return new Vector2(
            1.0f / (float)size,
            1.0f / (float)size
        );
    }

    public bool IsAllocated(Slot slot)
    {
        if(slot.token != this.token || !(slot.width > 0 && slot.height > 0 && slot.x >= 0 && slot.y >= 0))
        {
            return false;
        }
        
        int numCols = (int)Mathf.Ceil((float)slot.width / (float)fieldSize);
        int numRows = (int)Mathf.Ceil((float)slot.height / (float)fieldSize);

        for (int y = slot.y / fieldSize; y < slot.y / fieldSize + numRows; y++)
        {
            for (int x = slot.x / fieldSize; x < slot.x / fieldSize + numCols; x++)
            {
                if (!fields[x + y * cols])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void DeallocateSlot(Slot slot)
    {
        if(IsAllocated(slot))
        {
            // Debug.Log("Deallocated Texture Atlas Slot at " + slot.x + " " + slot.y);
            int numCols = (int)Mathf.Ceil((float)slot.width / (float)fieldSize);
            int numRows = (int)Mathf.Ceil((float)slot.height / (float)fieldSize);
            
            DebugMarkSlot(slot, Color.green);

            for (int y = slot.y / fieldSize; y < slot.y / fieldSize + numRows; y++)
            {
                for (int x = slot.x / fieldSize; x < slot.x / fieldSize + numCols; x++)
                {
                    fields[x + y * cols] = false;
                }
            }
            // curX = slot.x / fieldSize;
            // curY = slot.y / fieldSize;
            // EditorUtility.SetDirty(this);
        }
        else
        {
            // Debug.LogWarning("Not Deallocating a Texture Atlas Slot at " + slot.x + " " + slot.y + " because it's not allocated in this atlas");
        }
    }
    
    private void DebugMarkSlot(Slot slot, Color color)
    {
        for (int x = slot.x; x < slot.x + slot.width; x++)
        for (int y = slot.y; y < slot.y + slot.height; y++)
        {
            normalTexture.SetPixel(x, y, color);
            albedoTexture.SetPixel(x, y, color);
        }
        normalTexture.Apply();
        albedoTexture.Apply();
    }

    private bool isAvailable(int x, int y)
    {
        return x < cols && y < rows && !fields[x + y * cols];
    }


    public int GetNumAllocatedFields()
    {
        int c =0;
        foreach(bool b in fields)
        {
            if(b) c++;
        }
        return c;
    }

    public int GetNumTotalFields()
    {
        return fields.Length;
    }
}