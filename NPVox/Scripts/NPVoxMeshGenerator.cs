using UnityEngine;
using System;

public class NPVoxMeshTempData
{
    public NPVoxCoord voxCoord = NPVoxCoord.INVALID;

    public bool hasLeft = false;
    public bool hasRight = false;
    public bool hasUp = false;
    public bool hasDown = false;
    public bool hasForward = false;
    public bool hasBack = false;

    public bool isHidden = true;

    public bool includeLeft = false;
    public bool includeRight = false;
    public bool includeUp = false;
    public bool includeDown = false;
    public bool includeBack = false;
    public bool includeForward = false;

    public int numVertices = 0;
    public int vertexIndexOffsetBegin = 0;
    public int vertexGroupIndex = 0;

    public Vector3 voxelCenter = Vector3.zero;
    public int[] vertexIndexOffsets = new int[ 8 ];
    public Vector3[] vertexPositionOffsets = new Vector3[ 8 ];

    public NPVoxNormalMode normalMode = NPVoxNormalMode.SMOOTH;

    public NPVoxMeshTempData()
    {
    }
}

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

        var tmp = new NPVoxMeshTempData[ model.NumVoxels ];

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

        NPVoxBox voxelNormalNeighbours = new NPVoxBox( new NPVoxCoord( -1, -1, -1 ), new NPVoxCoord( 1, 1, 1 ) );

        // Collect temporary data to use for model generation
        int voxIndex = 0;
        foreach (NPVoxCoord voxCoord in voxelsToInclude.Enumerate())
        {
            if (model.HasVoxel(voxCoord))
            {
                // Compute voxel center
                tmp[ voxIndex ] = new NPVoxMeshTempData();
                tmp[ voxIndex ].voxelCenter = npVoxToUnity.ToUnityPosition(voxCoord) + cutoutOffset;
                tmp[ voxIndex ].voxCoord = voxCoord;

                // Determine vertex group index
                tmp[ voxIndex ].vertexGroupIndex = 0;
                if (hasVoxelGroups)
                {
                    tmp[ voxIndex ].vertexGroupIndex = model.GetVoxelGroup(voxCoord);
                }

                // Determine normal Mode
                if (NormalModePerVoxelGroup != null && NormalModePerVoxelGroup.Length > tmp[ voxIndex ].vertexGroupIndex )
                {
                    tmp[ voxIndex ].normalMode = NormalModePerVoxelGroup[ tmp[ voxIndex ].vertexGroupIndex ];
                }
                else
                {
                    tmp[ voxIndex ].normalMode = NormalMode;
                }

                // do we have this side
                tmp[ voxIndex ].hasLeft = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.LEFT, loop));
                tmp[ voxIndex ].hasRight = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.RIGHT, loop));
                tmp[ voxIndex ].hasDown = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.DOWN, loop));
                tmp[ voxIndex ].hasUp = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.UP, loop));
                tmp[ voxIndex ].hasForward = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.FORWARD, loop));
                tmp[ voxIndex ].hasBack = !model.HasVoxel(model.LoopCoord(voxCoord + NPVoxCoord.BACK, loop));

                // do we actually want to include this side in our mesh
                // NOTE: cutout < 0 means we still render the mesh even though it is cutout
                //       cutout > 0 means we don't render the mesh when cutout
                tmp[ voxIndex ].includeLeft = ( tmp[ voxIndex ].hasLeft || (cutout.Left < 0 && voxCoord.X == voxelsToInclude.Left)) && include.Left == 1;
                tmp[ voxIndex ].includeRight = ( tmp[ voxIndex ].hasRight || (cutout.Right < 0 && voxCoord.X == voxelsToInclude.Right)) && include.Right == 1;
                tmp[ voxIndex ].includeUp = ( tmp[ voxIndex ].hasUp || (cutout.Up < 0 && voxCoord.Y == voxelsToInclude.Up)) && include.Up == 1;
                tmp[ voxIndex ].includeDown = ( tmp[ voxIndex ].hasDown || (cutout.Down < 0 && voxCoord.Y == voxelsToInclude.Down)) && include.Down == 1;
                tmp[ voxIndex ].includeBack = ( tmp[ voxIndex ].hasBack || (cutout.Back < 0 && voxCoord.Z == voxelsToInclude.Back)) && include.Back == 1;
                tmp[ voxIndex ].includeForward = ( tmp[ voxIndex ].hasForward || (cutout.Forward < 0 && voxCoord.Z == voxelsToInclude.Forward)) && include.Forward == 1;

                tmp[ voxIndex ].isHidden = !tmp[ voxIndex ].hasForward && 
                                !tmp[ voxIndex ].hasBack && 
                                !tmp[ voxIndex ].hasLeft && 
                                !tmp[ voxIndex ].hasRight && 
                                !tmp[ voxIndex ].hasUp && 
                                !tmp[ voxIndex ].hasDown;


                if ( tmp[ voxIndex ].isHidden && optimization == NPVoxOptimization.PER_VOXEL)
                {
                    continue;
                }

                if ( tmp[ voxIndex ].isHidden && BloodColorIndex > 0)
                {
                    model.SetVoxel(voxCoord, (byte)BloodColorIndex); // WTF WTF WTF?!? we should not modify the MODEL in here !!!!           elfapo: AAAAHHH NOOOO!!!! :O    j.k. ;)
                }

                Color color = model.GetColor(voxCoord);

                // prepare cube vertices
                tmp[ voxIndex ].numVertices = 0;
                tmp[ voxIndex ].vertexIndexOffsetBegin = currentVertexIndex;

                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeBack || tmp[ voxIndex ].includeLeft || tmp[ voxIndex ].includeDown )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[0] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[0]] = new Vector3(-0.5f, -0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeBack || tmp[ voxIndex ].includeRight || tmp[ voxIndex ].includeDown )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[1] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[1]] = new Vector3(0.5f, -0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeBack || tmp[ voxIndex ].includeLeft || tmp[ voxIndex ].includeUp )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[2] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[2]] = new Vector3(-0.5f, 0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeBack || tmp[ voxIndex ].includeRight || tmp[ voxIndex ].includeUp )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[3] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[3]] = new Vector3(0.5f, 0.5f, -0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeForward || tmp[ voxIndex ].includeLeft || tmp[ voxIndex ].includeDown )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[4] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[4]] = new Vector3(-0.5f, -0.5f, 0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeForward || tmp[ voxIndex ].includeRight || tmp[ voxIndex ].includeDown )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[5] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[5]] = new Vector3(0.5f, -0.5f, 0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeForward || tmp[ voxIndex ].includeLeft || tmp[ voxIndex ].includeUp )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[6] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[6]] = new Vector3(-0.5f, 0.5f, 0.5f);
                }
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeForward || tmp[ voxIndex ].includeRight || tmp[ voxIndex ].includeUp )
                {
                    tmp[ voxIndex ].vertexIndexOffsets[7] = tmp[ voxIndex ].numVertices++;
                    tmp[ voxIndex ].vertexPositionOffsets[ tmp[ voxIndex ].vertexIndexOffsets[7]] = new Vector3(0.5f, 0.5f, 0.5f);
                }


                // add cube faces
                int i = currentTriangleIndex[ tmp[ voxIndex ].vertexGroupIndex ];

                // back
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeBack )
                {
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[0];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[2];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[1];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[2];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[3];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[1];
                }

                // Forward
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeForward )
                {
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[6];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[4];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[5];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[7];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[6];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[5];
                }

                // right
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeRight )
                {
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[1];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[3];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[5];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[3];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[7];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[5];
                }

                // left
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeLeft )
                {
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[0];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[4];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[2];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[2];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[4];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[6];
                }

                // up
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeUp )
                {
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[2];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[6];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[3];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[3];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[6];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[7];
                }

                // down
                if (optimization != NPVoxOptimization.PER_FACE || tmp[ voxIndex ].includeDown )
                {
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[0];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[1];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[4];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[1];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[5];
                    triangles[ tmp[ voxIndex ].vertexGroupIndex, i++] = tmp[ voxIndex ].vertexIndexOffsetBegin + tmp[ voxIndex ].vertexIndexOffsets[4];
                }
                

                // Tangents
                for ( int t = 0; t < tmp[ voxIndex ].numVertices; t++)
                {
                    // store voxel center for shader usage
                    Vector4 tangent = new Vector4();
                    tangent.x = tmp[ voxIndex ].voxelCenter.x;
                    tangent.y = tmp[ voxIndex ].voxelCenter.y;
                    tangent.z = tmp[ voxIndex ].voxelCenter.z;
                    // encode model size
                    tangent.w = ((voxelsToInclude.Size.X & 0x7F) << 14) | ((voxelsToInclude.Size.Y & 0x7F) << 7) | (voxelsToInclude.Size.Z & 0x7F);

                    tangents[ tmp[ voxIndex ].vertexIndexOffsetBegin + t] = tangent;
                }

                // UVs
                for (int t = 0; t < tmp[ voxIndex ].numVertices; t++)
                {
                    colors[ tmp[ voxIndex ].vertexIndexOffsetBegin + t] = color;
                }

                // translate & scale vertices to voxel position
                for (int t = 0; t < tmp[ voxIndex ].numVertices; t++)
                {
                    vertices[ tmp[ voxIndex ].vertexIndexOffsetBegin + t] = tmp[ voxIndex ].voxelCenter + Vector3.Scale( tmp[ voxIndex ].vertexPositionOffsets[t], cubeSize);
                }

                currentTriangleIndex[ tmp[ voxIndex ].vertexGroupIndex ] = i;
                currentVertexIndex += tmp[ voxIndex ].numVertices;
                voxIndex++;
            }
        }
        
        // elfapo: Remove invalid voxel information
        Array.Resize( ref tmp, voxIndex );

        ////////////////////////////////////// NORMAL STAGES ////////////////////////////////////////
        // elfapo TODO: Move Normal stages to Normal Processor Pipeline: 

        foreach ( NPVoxMeshTempData data in tmp )
        { 
            // calculate normals based on present neighbour voxels
            Vector3 voxelNormal = Vector3.zero;
            if ( !data.isHidden )
            {
                foreach ( NPVoxCoord offset in voxelNormalNeighbours.Enumerate() )
                {
                    NPVoxCoord checkCoord = data.voxCoord + offset;
                    checkCoord = model.LoopCoord( checkCoord, loop );
                    if ( !model.HasVoxel( checkCoord ) )
                    {
                        voxelNormal += NPVoxCoordUtil.ToVector( offset );
                    }
                }
                voxelNormal.Normalize();
            }
            else
            {
                voxelNormal = data.voxelCenter.normalized;
            }

            for ( int t = 0; t < data.numVertices; t++ )
            {
                Vector3 normal = Vector3.zero;

                switch ( data.normalMode )
                {
                    case NPVoxNormalMode.VOXEL:
                        normal = voxelNormal;

                        normal = Vector3.zero;

                        if ( data.vertexPositionOffsets[ t ].x < 0.0f )
                        {
                            if ( data.hasLeft && !data.hasForward && !data.hasBack && !data.hasUp && !data.hasDown )
                            {
                                normal.x = -1;
                            }
                            else
                            {
                                normal.x = voxelNormal.x;
                            }
                        }
                        else if ( data.vertexPositionOffsets[ t ].x > 0.0f )
                        {
                            if ( data.hasRight && !data.hasForward && !data.hasBack && !data.hasUp && !data.hasDown )
                            {
                                normal.x = 1;
                            }
                            else
                            {
                                normal.x = voxelNormal.x;
                            }
                        }

                        if ( data.vertexPositionOffsets[ t ].y < 0.0f )
                        {
                            if ( data.hasUp && !data.hasForward && !data.hasBack && !data.hasLeft && !data.hasRight )
                            {
                                normal.y = -1;
                            }
                            else
                            {
                                normal.y = voxelNormal.y;
                            }
                        }
                        else if ( data.vertexPositionOffsets[ t ].y > 0.0f )
                        {
                            if ( data.hasDown && !data.hasForward && !data.hasBack && !data.hasLeft && !data.hasRight )
                            {
                                normal.y = +1;
                            }
                            else
                            {
                                normal.y = voxelNormal.y;
                            }
                        }

                        if ( data.vertexPositionOffsets[ t ].z < 0.0f )
                        {
                            if ( data.hasBack && !data.hasLeft && !data.hasRight && !data.hasUp && !data.hasDown )
                            {
                                normal.z = -1;
                            }
                            else
                            {
                                normal.z = voxelNormal.z;
                            }
                        }
                        else if ( data.vertexPositionOffsets[ t ].z > 0.0f )
                        {
                            if ( data.hasForward && !data.hasLeft && !data.hasRight && !data.hasUp && !data.hasDown )
                            {
                                normal.z = +1;
                            }
                            else
                            {
                                normal.z = voxelNormal.z;
                            }
                        }

                        if ( Mathf.Abs( normal.x ) < 0.1f && Mathf.Abs( normal.y ) < 0.1f && Mathf.Abs( normal.z ) < 0.1f )
                        {
                            // we would like to have full color when we are a stand-alone voxel, however there is no way to do so right now, so we just
                            // fallback to the centoid normal
                            normal = data.voxelCenter;
                        }

                        normal.Normalize();
                        break;

                    case NPVoxNormalMode.SMOOTH:
                        normal = Vector3.zero;

                        for ( float xx = -0.5f; xx < 1.0f; xx += 1f )
                            for ( float yy = -.5f; yy < 1; yy += 1 )
                                for ( float zz = -.5f; zz < 1; zz += 1 )
                                {
                                    sbyte xCoord = ( sbyte ) Mathf.Round( data.vertexPositionOffsets[ t ].x + xx );
                                    sbyte yCoord = ( sbyte ) Mathf.Round( data.vertexPositionOffsets[ t ].y + yy );
                                    sbyte zCoord = ( sbyte ) Mathf.Round( data.vertexPositionOffsets[ t ].z + zz );

                                    if ( !model.HasVoxel( data.voxCoord + new NPVoxCoord( ( sbyte ) xCoord, ( sbyte ) yCoord, ( sbyte ) zCoord ) ) )
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

                normals[ data.vertexIndexOffsetBegin + t ] = normal;
            }
        }



        ////////////////////////////////////// NORMAL STAGES ////////////////////////////////////////
        // elfapo TODO: Test area 'Normal Processor' Move Normal Processor stages to Normal Processor Pipeline: 

        NPVoxNormalProcessor_Variance processor = new NPVoxNormalProcessor_Variance();
        processor.NormalVariance = NormalVariance;
        processor.NormalVarianceSeed = NormalVarianceSeed;
        processor.OneTimeInit();
        processor.Process( model, tmp, normals, out normals );

        ////////////////////////////////////// NORMAL STAGES ////////////////////////////////////////


        // shrink arrays as needed
        if ( optimization != NPVoxOptimization.OFF)
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