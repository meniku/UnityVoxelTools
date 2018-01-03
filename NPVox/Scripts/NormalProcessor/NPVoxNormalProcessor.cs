using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public abstract class NPVoxNormalProcessorPass : ScriptableObject
{
    public abstract void Process( NPVoxModel model, NPVoxMeshTempData tempdata, Vector3[] inNormals, ref Vector3[] outNormals );
    
    public bool IsEnabled { get; set; }

    public NPVoxNormalProcessorPass()
    {
        IsEnabled = true;
    }
}

[System.Serializable]
public abstract class NPVoxNormalProcessor : ScriptableObject
{
    protected Vector3[] m_normalOutput;

    public List<NPVoxNormalProcessorPass> m_passes = null;

    protected readonly float GUITabWidth = 40.0f;

    [SerializeField]
    public List<int> m_voxelGroupFilter;
    
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

        if ( m_passes == null )
        {
            m_passes = new List<NPVoxNormalProcessorPass>();
            OneTimeInit();
        }
    }

    public void InitOutputBuffer( Vector3[] inNormals )
    {
        m_normalOutput = new Vector3[inNormals.Length];
    }

    public void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, Vector3[] outNormals)
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
            if ( pass.IsEnabled )
            {
                foreach (NPVoxMeshTempData data in tempdata)
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
        }

        m_normalOutput.CopyTo( outNormals, 0 );
    }

    public void OnDestroy()
    {
        foreach( NPVoxNormalProcessorPass pass in m_passes )
        {
            ScriptableObject.DestroyImmediate( pass, true );
        }

        m_passes.Clear();
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

    protected PASS_TYPE AddPass<PASS_TYPE>() where PASS_TYPE : NPVoxNormalProcessorPass
    {
        PASS_TYPE pass = ScriptableObject.CreateInstance<PASS_TYPE>();

        m_passes.Add( pass );

        pass.hideFlags = HideFlags.HideInHierarchy;

        string path = UnityEditor.AssetDatabase.GetAssetPath( this );
        if ( path.Length > 0 )
        {
            UnityEditor.AssetDatabase.AddObjectToAsset( pass, path );
            UnityEditor.EditorUtility.SetDirty( pass );
        }

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
}
