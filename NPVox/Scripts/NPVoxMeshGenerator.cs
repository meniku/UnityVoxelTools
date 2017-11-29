using UnityEngine;
using System;

public class NPVoxMeshGenerator
{
    public static void CreateMesh(
        NPVoxModel model,
        Mesh mesh,
        Vector3 cubeSize,
        Vector3 NormalVariance,
        int NormalVarianceSeed = 0,
        NPVoxOptimization optimization = NPVoxOptimization.OFF,
        NPVoxNormalMode NormalMode = NPVoxNormalMode.SMOOTH,
        int BloodColorIndex = 0,
        NPVoxFaces loop = null,
        NPVoxFaces cutout = null, 
        NPVoxFaces include = null,
        int MinVertexGroups = 1,
        NPVoxNormalMode[] NormalModePerVoxelGroup = null
    )
    {
        bool hasVoxelGroups = model.HasVoxelGroups();
        var vertices = new Vector3[model.NumVoxels * 8];
        byte vertexGroupCount = model.NumVoxelGroups;
        var triangles = new int[vertexGroupCount, model.NumVoxels * 36];

        var normals = new Vector3[model.NumVoxels * 8];
        var tangents = new Vector4[model.NumVoxels * 8];
        var colors = new Color[model.NumVoxels * 8];

        int currentVertexIndex = 0;
        var currentTriangleIndex = new int[vertexGroupCount];
        for (int i = 0; i < vertexGroupCount; i++)
        {
            currentTriangleIndex[i] = 0;
        }

        UnityEngine.Random.InitState( NormalVarianceSeed );

        if (loop == null)
        {
            loop = new NPVoxFaces();
        }

        if (include == null)
        {
            include = new NPVoxFaces(1, 1, 1, 1, 1, 1);
        }

        NPVoxBox voxelsToInclude = model.BoundingBox;
        Vector3 cutoutOffset = Vector3.zero;
        if (cutout != null)
        {
            Vector3 originalCenter = voxelsToInclude.SaveCenter;
            voxelsToInclude.Left = (sbyte)Mathf.Abs(cutout.Left);
            voxelsToInclude.Down = (sbyte)Mathf.Abs(cutout.Down);
            voxelsToInclude.Back = (sbyte)Mathf.Abs(cutout.Back);
            voxelsToInclude.Right = (sbyte)(voxelsToInclude.Right - (sbyte)Mathf.Abs(cutout.Right));
            voxelsToInclude.Up = (sbyte)(voxelsToInclude.Up - (sbyte)Mathf.Abs(cutout.Up));
            voxelsToInclude.Forward = (sbyte)(voxelsToInclude.Forward - (sbyte)Mathf.Abs(cutout.Forward));
            cutoutOffset = Vector3.Scale(originalCenter - voxelsToInclude.SaveCenter, cubeSize);
        }

        NPVoxToUnity npVoxToUnity = new NPVoxToUnity(model, cubeSize);
        Vector3 size = new Vector3(
            voxelsToInclude.Size.X * cubeSize.x,
            voxelsToInclude.Size.Y * cubeSize.y,
            voxelsToInclude.Size.Z * cubeSize.z
        );

        NPVoxBox voxelNormalNeighbours = new NPVoxBox(new NPVoxCoord(-1, -1, -1), new NPVoxCoord(1, 1, 1));

        NPVoxNormalMode normalMode = NormalMode;

        foreach (NPVoxCoord voxCoord in voxelsToInclude.Enumerate())
        {
            if (model.HasVoxel(voxCoord))
            {
                Vector3 voxelCenter = npVoxToUnity.ToUnityPosition(voxCoord) + cutoutOffset;

                int vertexGroupIndex = 0;
                if (hasVoxelGroups)
                {
                    vertexGroupIndex = model.GetVoxelGroup(voxCoord);
                }

                if (NormalModePerVoxelGroup != null && NormalModePerVoxelGroup.Length > vertexGroupIndex)
                {
                    normalMode = NormalModePerVoxelGroup[vertexGroupIndex];
                }
                else
                {
                    normalMode = NormalMode;
                }

                // do we have this side
                bool hasLeft = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.LEFT, loop));
                bool hasRight = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.RIGHT, loop));
                bool hasDown = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.DOWN, loop));
                bool hasUp = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.UP, loop));
                bool hasForward = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.FORWARD, loop));
                bool hasBack = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.BACK, loop));

                // do we actually want to include this side in our mesh
                // NOTE: cutout < 0 means we still render the mesh even though it is cutout
                //       cutout > 0 means we don't render the mesh when cutout
                bool includeLeft = (hasLeft || (cutout.Left < 0 && voxCoord.X == voxelsToInclude.Left)) && include.Left == 1;
                bool includeRight = (hasRight || (cutout.Right < 0 && voxCoord.X == voxelsToInclude.Right)) && include.Right == 1;
                bool includeUp = (hasUp || (cutout.Up < 0 && voxCoord.Y == voxelsToInclude.Up)) && include.Up == 1;
                bool includeDown = (hasDown || (cutout.Down < 0 && voxCoord.Y == voxelsToInclude.Down)) && include.Down == 1;
                bool includeBack = (hasBack || (cutout.Back < 0 && voxCoord.Z == voxelsToInclude.Back)) && include.Back == 1;
                bool includeForward = (hasForward || (cutout.Forward < 0 && voxCoord.Z == voxelsToInclude.Forward)) && include.Forward == 1;
                
                bool isHidden = !hasForward && !hasBack && !hasLeft && !hasRight && !hasUp && !hasDown;

                if (isHidden && optimization == NPVoxOptimization.PER_VOXEL)
                {
                    continue;
                }

                if (isHidden && BloodColorIndex > 0)
                {
                    model.SetVoxel(voxCoord, (byte)BloodColorIndex); // WTF WTF WTF?!? we should not modify the MODEL in here !!!!
                }

                Color color = model.GetColor(voxCoord);

                // prepare cube vertices
                int numVertices = 0;

                int[] vertexIndexOffsets = new int[8];
                Vector3[] vertexPositionOffsets = new Vector3[8];

                if (optimization != NPVoxOptimization.PER_FACE || includeBack || includeLeft || includeDown)
                {
                    vertexIndexOffsets[0] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[0]] = new Vector3(-0.5f, -0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || includeBack || includeRight || includeDown)
                {
                    vertexIndexOffsets[1] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[1]] = new Vector3(0.5f, -0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || includeBack || includeLeft || includeUp)
                {
                    vertexIndexOffsets[2] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[2]] = new Vector3(-0.5f, 0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || includeBack || includeRight || includeUp)
                {
                    vertexIndexOffsets[3] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[3]] = new Vector3(0.5f, 0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || includeForward || includeLeft || includeDown)
                {
                    vertexIndexOffsets[4] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[4]] = new Vector3(-0.5f, -0.5f, 0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || includeForward || includeRight || includeDown)
                {
                    vertexIndexOffsets[5] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[5]] = new Vector3(0.5f, -0.5f, 0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || includeForward || includeLeft || includeUp)
                {
                    vertexIndexOffsets[6] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[6]] = new Vector3(-0.5f, 0.5f, 0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || includeForward || includeRight || includeUp)
                {
                    vertexIndexOffsets[7] = numVertices++;
                    vertexPositionOffsets[vertexIndexOffsets[7]] = new Vector3(0.5f, 0.5f, 0.5f);
                }

                // add cube faces
                int i = currentTriangleIndex[vertexGroupIndex];

                // back
                if (optimization != NPVoxOptimization.PER_FACE || includeBack)
                {
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[0];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[2];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[1];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[2];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[3];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[1];
                }

                // Forward
                if (optimization != NPVoxOptimization.PER_FACE || includeForward)
                {
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[6];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[4];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[5];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[7];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[6];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[5];
                }

                // right
                if (optimization != NPVoxOptimization.PER_FACE || includeRight)
                {
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[1];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[3];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[5];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[3];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[7];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[5];
                }

                // left
                if (optimization != NPVoxOptimization.PER_FACE || includeLeft)
                {
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[0];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[4];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[2];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[2];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[4];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[6];
                }

                // up
                if (optimization != NPVoxOptimization.PER_FACE || includeUp)
                {
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[2];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[6];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[3];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[3];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[6];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[7];
                }

                // down
                if (optimization != NPVoxOptimization.PER_FACE || includeDown)
                {
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[0];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[1];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[4];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[1];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[5];
                    triangles[vertexGroupIndex, i++] = currentVertexIndex + vertexIndexOffsets[4];
                }

                // TODO create some kind of strategy pattern for the normal calculation, else this here is becomming a mess ...
                Vector3 variance = Vector3.zero;
                if (NormalVariance.x != 0 || NormalVariance.y != 0 || NormalVariance.z != 0)
                {
                    variance.x = -NormalVariance.x * 0.5f + 2 * UnityEngine.Random.value * NormalVariance.x;
                    variance.y = -NormalVariance.y * 0.5f + 2 * UnityEngine.Random.value * NormalVariance.y;
                    variance.z = -NormalVariance.z * 0.5f + 2 * UnityEngine.Random.value * NormalVariance.z;
                }

                // calculate normals based on present neighbour voxels
                Vector3 voxelNormal = Vector3.zero;
                if (!isHidden)
                {
                    foreach (NPVoxCoord offset in voxelNormalNeighbours.Enumerate())
                    {
                        NPVoxCoord checkCoord = voxCoord + offset;
                        checkCoord = model.LoopCoord(checkCoord, loop);
                        if (!model.HasVoxel(checkCoord))
                        {
                            voxelNormal += NPVoxCoordUtil.ToVector(offset);
                        }
                    }
                    voxelNormal.Normalize();
                }
                else
                {
                    voxelNormal = voxelCenter.normalized;
                }

                // Normals
                for (int t = 0; t < numVertices; t++)
                {
                    Vector3 normal = Vector3.zero;

                    switch (normalMode)
                    {
                        case NPVoxNormalMode.VOXEL:
                            normal = voxelNormal;

                            normal = Vector3.zero;

                            if (vertexPositionOffsets[t].x < 0.0f)
                            {
                                if (hasLeft && !hasForward && !hasBack && !hasUp && !hasDown)
                                {
                                    normal.x = -1;
                                }
                                else
                                {
                                    normal.x = voxelNormal.x;
                                }
                            }
                            else if (vertexPositionOffsets[t].x > 0.0f)
                            {
                                if (hasRight && !hasForward && !hasBack && !hasUp && !hasDown)
                                {
                                    normal.x = 1;
                                }
                                else
                                {
                                    normal.x = voxelNormal.x;
                                }
                            }

                            if (vertexPositionOffsets[t].y < 0.0f)
                            {
                                if (hasUp && !hasForward && !hasBack && !hasLeft && !hasRight)
                                {
                                    normal.y = -1;
                                }
                                else
                                {
                                    normal.y = voxelNormal.y;
                                }
                            }
                            else if (vertexPositionOffsets[t].y > 0.0f)
                            {
                                if (hasDown && !hasForward && !hasBack && !hasLeft && !hasRight)
                                {
                                    normal.y = +1;
                                }
                                else
                                {
                                    normal.y = voxelNormal.y;
                                }
                            }

                            if (vertexPositionOffsets[t].z < 0.0f)
                            {
                                if (hasBack && !hasLeft && !hasRight && !hasUp && !hasDown)
                                {
                                    normal.z = -1;
                                }
                                else
                                {
                                    normal.z = voxelNormal.z;
                                }
                            }
                            else if (vertexPositionOffsets[t].z > 0.0f)
                            {
                                if (hasForward && !hasLeft && !hasRight && !hasUp && !hasDown)
                                {
                                    normal.z = +1;
                                }
                                else
                                {
                                    normal.z = voxelNormal.z;
                                }
                            }

                            if (Mathf.Abs(normal.x) < 0.1f && Mathf.Abs(normal.y) < 0.1f && Mathf.Abs(normal.z) < 0.1f)
                            {
                                // we would like to have full color when we are a stand-alone voxel, however there is no way to do so right now, so we just
                                // fallback to the centoid normal
                                normal = voxelCenter;
                            }

                            normal.Normalize();
                            break;

                        case NPVoxNormalMode.SMOOTH:
                            normal = Vector3.zero;

                            for (float xx = -0.5f; xx < 1.0f; xx += 1f)
                                for (float yy = -.5f; yy < 1; yy += 1)
                                    for (float zz = -.5f; zz < 1; zz += 1)
                                    {
                                        sbyte xCoord = (sbyte)Mathf.Round(vertexPositionOffsets[t].x + xx);
                                        sbyte yCoord = (sbyte)Mathf.Round(vertexPositionOffsets[t].y + yy);
                                        sbyte zCoord = (sbyte)Mathf.Round(vertexPositionOffsets[t].z + zz);

                                        if (!model.HasVoxel(voxCoord + new NPVoxCoord((sbyte)xCoord, (sbyte)yCoord, (sbyte)zCoord)))
                                        {

                                            normal += new Vector3(
                                                xx,
                                                yy,
                                                zz
                                            );
                                        }
                                    }

                            normal.Normalize();
                            break;

                        case NPVoxNormalMode.FORWARD: normal = Vector3.forward; break;
                        case NPVoxNormalMode.BACK: normal = Vector3.back; break;
                        case NPVoxNormalMode.UP: normal = Vector3.up; break;
                        case NPVoxNormalMode.DOWN: normal = Vector3.down; break;
                        case NPVoxNormalMode.LEFT: normal = Vector3.left; break;
                        case NPVoxNormalMode.RIGHT: normal = Vector3.right; break;

                    }

                    normals[currentVertexIndex + t] = (normal + variance).normalized;

                    // store voxel center for shader usage
                    Vector4 tangent = new Vector4();
                    tangent.x = voxelCenter.x;
                    tangent.y = voxelCenter.y;
                    tangent.z = voxelCenter.z;
                    // encode model size
                    tangent.w = ((voxelsToInclude.Size.X & 0x7F) << 14) | ((voxelsToInclude.Size.Y & 0x7F) << 7) | (voxelsToInclude.Size.Z & 0x7F);


                    tangents[currentVertexIndex + t] = tangent;
                }

                // UVs
                for (int t = 0; t < numVertices; t++)
                {
                    colors[currentVertexIndex + t] = color;
                }

                // translate & scale vertices to voxel position
                for (int t = 0; t < numVertices; t++)
                {
                    vertices[currentVertexIndex + t] = voxelCenter + Vector3.Scale(vertexPositionOffsets[t], cubeSize);
                }

                currentTriangleIndex[vertexGroupIndex] = i;
                currentVertexIndex += numVertices;
            }
        }

        // shrink arrays as needed
        if (optimization != NPVoxOptimization.OFF)
        {
            Array.Resize(ref vertices, currentVertexIndex);
            Array.Resize(ref normals, currentVertexIndex);
            Array.Resize(ref tangents, currentVertexIndex);
            Array.Resize(ref colors, currentVertexIndex);
        }

        mesh.vertices = vertices;
        if (hasVoxelGroups)
        {
            mesh.subMeshCount = Math.Max(vertexGroupCount, MinVertexGroups);
            for (int i = 0; i < vertexGroupCount; i++)
            {
                int numberOfTrianglesForVertexGroup = currentTriangleIndex[i];
                int[] trianglesForVertexGroup = new int[numberOfTrianglesForVertexGroup];
                for (int j = 0; j < numberOfTrianglesForVertexGroup; j++)
                {
                    trianglesForVertexGroup[j] = triangles[i, j];
                }
                mesh.SetTriangles(trianglesForVertexGroup, i);
            }
        }
        else
        {
            int numberOfTrianglesForVertexGroup = currentTriangleIndex[0];
            int[] trianglesForVertexGroup = new int[numberOfTrianglesForVertexGroup];
            Buffer.BlockCopy(triangles, 0,
                             trianglesForVertexGroup, 0,
                             numberOfTrianglesForVertexGroup * sizeof(int));

            if (MinVertexGroups < 2)
            {
                mesh.triangles = trianglesForVertexGroup;
            }
            else
            {
                mesh.subMeshCount = MinVertexGroups;
                mesh.SetTriangles(trianglesForVertexGroup, 0);
            }
        }



        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.colors = colors;
        mesh.bounds = new Bounds(Vector3.zero, size);

        if (NormalMode == NPVoxNormalMode.AUTO)
        {
            mesh.RecalculateNormals();
        }
    }
}