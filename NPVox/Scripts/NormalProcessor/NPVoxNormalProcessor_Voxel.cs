using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class NPVoxNormalProcessorPass_Voxel : NPVoxNormalProcessorPass
{
    public NPVoxNormalMode m_normalMode;
    
    private static NPVoxBox voxelNormalNeighbours = new NPVoxBox(new NPVoxCoord(-1, -1, -1), new NPVoxCoord(1, 1, 1));

    public override void Process( NPVoxModel model, NPVoxMeshData tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        // calculate normals based on present neighbour voxels
        Vector3 voxelNormal = Vector3.zero;
        if ( !tempdata.isHidden )
        {
            foreach ( NPVoxCoord offset in voxelNormalNeighbours.Enumerate() )
            {
                NPVoxCoord checkCoord = tempdata.voxCoord + offset;
                checkCoord = model.LoopCoord( checkCoord, tempdata.loop );
                if ( !model.HasVoxel( checkCoord ) )
                {
                    voxelNormal += NPVoxCoordUtil.ToVector( offset );
                }
            }
            voxelNormal.Normalize();
        }
        else
        {
            voxelNormal = tempdata.voxelCenter.normalized;
        }

        for ( int t = 0; t < tempdata.numVertices; t++ )
        {
            Vector3 normal = Vector3.zero;

            switch ( m_normalMode)
            {
                case NPVoxNormalMode.VOXEL:
                    normal = voxelNormal;

                    normal = Vector3.zero;

                    if (tempdata.vertexPositionOffsets[ t ].x < 0.0f )
                    {
                        if (tempdata.hasLeft && !tempdata.hasForward && !tempdata.hasBack && !tempdata.hasUp && !tempdata.hasDown )
                        {
                            normal.x = -1;
                        }
                        else
                        {
                            normal.x = voxelNormal.x;
                        }
                    }
                    else if (tempdata.vertexPositionOffsets[ t ].x > 0.0f )
                    {
                        if (tempdata.hasRight && !tempdata.hasForward && !tempdata.hasBack && !tempdata.hasUp && !tempdata.hasDown )
                        {
                            normal.x = 1;
                        }
                        else
                        {
                            normal.x = voxelNormal.x;
                        }
                    }

                    if (tempdata.vertexPositionOffsets[ t ].y < 0.0f )
                    {
                        if (tempdata.hasUp && !tempdata.hasForward && !tempdata.hasBack && !tempdata.hasLeft && !tempdata.hasRight )
                        {
                            normal.y = -1;
                        }
                        else
                        {
                            normal.y = voxelNormal.y;
                        }
                    }
                    else if (tempdata.vertexPositionOffsets[ t ].y > 0.0f )
                    {
                        if (tempdata.hasDown && !tempdata.hasForward && !tempdata.hasBack && !tempdata.hasLeft && !tempdata.hasRight )
                        {
                            normal.y = +1;
                        }
                        else
                        {
                            normal.y = voxelNormal.y;
                        }
                    }

                    if (tempdata.vertexPositionOffsets[ t ].z < 0.0f )
                    {
                        if (tempdata.hasBack && !tempdata.hasLeft && !tempdata.hasRight && !tempdata.hasUp && !tempdata.hasDown )
                        {
                            normal.z = -1;
                        }
                        else
                        {
                            normal.z = voxelNormal.z;
                        }
                    }
                    else if (tempdata.vertexPositionOffsets[ t ].z > 0.0f )
                    {
                        if (tempdata.hasForward && !tempdata.hasLeft && !tempdata.hasRight && !tempdata.hasUp && !tempdata.hasDown )
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
                        normal = tempdata.voxelCenter;
                    }

                    normal.Normalize();
                    break;

                case NPVoxNormalMode.SMOOTH:
                    normal = Vector3.zero;

                    for ( float xx = -0.5f; xx < 1.0f; xx += 1f )
                        for ( float yy = -.5f; yy < 1; yy += 1 )
                            for ( float zz = -.5f; zz < 1; zz += 1 )
                            {
                                sbyte xCoord = ( sbyte ) Mathf.Round(tempdata.vertexPositionOffsets[ t ].x + xx);
                                sbyte yCoord = ( sbyte ) Mathf.Round(tempdata.vertexPositionOffsets[ t ].y + yy);
                                sbyte zCoord = ( sbyte ) Mathf.Round(tempdata.vertexPositionOffsets[ t ].z + zz);

                                if ( !model.HasVoxel(tempdata.voxCoord + new NPVoxCoord(( sbyte ) xCoord, ( sbyte ) yCoord, ( sbyte ) zCoord )))
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
            outNormals[tempdata.vertexIndexOffsetBegin + t] = normal;
        }
    }
}

[ NPVoxAttributeNormalProcessorListItem( "From Voxel Data", typeof( NPVoxNormalProcessor_Voxel ), NPVoxNormalProcessorType.Generator ) ]
public class NPVoxNormalProcessor_Voxel : NPVoxNormalProcessor
{
    [SerializeField]
    private NPVoxNormalProcessorPass_Voxel m_passVoxel;

    [SerializeField]
    private NPVoxNormalMode m_normalMode;

    // Processor parameters
    public NPVoxNormalMode NormalMode
    {
        get { return m_normalMode; }
        set
        {
            m_normalMode = value;
        }
    }
    
    public NPVoxNormalProcessor_Voxel()
    {
    }

    public override object Clone()
    {
        NPVoxNormalProcessor_Voxel clone = ScriptableObject.CreateInstance<NPVoxNormalProcessor_Voxel>();
        clone.m_normalMode = m_normalMode;
        clone.m_passVoxel.m_normalMode = m_passVoxel.m_normalMode;

        foreach (int filter in m_voxelGroupFilter)
        {
            clone.m_voxelGroupFilter.Add(filter);
        }

        return clone;
    }

    protected override void OneTimeInit()
    {
        m_passVoxel = AddPass<NPVoxNormalProcessorPass_Voxel>();
    }

    protected override void OnBeforeProcess(NPVoxModel model, NPVoxMeshData[] tempdata)
    {
        m_passVoxel.m_normalMode = NormalMode;
    }

    protected override void OnGUIInternal()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space( GUITabWidth );
        NormalMode = ( NPVoxNormalMode ) EditorGUILayout.EnumPopup( "Normal Mode", NormalMode );
        GUILayout.EndHorizontal();
    }
}
