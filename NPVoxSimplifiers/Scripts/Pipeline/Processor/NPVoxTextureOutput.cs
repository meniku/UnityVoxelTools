using UnityEngine;
using System.Collections;

[NPipeAppendableAttribute("Texture Output", typeof(NPVoxIMeshFactory), true, true)]
public class NPVoxTextureOutput : NPVoxCompositeProcessorBase<NPVoxIMeshFactory, Texture2D>, NPVoxITextureFactory
{   
    public int Width = 16;
    public int Height = 16;
    
    public NPVoxTextureType Type;

    override protected Texture2D CreateProduct(Texture2D reuse = null)
    {
        Texture2D texture = reuse;

        if (texture == null)
        {
            texture = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
        }

        if (Input == null)
        {
            Debug.LogWarning("No Input set up");
            return texture;
        }
		
		this.hideFlags = HideFlags.HideInHierarchy;


        Mesh mesh = ((NPVoxIMeshFactory)Input).GetProduct();
        if (mesh)
        {
			NPVoxTextureGenerator.CreateTexture(mesh, texture, Type, Width, Height);
        }
        return texture;
    }
    
    override public string GetTypeName()
    {
        return "Texture Output";
    }
}
