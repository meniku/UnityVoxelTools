using UnityEngine;

public class NPVoxTextureGenerator
{
    public static void CreateTexture(
        Mesh mesh,
        Texture2D texture,
        NPVoxTextureType textureType,
        int textureWidth,
        int textureHeight
    )
    {
        texture.Resize(textureWidth, textureHeight, TextureFormat.ARGB32, true);
        texture.filterMode = FilterMode.Point;

        GameObject go = new GameObject();
        Camera cam = go.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.enabled = true;
        cam.orthographic = true;
        cam.orthographicSize = 1;
        cam.targetTexture = new RenderTexture(textureWidth, textureHeight, 1, RenderTextureFormat.ARGB32);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.cullingMask = 1 << 31;

        cam.Render();

        GameObject target = new GameObject();
        MeshFilter filter = target.AddComponent<MeshFilter>();
        MeshRenderer renderer = target.AddComponent<MeshRenderer>();
        Shader shader = null;
        if( textureType == NPVoxTextureType.ALBEDO )
        {
            shader = Shader.Find(NPVoxConstants.SNAPSHOT_SHADER_ALBEDO );
        }
        else if( textureType == NPVoxTextureType.NORMALMAP )
        {
            shader = Shader.Find(NPVoxConstants.SNAPSHOT_SHADER_NORMALMAP );
        }
        else if( textureType == NPVoxTextureType.HEIGHTMAP )
        {
            shader = Shader.Find(NPVoxConstants.SNAPSHOT_SHADER_HEIGHMAP );
            Debug.Log("using heighmap");
        }

        renderer.material = new Material(shader);
        target.transform.position = new Vector3(0f, 0f, 0f);

        filter.sharedMesh = mesh;
        target.layer = 31;

        cam.Render();
        RenderTexture.active = cam.targetTexture;

        texture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0, false);
        if(textureType == NPVoxTextureType.NORMALMAP)
        {
            Color theColour = new Color();
            for (int x = 0; x < textureWidth; x++)
                for (int y = 0; y < textureHeight; y++)
                {
                    theColour.r = texture.GetPixel(x, y).g;
                    theColour.g = theColour.r;
                    theColour.b = theColour.r;
                    theColour.a = texture.GetPixel(x, y).r;
                    texture.SetPixel(x, y, theColour);
                }
        }
        texture.Apply();
        // Clean up
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(target);
    }

    public static void CreateSubTexture(
         Mesh mesh,
         Quaternion camRotation,
         Vector3 camPosition,
         Texture2D texture,
         NPVoxTextureType textureType,
         int textureWidth,
         int textureHeight,
         int targetX,
         int targetY,
         Vector2 ratio,
         Quaternion normalOffset
     )
    {
        GameObject go = new GameObject();
        Camera cam = go.AddComponent<Camera>();
        cam.backgroundColor = new Color(0,0,0,0);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.enabled = true;
        cam.orthographic = true;
        cam.orthographicSize = 1 * (((float)textureHeight) / 16.0f);
        cam.projectionMatrix = Matrix4x4.Ortho(
            -1f * ratio.x,
            1f * ratio.x,
            -1f * ratio.y,
            1f * ratio.y,
            cam.nearClipPlane,
            cam.farClipPlane
        );
        cam.targetTexture = new RenderTexture(textureWidth, textureHeight, 1, RenderTextureFormat.ARGB32);
        cam.cullingMask = 1 << 31;
        cam.Render();

        // Setup Camera Rotation & Position
        go.transform.rotation = camRotation;;
        Vector3 dist = go.transform.TransformDirection(Vector3.back) * 10f;
        go.transform.position = camPosition;
        go.transform.position += dist;

        GameObject target = new GameObject();
        MeshFilter filter = target.AddComponent<MeshFilter>();
        MeshRenderer renderer = target.AddComponent<MeshRenderer>();
        Shader shader = null;
        if( textureType == NPVoxTextureType.ALBEDO )
        {
            shader = Shader.Find(NPVoxConstants.SNAPSHOT_SHADER_ALBEDO );
        }
        else if( textureType == NPVoxTextureType.NORMALMAP )
        {
            shader = Shader.Find(NPVoxConstants.SNAPSHOT_SHADER_NORMALMAP );
        }
        else if( textureType == NPVoxTextureType.HEIGHTMAP )
        {
            shader = Shader.Find(NPVoxConstants.SNAPSHOT_SHADER_HEIGHMAP );
            // texture.filterMode = FilterMode.Bilinear;
        }

        renderer.material = new Material(shader);
        target.transform.position = new Vector3(0f, 0f, 0f);

        filter.sharedMesh = mesh;
        target.layer = 31;

        cam.Render();
        RenderTexture.active = cam.targetTexture;

        texture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), targetX, targetY, false);
        if (textureType == NPVoxTextureType.NORMALMAP)
        {
            for (int x = targetX; x < targetX + textureWidth; x++)
                for (int y = targetY; y < targetY + textureHeight; y++)
                {
                    Color original = texture.GetPixel(x, y);
                    Vector4 unpackedNormal = new Vector4(original.r - 0.5f, original.g - 0.5f, original.b - 0.5f, original.a - 0.5f) * 2f;
                    Vector3 normal = unpackedNormal;
                    if(normalOffset != Quaternion.identity)
                    {
                        normal = normalOffset * normal;
                    }
                    
                    Color packedNormal = new Color();
                    packedNormal.r = normal.x / 2f + 0.5f;
                    packedNormal.g = normal.y / 2f + 0.5f;
                    packedNormal.b = normal.z / 2f + 0.5f;
                    texture.SetPixel(x, y, packedNormal);
                }
        }
        else
        {
            Color theColour = new Color();
            for (int x = targetX; x < targetX + textureWidth; x++)
                for (int y = targetY; y < targetY + textureHeight; y++)
                {
                    // texture.ReadPixels(new Rect(x, y, 1, 1), x, y, false);
                    theColour = texture.GetPixel(x, y);
                    // theColour.a = 1.0f;
                    texture.SetPixel(x, y, theColour);
                }
        }
        texture.Apply();
        // Clean up
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(target);
    }


    public static void AddBorder(
        Texture2D texture,
        int textureWidth,
        int textureHeight,
        int targetX,
        int targetY)
    {
        CopyArea(texture, new Rect(targetX, targetY, 1, textureHeight), texture, targetX - 1, targetY);
        CopyArea(texture, new Rect(targetX + textureWidth - 1, targetY, 1, textureHeight), texture, targetX + textureWidth, targetY);
        CopyArea(texture, new Rect(targetX, targetY, textureWidth, 1), texture, targetX, targetY - 1);
        CopyArea(texture, new Rect(targetX, targetY + textureHeight - 1, textureWidth, 1), texture, targetX, targetY + textureHeight);
        texture.SetPixel(targetX - 1, targetY - 1, texture.GetPixel(targetX, targetY));
        texture.SetPixel(targetX + textureWidth, targetY + textureHeight, texture.GetPixel(targetX + textureWidth - 1, targetY + textureHeight - 1));
        texture.SetPixel(targetX - 1, targetY + textureHeight, texture.GetPixel(targetX, targetY + textureHeight - 1));
        texture.SetPixel(targetX + textureWidth, targetY - 1, texture.GetPixel(targetX + textureWidth - 1, targetY));
    }

    private static void CopyArea(Texture2D source, Rect sourceRect, Texture2D target, int targetX, int targetY)
    {
        int dX = targetX - (int)Mathf.Round(sourceRect.xMin);
        int dY = targetY - (int)Mathf.Round(sourceRect.yMin);
        for (int x = (int)Mathf.Round(sourceRect.xMin); x < (int)Mathf.Round(sourceRect.xMax); x++)
        {
            for (int y = (int)Mathf.Round(sourceRect.yMin); y < (int)Mathf.Round(sourceRect.yMax); y++)
            {
                // Debug.Log("X: " + x + " Y:" + y + " dX: " + dX + " dY: " + dY);
                target.SetPixel(x + dX, y + dY, source.GetPixel(x, y));
            }
        }

        target.Apply();
    }
}