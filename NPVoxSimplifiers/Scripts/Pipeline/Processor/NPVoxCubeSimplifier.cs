using UnityEngine;

[NPipeAppendableAttribute("Cube Simplifier", typeof(NPVoxMeshOutput), true, true)]
public class NPVoxCubeSimplifier : NPVoxCompositeProcessorBase<NPVoxMeshOutput, Mesh>, NPVoxIMeshFactory
{
    override public string GetTypeName()
    {
        return "Cube Simplifier";
    }

    public bool Baked45Angle = false;

    public Material SourceMaterial;
    public NPVoxTextureAtlas TextureAtlas;

    [SerializeField, HideInInspector]
    private NPVoxTextureAtlas.Slot leftSlot = null;

    [SerializeField, HideInInspector]
    private NPVoxTextureAtlas.Slot rightSlot = null;

    [SerializeField, HideInInspector]
    private NPVoxTextureAtlas.Slot forwardSlot = null;

    [SerializeField, HideInInspector]
    private NPVoxTextureAtlas.Slot backSlot = null;

    [SerializeField, HideInInspector]
    private NPVoxTextureAtlas.Slot upSlot = null;

    [SerializeField, HideInInspector]
    private NPVoxTextureAtlas.Slot downSlot = null;

    [SerializeField, HideInInspector]
    private NPVoxTextureAtlas slotsAllocatedAtTA = null;

    public NPVoxFaces inset = new NPVoxFaces();

    public Material GetAtlasMaterial()
    {
        return TextureAtlas ? TextureAtlas.GetMaterial(SourceMaterial) : null;
    }

    public NPVoxMeshOutput InputMeshFactory
    {
        get
        {
            return (Input as NPVoxMeshOutput);
        }
    }
    override public void Import()
    {
        StorageMode = NPipeStorageMode.ATTACHED;
        // if( ! TextureAtlas)
        // {
        //     TextureAtlas = (NPVoxTextureAtlas)UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath("5c1c207e734b04e30a4fb8a357ec88df"), typeof(NPVoxTextureAtlas));
        // }
        base.Import();
    }

    // Todo add OnDestroy

    override public void Destroy()
    {
        if (leftSlot != null && slotsAllocatedAtTA != null)
        {
            slotsAllocatedAtTA.DeallocateSlot(leftSlot);
        }
        if (rightSlot != null && slotsAllocatedAtTA != null)
        {
            slotsAllocatedAtTA.DeallocateSlot(rightSlot);
        }
        if (upSlot != null && slotsAllocatedAtTA != null)
        {
            slotsAllocatedAtTA.DeallocateSlot(upSlot);
        }
        if (downSlot != null && slotsAllocatedAtTA != null)
        {
            slotsAllocatedAtTA.DeallocateSlot(downSlot);
        }
        if (backSlot != null && slotsAllocatedAtTA != null)
        {
            slotsAllocatedAtTA.DeallocateSlot(backSlot);
        }
        if (forwardSlot != null && slotsAllocatedAtTA != null)
        {
            slotsAllocatedAtTA.DeallocateSlot(forwardSlot);
        }

        base.Destroy();
    }

    override protected Mesh CreateProduct(Mesh mesh = null)
    {
        // Debug.Log("create product");
        NPVoxMeshOutput meshOutput = (Input as NPVoxMeshOutput);
        if (meshOutput && meshOutput.GetProduct() && TextureAtlas)
        {
            TextureAtlas.GetMaterial(SourceMaterial);

            NPVoxFaces includedFaces = GetIncludedFaces();
            NPVoxToUnity npVoxToUnity = InputMeshFactory.GetNPVoxToUnity();
            int faceCount = GetFaceCount();
            NPVoxBox originalBox = InputMeshFactory.GetVoxModel().BoundingBox;

            NPVoxBox cutoutBox = originalBox.Clone();
            NPVoxFaces cutout = InputMeshFactory.Cutout;
            Vector3 cutoutOffset = Vector3.zero;
            if (cutout != null)
            {
                Vector3 originalCenter = cutoutBox.SaveCenter;
                cutoutBox.Left = (sbyte)Mathf.Abs(cutout.Left);
                cutoutBox.Down = (sbyte)Mathf.Abs(cutout.Down);
                cutoutBox.Back = (sbyte)Mathf.Abs(cutout.Back);
                cutoutBox.Right = (sbyte)(cutoutBox.Right - (sbyte)Mathf.Abs(cutout.Right));
                cutoutBox.Up = (sbyte)(cutoutBox.Up - (sbyte)Mathf.Abs(cutout.Up));
                cutoutBox.Forward = (sbyte)(cutoutBox.Forward - (sbyte)Mathf.Abs(cutout.Forward));
                cutoutOffset = Vector3.Scale(originalCenter - cutoutBox.SaveCenter, InputMeshFactory.VoxelSize);
            }

            // we have to be careful. Unlike cutout, which is already removed from the mesh we want to render, the inset is not yet applied and
            // also won't result in a "move" of the object. So it's important that we calculate a correct offset for our final mesh.
            NPVoxBox insetBox = cutoutBox.Clone();
            Vector3 insetOffset = Vector3.zero;
            if (inset != null)
            {
                Vector3 cutoutCenter = cutoutBox.SaveCenter;
                insetBox.Left += (sbyte)Mathf.Abs(inset.Left);
                insetBox.Right -= (sbyte)Mathf.Abs(inset.Right);
                insetBox.Down += (sbyte)Mathf.Abs(inset.Down);
                insetBox.Up -= (sbyte)Mathf.Abs(inset.Up);
                insetBox.Back += (sbyte)Mathf.Abs(inset.Back);
                insetBox.Forward -= (sbyte)Mathf.Abs(inset.Forward);
                insetOffset = Vector3.Scale(cutoutCenter - insetBox.SaveCenter, InputMeshFactory.VoxelSize);
            }
            Vector3 insetCenter = insetBox.SaveCenter;

            if (Baked45Angle)
            {
                backSlot = CreateTexture(backSlot,
                    includedFaces.Back != 0,
                    insetBox.Size.X, insetBox.Size.Y,
                    Quaternion.Euler(-45, 0, 0),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetCenter.x, insetCenter.y, insetBox.Back)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.X,  ((float)insetBox.Size.Y) / Mathf.Sqrt(2))) * 0.5f,
                    Quaternion.Euler(+45, 0, 0)
                );
                downSlot = CreateTexture(downSlot,
                    includedFaces.Down != 0,
                    insetBox.Size.X, insetBox.Size.Z * 3,
                    Quaternion.Euler(-45, 0, 0),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetCenter.x, insetBox.Down, insetCenter.z)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.X, ((float)insetBox.Size.Z) / Mathf.Sqrt(2))) * 0.5f,
                    Quaternion.Euler(-45, 0, 0)
                );

                leftSlot = CreateTexture(leftSlot, false, 0, 0, Quaternion.identity, Vector3.zero, Vector2.zero, Quaternion.identity);
                rightSlot = CreateTexture(rightSlot, false, 0, 0, Quaternion.identity, Vector3.zero, Vector2.zero, Quaternion.identity);
                upSlot = CreateTexture(upSlot, false, 0, 0, Quaternion.identity, Vector3.zero, Vector2.zero, Quaternion.identity);
                forwardSlot = CreateTexture(forwardSlot, false, 0, 0, Quaternion.identity, Vector3.zero, Vector2.zero, Quaternion.identity);
            }
            else
            {
                leftSlot = CreateTexture(leftSlot,
                    includedFaces.Left != 0,
                    insetBox.Size.Z, insetBox.Size.Y,
                    Quaternion.Euler(0, 90, 0),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetBox.Left, insetCenter.y, insetCenter.z)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.Z, insetBox.Size.Y)) * 0.5f,
                    Quaternion.identity
                );
                rightSlot = CreateTexture(rightSlot,
                    includedFaces.Right != 0,
                    insetBox.Size.Z, insetBox.Size.Y,
                    Quaternion.Euler(0, -90, 0),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetBox.Right, insetCenter.y, insetCenter.z)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.Z, insetBox.Size.Y)) * 0.5f,
                    Quaternion.identity
                );
                downSlot = CreateTexture(downSlot,
                    includedFaces.Down != 0,
                    insetBox.Size.X, insetBox.Size.Z,
                    Quaternion.Euler(-90, 0, 0),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetCenter.x, insetBox.Down, insetCenter.z)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.X, insetBox.Size.Z)) * 0.5f,
                    Quaternion.identity
                );
                upSlot = CreateTexture(upSlot,
                    includedFaces.Up != 0,
                    insetBox.Size.X, insetBox.Size.Z,
                    Quaternion.Euler(90, 0, 180),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetCenter.x, insetBox.Up, insetCenter.z)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.X, insetBox.Size.Z)) * 0.5f,
                    Quaternion.identity
                );
                backSlot = CreateTexture(backSlot,
                    includedFaces.Back != 0,
                    insetBox.Size.X, insetBox.Size.Y,
                    Quaternion.Euler(0, 0, 0),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetCenter.x, insetCenter.y, insetBox.Back)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.X, insetBox.Size.Y)) * 0.5f,
                    Quaternion.identity
                );
                forwardSlot = CreateTexture(forwardSlot,
                    includedFaces.Forward != 0,
                    insetBox.Size.X, insetBox.Size.Y,
                    Quaternion.Euler(-180, 0, 0),
                    npVoxToUnity.ToUnityPosition(new Vector3(insetCenter.x, insetCenter.y, insetBox.Forward)),
                    npVoxToUnity.ToUnityDirection(new Vector2(insetBox.Size.X, insetBox.Size.Y)) * 0.5f,
                    Quaternion.identity
                );
            }
            slotsAllocatedAtTA = TextureAtlas;

            if (mesh == null)
            {
                mesh = new Mesh();
            }
            else
            {
                mesh.Clear();
            }
            mesh.name = "zzz Cube Simplifier Mesh";

            int border = 1;
            var vertices = new Vector3[faceCount * 4];
            var uvs = new Vector2[faceCount * 4];
            var tris = new int[faceCount * 3 * 2];
            var normals = new Vector3[faceCount * 4];

            int v = 0;
            int t = 0;

            int v0 = 0;

            System.Action<Vector3, NPVoxTextureAtlas.Slot> addQuad = (Vector3 dir, NPVoxTextureAtlas.Slot theSlot) =>
            {
                normals[v0] = dir;
                normals[v0 + 1] = dir;
                normals[v0 + 2] = dir;
                normals[v0 + 3] = dir;

                tris[t++] = v0;
                tris[t++] = v0 + 1;
                tris[t++] = v0 + 2;

                tris[t++] = v0;
                tris[t++] = v0 + 2;
                tris[t++] = v0 + 3;

                Vector2 uvMax = theSlot.GetUVmax(border);
                Vector2 uvMin = theSlot.GetUVmin(border);

                uvs[v0].x = uvMin.x;
                uvs[v0].y = uvMax.y;
                uvs[v0 + 1].x = uvMax.x;
                uvs[v0 + 1].y = uvMax.y;
                uvs[v0 + 2].x = uvMax.x;
                uvs[v0 + 2].y = uvMin.y;
                uvs[v0 + 3].x = uvMin.x;
                uvs[v0 + 3].y = uvMin.y;
            };

            NPVoxBox bounds = insetBox;

            Vector3 LDB = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.LeftDownBack) + (Vector3.left * npVoxToUnity.VoxeSize.x + Vector3.down * npVoxToUnity.VoxeSize.y + Vector3.back * npVoxToUnity.VoxeSize.z) * 0.5f;
            Vector3 RDB = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.RightDownBack) + (Vector3.right * npVoxToUnity.VoxeSize.x + Vector3.down * npVoxToUnity.VoxeSize.y + Vector3.back * npVoxToUnity.VoxeSize.z) * 0.5f;
            Vector3 LUB = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.LeftUpBack) + (Vector3.left * npVoxToUnity.VoxeSize.x + Vector3.up * npVoxToUnity.VoxeSize.y + Vector3.back * npVoxToUnity.VoxeSize.z) * 0.5f;
            Vector3 RUB = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.RightUpBack) + (Vector3.right * npVoxToUnity.VoxeSize.x + Vector3.up * npVoxToUnity.VoxeSize.y + Vector3.back * npVoxToUnity.VoxeSize.z) * 0.5f;
            Vector3 LDF = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.LeftDownForward) + (Vector3.left * npVoxToUnity.VoxeSize.x + Vector3.down * npVoxToUnity.VoxeSize.y + Vector3.forward * npVoxToUnity.VoxeSize.z) * 0.5f;
            Vector3 RDF = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.RightDownForward) + (Vector3.right * npVoxToUnity.VoxeSize.x + Vector3.down * npVoxToUnity.VoxeSize.y + Vector3.forward * npVoxToUnity.VoxeSize.z) * 0.5f;
            Vector3 LUF = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.LeftUpForward) + (Vector3.left * npVoxToUnity.VoxeSize.x + Vector3.up * npVoxToUnity.VoxeSize.y + Vector3.forward * npVoxToUnity.VoxeSize.z) * 0.5f;
            Vector3 RUF = cutoutOffset + npVoxToUnity.ToUnityPosition(bounds.RightUpForward) + (Vector3.right * npVoxToUnity.VoxeSize.x + Vector3.up * npVoxToUnity.VoxeSize.y + Vector3.forward * npVoxToUnity.VoxeSize.z) * 0.5f;

            if (downSlot != null)
            {
                v0 = v;
                vertices[v++] = LDB;
                vertices[v++] = RDB;
                vertices[v++] = RDF;
                vertices[v++] = LDF;

                addQuad(Vector3.down, downSlot);
            }

            if (upSlot != null)
            {
                v0 = v;
                vertices[v++] = RUB;
                vertices[v++] = LUB;
                vertices[v++] = LUF;
                vertices[v++] = RUF;

                addQuad(Vector3.up, upSlot);
            }

            if (forwardSlot != null)
            {
                v0 = v;
                vertices[v++] = LDF;
                vertices[v++] = RDF;
                vertices[v++] = RUF;
                vertices[v++] = LUF;

                addQuad(Vector3.forward, forwardSlot);
            }

            if (backSlot != null)
            {
                v0 = v;
                vertices[v++] = LUB;
                vertices[v++] = RUB;
                vertices[v++] = RDB;
                vertices[v++] = LDB;

                addQuad(Vector3.back, backSlot);
            }
            if (leftSlot != null)
            {
                v0 = v;
                vertices[v++] = LUF;
                vertices[v++] = LUB;
                vertices[v++] = LDB;
                vertices[v++] = LDF;

                addQuad(Vector3.left, leftSlot);
            }
            if (rightSlot != null)
            {
                v0 = v;
                vertices[v++] = RUB;
                vertices[v++] = RUF;
                vertices[v++] = RDF;
                vertices[v++] = RDB;

                addQuad(Vector3.right, rightSlot);
            }

            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            mesh.normals = normals;
            TangentSolver.Solve(mesh);
            // mesh.bounds = new Bounds(
            //     insetOffset,
            //     new Vector3(
            //         bounds.Size.X * npVoxToUnity.VoxeSize.x,
            //         bounds.Size.Y * npVoxToUnity.VoxeSize.y,
            //         bounds.Size.Z * npVoxToUnity.VoxeSize.z
            //     )
            // );
            Mesh sourceMesh = meshOutput.GetProduct();
            mesh.bounds = sourceMesh.bounds;
            mesh.name = "zzz Cube Mesh ";
            return mesh;
        }
        else
        {
            Debug.LogWarning("No Input set up");
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            else
            {
                mesh.Clear();
            }
            return mesh;
        }
    }

    private NPVoxTextureAtlas.Slot CreateTexture(NPVoxTextureAtlas.Slot slot, bool bInclude, int width, int height, Quaternion cameraAngle, Vector3 camPosition, Vector2 ratio, Quaternion normalOffset)
    {
        int border = 1;
        NPVoxMeshOutput meshOutput = (Input as NPVoxMeshOutput);
        Mesh sourceMesh = meshOutput.GetProduct();
        Texture2D albedoTexture = TextureAtlas.GetAlbedoTexture();
        Texture2D normalTexture = TextureAtlas.GetNormalTexture();

        // if(BakeGaianigmaAngle)
        // {
        //     height *= 2;
        // }

        if (bInclude)
        {
            if (slot == null || slot.width != width + border * 2 || slot.height != height + border * 2 || !TextureAtlas.IsAllocated(slot) || slotsAllocatedAtTA != this.TextureAtlas)
            {
                if (slot.width != 0 && slot.height != 0 && slotsAllocatedAtTA != null)
                {
                    slotsAllocatedAtTA.DeallocateSlot(slot);
                }
                slot = TextureAtlas.AllocateSlot(width + border * 2, height + border * 2);
            }

            NPVoxTextureGenerator.CreateSubTexture(sourceMesh, cameraAngle, camPosition, albedoTexture, NPVoxTextureType.ALBEDO, width, height, slot.x + border, slot.y + border, ratio, normalOffset);
            NPVoxTextureGenerator.CreateSubTexture(sourceMesh, cameraAngle, camPosition, normalTexture, NPVoxTextureType.NORMALMAP, width, height, slot.x + border, slot.y + border, ratio, normalOffset);
            if (border > 0)
            {
                NPVoxTextureGenerator.AddBorder(albedoTexture, width, height, slot.x + border, slot.y + border);
                NPVoxTextureGenerator.AddBorder(normalTexture, width, height, slot.x + border, slot.y + border);
            }
        }
        else if (slot != null)
        {
            if (slotsAllocatedAtTA != null)
            {
                slotsAllocatedAtTA.DeallocateSlot(slot);
            }
            slot = null;
        }
        return slot;
    }

    public NPVoxFaces GetIncludedFaces()
    {
        NPVoxFaces sides = new NPVoxFaces();
        NPVoxMeshOutput meshOutput = InputMeshFactory;
        if (meshOutput != null)
        {
            // sides.Back = (sbyte)((meshOutput.Loop.Back == 0 && meshOutput.Include.Back != 0) ? 1 : 0);
            // sides.Forward = (sbyte)((meshOutput.Loop.Forward == 0 && meshOutput.Include.Forward != 0) ? 1 : 0);
            // sides.Left = (sbyte)((meshOutput.Loop.Left == 0 && meshOutput.Include.Left != 0) ? 1 : 0);
            // sides.Right = (sbyte)((meshOutput.Loop.Right == 0 && meshOutput.Include.Right != 0) ? 1 : 0);
            // sides.Up = (sbyte)((meshOutput.Loop.Up == 0 && meshOutput.Include.Up != 0) ? 1 : 0);
            // sides.Down = (sbyte)((meshOutput.Loop.Down == 0 && meshOutput.Include.Down != 0) ? 1 : 0);

            sides.Back = (sbyte)((meshOutput.Cutout.Back <= 0 && meshOutput.Loop.Back == 0 && meshOutput.Include.Back != 0) ? 1 : 0);
            sides.Forward = (sbyte)((!Baked45Angle && meshOutput.Cutout.Forward <= 0 && meshOutput.Loop.Forward == 0 && meshOutput.Include.Forward != 0) ? 1 : 0);
            sides.Left = (sbyte)((!Baked45Angle && meshOutput.Cutout.Left <= 0 && meshOutput.Loop.Left == 0 && meshOutput.Include.Left != 0) ? 1 : 0);
            sides.Right = (sbyte)((!Baked45Angle && meshOutput.Cutout.Right <= 0 && meshOutput.Loop.Right == 0 && meshOutput.Include.Right != 0) ? 1 : 0);
            sides.Up = (sbyte)((!Baked45Angle && meshOutput.Cutout.Up <= 0 && meshOutput.Loop.Up == 0 && meshOutput.Include.Up != 0) ? 1 : 0);
            sides.Down = (sbyte)((meshOutput.Cutout.Down <= 0 && meshOutput.Loop.Down == 0 && meshOutput.Include.Down != 0) ? 1 : 0);
        }
        return sides;
    }

    public NPVoxCoord GetOutputVoxModelSize()
    {
        return InputMeshFactory.GetOutputSize();
    }

    public int GetFaceCount()
    {
        NPVoxFaces faces = GetIncludedFaces();
        return faces.Back + faces.Forward + faces.Left + faces.Right + faces.Up + faces.Down;
    }


}