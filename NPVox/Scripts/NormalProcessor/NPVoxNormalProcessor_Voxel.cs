using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class NPVoxNormalProcessorPass_Voxel : NPVoxNormalProcessorPass
{
    public override void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        NPVoxBox voxelNormalNeighbours = new NPVoxBox( new NPVoxCoord( -1, -1, -1 ), new NPVoxCoord( 1, 1, 1 ) );

        foreach ( NPVoxMeshTempData data in tempdata )
        {
            // calculate normals based on present neighbour voxels
            Vector3 voxelNormal = Vector3.zero;
            if ( !data.isHidden )
            {
                foreach ( NPVoxCoord offset in voxelNormalNeighbours.Enumerate() )
                {
                    NPVoxCoord checkCoord = data.voxCoord + offset;
                    checkCoord = model.LoopCoord( checkCoord, data.loop );
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
                outNormals[ data.vertexIndexOffsetBegin + t ] = normal;
            }

        }
    }
}

[ NPVoxAttributeNormalProcessorListItem( "From Voxel Data", typeof( NPVoxNormalProcessor_Voxel ), NPVoxNormalProcessorType.Generator ) ]
public class NPVoxNormalProcessor_Voxel : NPVoxNormalProcessor
{
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

    protected override void OneTimeInit()
    {
        AddPass<NPVoxNormalProcessorPass_Voxel>();
    }

    protected override void PerModelInit()
    {
    }

    public override void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space( GUITabWidth );
        NormalMode = ( NPVoxNormalMode ) EditorGUILayout.EnumPopup( "Normal Mode", NormalMode );
        GUILayout.EndHorizontal();
    }
}
