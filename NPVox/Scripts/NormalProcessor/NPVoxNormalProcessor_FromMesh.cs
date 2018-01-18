using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathUtilites;

class NPVoxNormalProcessorPass_FromMesh : NPVoxNormalProcessorPass
{
    public Mesh meshReference = null;
    public Vector3 scale = Vector3.zero;
    public Vector3 offset = Vector3.zero;
    List<Vector3> meshVertices = new List<Vector3>();
    List<Vector3> meshNormals = new List<Vector3>();
    int[] triangleList = null;

    public bool dataAlreadyCollected = false;

    public override void Process( NPVoxModel model, NPVoxMeshTempData tempdata, Vector3[] inNormals, ref Vector3[] outNormals )
    {
        if (meshReference != null)
        {
            Vector3 sizeVoxel = tempdata.voxToUnity.VoxeSize;
            if ( !dataAlreadyCollected )
            {
                Vector3 sizeVoxModel = tempdata.voxToUnity.UnityVoxModelSize;
                NPVoxBox boundsVoxModel = model.BoundingBox;
                Bounds boundsMesh = meshReference.bounds;
                
                offset = boundsMesh.min;
                
                scale = new Vector3(
                    boundsMesh.size.x / sizeVoxModel.x,
                    boundsMesh.size.y / sizeVoxModel.z,
                    boundsMesh.size.z / sizeVoxModel.y 
                    );
                
                meshReference.GetNormals( meshNormals );

                meshReference.GetVertices( meshVertices );
                triangleList = meshReference.GetTriangles( 0 );
            }

            Vector3 x = new Vector3(
                (tempdata.voxCoord.X + 0.5f) * sizeVoxel.x,
                (tempdata.voxCoord.Y + 0.5f ) * sizeVoxel.y,
                (tempdata.voxCoord.Z + 0.5f ) * sizeVoxel.z
                );

            x = new Vector3(
                x.x * scale.x,
                x.z * scale.z,
                x.y * scale.y
                );

            x += offset;

            int bestTriangle = -1;
            float bestDistanceSquared = float.PositiveInfinity;
            Vector3 bestNormal = Vector3.zero;
            Vector3 bestPoint = Vector3.zero;

            for ( int triangle = 0; triangle < triangleList.Length; triangle += 3 )
            {
                Vector3 v1 = meshVertices[ triangleList[ triangle + 0 ] ];
                Vector3 v2 = meshVertices[ triangleList[ triangle + 1 ] ];
                Vector3 v3 = meshVertices[ triangleList[ triangle + 2 ] ];

                Bounds boundTest = new Bounds( v1, Vector3.zero );
                boundTest.Encapsulate( v2 );
                boundTest.Encapsulate( v3 );
                boundTest.Expand( 0.1f );

                if ( !boundTest.Contains(x) )
                {
                    continue;
                }

                Vector3 n = LinearAlgebra.ComputePlaneNormal( v1, v2, v3 );

                Vector3 xProjected;
                LinearAlgebra.ProjectPointToPlane( v1, n, x, out xProjected );

                Vector3 xBarycentric = LinearAlgebra.WorldToBarycentric3( v1, v2, v3, xProjected );

                if ( xBarycentric.x < 0.0 )
                {
                     xProjected = LinearAlgebra.ClampToLine( v2, v3, xProjected );
                }

                else if ( xBarycentric.y < 0.0 )
                {
                    xProjected = LinearAlgebra.ClampToLine( v3, v1, xProjected );
                }

                else if ( xBarycentric.z < 0.0 )
                {
                    xProjected = LinearAlgebra.ClampToLine( v1, v2, xProjected );
                }

                float sqaredDistance = ( xProjected - x ).sqrMagnitude;

                if ( !float.IsNaN( sqaredDistance ) && sqaredDistance < bestDistanceSquared )
                {
                    bestDistanceSquared = sqaredDistance;
                    bestTriangle = triangle;
                    bestNormal = n;
                    bestPoint = xProjected;
                }
            }

            Vector3 average = Vector3.zero;

            bool smoothNormals = true;

            if ( bestTriangle != -1 )
            {
                if ( smoothNormals )
                {
                    average = LinearAlgebra.BarycentricToWorld(
                        meshNormals[ triangleList[ bestTriangle + 0 ] ],
                        meshNormals[ triangleList[ bestTriangle + 1 ] ],
                        meshNormals[ triangleList[ bestTriangle + 2 ] ],
                        LinearAlgebra.WorldToBarycentric3(
                            meshVertices[ triangleList[ bestTriangle + 0 ] ],
                            meshVertices[ triangleList[ bestTriangle + 1 ] ],
                            meshVertices[ triangleList[ bestTriangle + 2 ] ],
                            bestPoint )
                        );
                }
                else
                {
                    average = meshNormals[ triangleList[ bestTriangle ] ];
                    //average = new Vector3(
                    //    bestNormal.x,
                    //    bestNormal.z,
                    //    bestNormal.y
                    //    );
                }


                average = new Vector3(
                    average.x / scale.x,
                    average.z / scale.z,
                    average.y / scale.y
                    );
            }

            for ( int t = 0; t < tempdata.numVertices; t++ )
            {
                outNormals[ tempdata.vertexIndexOffsetBegin + t ] = average;
            }
        }
        else
        {
            for ( int t = 0; t < tempdata.numVertices; t++ )
            {
                outNormals[ tempdata.vertexIndexOffsetBegin + t ] = inNormals[ tempdata.vertexIndexOffsetBegin + t ];
            }
        }
    }
}


[NPVoxAttributeNormalProcessorListItem( "From Mesh", typeof( NPVoxNormalProcessor_FromMesh ), NPVoxNormalProcessorType.Generator )]
class NPVoxNormalProcessor_FromMesh : NPVoxNormalProcessor
{
    [SerializeField]
    private Mesh m_meshReference = null;

    private NPVoxNormalProcessorPass_FromMesh m_passFromMesh;

    private NPVoxNormalProcessorPass_Normalize m_passNormalize;

    public override object Clone()
    {
        NPVoxNormalProcessor_FromMesh clone = ScriptableObject.CreateInstance<NPVoxNormalProcessor_FromMesh>();

        clone.m_meshReference = m_meshReference;

        foreach ( int filter in m_voxelGroupFilter )
        {
            clone.m_voxelGroupFilter.Add( filter );
        }

        return clone;
    }

    protected override void OneTimeInit()
    {
        m_passFromMesh = AddPass<NPVoxNormalProcessorPass_FromMesh>();
        m_passNormalize = AddPass<NPVoxNormalProcessorPass_Normalize>();
    }

    protected override void OnGUIInternal()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(GUITabWidth);
        GUILayout.Label("Mesh Input");

        EditorGUILayout.ObjectField( m_meshReference, typeof( Mesh ), false );

        if ( GUILayout.Button( "Select" ) )
        {
            int controlID = EditorGUIUtility.GetControlID( FocusType.Passive );
            EditorGUIUtility.ShowObjectPicker<Mesh>( m_meshReference, false, "", controlID );
        }

        string commandName = Event.current.commandName;
        if ( commandName == "ObjectSelectorUpdated" )
        {
            m_meshReference = EditorGUIUtility.GetObjectPickerObject() as Mesh;
        }

        GUILayout.EndHorizontal();
    }

    protected override void PerModelInit()
    {
        m_passFromMesh.meshReference = m_meshReference;
        m_passFromMesh.dataAlreadyCollected = false;
    }
}
