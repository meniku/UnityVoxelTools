using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets;

[ExecuteInEditMode]
public class NPVoxNormalProcessorPreviewContext : ScriptableObject
{
    public NPVoxMeshOutput MeshOutput           { get; private set; }
    public NPVoxNormalProcessor ViewedProcessor { get; private set; }
    public GameObject PreviewObject             { get; private set; }
    public Mesh PreviewMesh                     { get; private set; }
    public bool IsValid                         { get; private set; }

    public NPVoxToUnity VoxToUnity              { get; private set; }

    public Camera m_camera = null;

    void OnEnable()
    {
        MeshOutput = null;
        ViewedProcessor = null;
        PreviewObject = null;
        IsValid = false;
    }

    public void Invalidate()
    {
        //DestroyImmediate( PreviewMesh );
        GameObject.DestroyImmediate( PreviewObject );
        MeshOutput = null;
        ViewedProcessor = null;
        PreviewObject = null;
        IsValid = false;
    }

    public void Set( NPVoxMeshOutput _meshOutput, NPVoxNormalProcessor _processor )
    {
        MeshOutput = _meshOutput;
        ViewedProcessor = _processor;
        PreviewObject = MeshOutput.Instatiate();

        VoxToUnity = new NPVoxToUnity( MeshOutput.GetVoxModel(), MeshOutput.VoxelSize );

        MeshFilter mf = PreviewObject.GetComponent<MeshFilter>();
        PreviewMesh = Mesh.Instantiate<Mesh>( mf.sharedMesh );
        mf.sharedMesh = PreviewMesh;

        PreviewObject.hideFlags = HideFlags.HideAndDontSave;
        PreviewObject.SetActive( false );
        IsValid = true;
    }
}

public abstract class NPVoxNormalProcessorPass
{
    public abstract void Process( NPVoxModel model, NPVoxMeshData tempdata, Vector3[] inNormals, ref Vector3[] outNormals );
}

[System.Serializable]
public abstract class NPVoxNormalProcessor : ScriptableObject, ICloneable
{
    protected readonly float GUITabWidth = 40.0f;
    
    protected Vector3[] m_normalOutput;
    
    protected List<NPVoxNormalProcessorPass> m_passes = null;

    [SerializeField]
    protected List<int> m_voxelGroupFilter;
    
    [NonSerialized]
    protected List<NPVoxNormalProcessorPreviewContext> m_validPreviewContexts;

    public List<NPVoxNormalProcessorPass> Passes
    {
        get { return m_passes; }
        set { m_passes = value; }
    }

    public NPVoxNormalProcessor()
    {
    }
    
    protected abstract void PerModelInit();

    protected abstract void OneTimeInit();

    public void OnEnable()
    {
        if ( m_voxelGroupFilter == null )
        {
            m_voxelGroupFilter = new List<int>();
        }
        
        m_passes = new List<NPVoxNormalProcessorPass>();

        m_validPreviewContexts = new List<NPVoxNormalProcessorPreviewContext>();

        OneTimeInit();
    }

    public void InitOutputBuffer( int length )
    {
        m_normalOutput = new Vector3[ length ];
    }

    public void Process( NPVoxModel model, NPVoxMeshData[] tempdata, Vector3[] inNormals, Vector3[] outNormals)
    {
        if ( m_normalOutput == null || (m_normalOutput.Length != inNormals.Length ) )
        {
            Debug.LogWarning("NPVox: Normal Processor of Type '" + GetType().ToString() + "': Output Buffer has not been initialized!");
            inNormals.CopyTo(outNormals, 0);
            return;
        }

        inNormals.CopyTo(m_normalOutput, 0);

        Vector3[] normalBuffer = new Vector3[inNormals.Length];

        PerModelInit();

        if ( m_passes.Count == 0 )
        {
            Debug.LogError( "NPVox: Normal Processor '" + GetType().ToString() + "' does not contain any passes!" );
        }

        foreach ( NPVoxNormalProcessorPass pass in m_passes )
        {
            foreach (NPVoxMeshData data in tempdata)
            {
                if (data.AppliesToVoxelGroup(m_voxelGroupFilter.ToArray()))
                {
                    pass.Process(model, data, m_normalOutput, ref normalBuffer);
                    for( int i = 0; i < data.numVertices; i++ )
                    {
                        m_normalOutput[data.vertexIndexOffsetBegin + i] = normalBuffer[data.vertexIndexOffsetBegin + i];
                    }
                }
            }
        }

        m_normalOutput.CopyTo( outNormals, 0 );
    }

    public void OnDestroy()
    {
        m_passes.Clear();

        foreach( NPVoxNormalProcessorPreviewContext context in m_validPreviewContexts )
        {
            context.Invalidate();
        }

        ClearInvalidPreviewContexts();
    }
    
    public void OnGUI()
    {
        if ( m_voxelGroupFilter == null )
        {
            m_voxelGroupFilter = new List<int>();
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(300.0f), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Voxel Group Filters", GUILayout.Width(150.0f), GUILayout.ExpandWidth(true));
        int oldSize = m_voxelGroupFilter.Count;
        int newSize = EditorGUILayout.IntField(oldSize, GUILayout.Width(40.0f), GUILayout.ExpandWidth(false));
        newSize = Math.Max(0, newSize);
        if ( newSize > oldSize)
        {
            for ( int i = 0; i < newSize - oldSize; i++ )
            {
                m_voxelGroupFilter.Add(0);
            }
        }
        else if ( newSize < oldSize)
        {
            for (int i = 0; i < oldSize - newSize; i++)
            {
                m_voxelGroupFilter.RemoveAt(m_voxelGroupFilter.Count - 1);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel += 1;
        for ( int i = 0; i < m_voxelGroupFilter.Count; i++ )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(300.0f), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Index " + i, GUILayout.Width(150.0f), GUILayout.ExpandWidth(true));
            m_voxelGroupFilter[i] = EditorGUILayout.IntField(m_voxelGroupFilter[i], GUILayout.Width(40.0f), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel -= 1;

        OnGUIInternal();
    }

    protected abstract void OnGUIInternal();

    protected PASS_TYPE AddPass<PASS_TYPE>() where PASS_TYPE : NPVoxNormalProcessorPass, new()
    {
        PASS_TYPE pass = new PASS_TYPE();

        m_passes.Add( pass );
        
        return pass;
    }
    
    public Vector3[] GetNormalOutput()
    {
        return m_normalOutput;
    }

    public void AddVoxelGroupFilter( int index )
    {
        if ( m_voxelGroupFilter == null )
        {
            m_voxelGroupFilter = new List<int>();
        }

        if (index >= 0)
        {
            m_voxelGroupFilter.Add(index);
        }
    }

    public void ClearVoxelGroupFilters()
    {
        m_voxelGroupFilter.Clear();
    }

    public void AddToAsset( string path )
    {
        if (path.Length > 0)
        {
            UnityEditor.AssetDatabase.AddObjectToAsset(this, path);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    public abstract object Clone();

    public NPVoxNormalProcessorPreviewContext GeneratePreviewContext( NPVoxMeshOutput _meshOutput )
    {
        ClearInvalidPreviewContexts();
        NPVoxNormalProcessorPreviewContext previewContext = ScriptableObject.CreateInstance< NPVoxNormalProcessorPreviewContext >();
        previewContext.Set( _meshOutput, this );
        m_validPreviewContexts.Add( previewContext );
        return previewContext;
    }

    public void ClearInvalidPreviewContexts()
    {
        NPVoxNormalProcessorPreviewContext[] contexts = m_validPreviewContexts.ToArray();
        foreach ( NPVoxNormalProcessorPreviewContext context in contexts )
        {
            if ( !context.IsValid )
            {
                m_validPreviewContexts.Remove( context );
            }
        }
    }

    public virtual void OnListChanged( NPVoxNormalProcessorList _list )
    {
        m_normalOutput = null;

        NPVoxNormalProcessorPreviewContext[] contexts = m_validPreviewContexts.ToArray();
        foreach ( NPVoxNormalProcessorPreviewContext context in contexts )
        {
            context.Invalidate();
        }

        ClearInvalidPreviewContexts();
    }

    public bool IsOutputValid()
    {
        return ( m_normalOutput != null && m_normalOutput.Length > 0 );
    }

    public Vector3[] GetOutput()
    {
        return m_normalOutput;
    }

    public Vector3[] GetOutputCopy()
    {
        Vector3[] copy = new Vector3[ m_normalOutput.Length ];
        m_normalOutput.CopyTo( copy, 0 );
        return copy;
    }
}
