using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public abstract class NPVoxNormalProcessorPass : ScriptableObject
{
    protected Vector3[] m_normalBuffer;

    public abstract void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, out Vector3[] outNormals );
    
    public bool IsEnabled { get; set; }

    public NPVoxNormalProcessorPass()
    {
        IsEnabled = true;
    }

    public Vector3[] GetNormalBuffer()
    {
        return m_normalBuffer;
    }
}

[System.Serializable]
public abstract class NPVoxNormalProcessor : ScriptableObject
{
    protected readonly float GUITabWidth = 40.0f;

    protected List<NPVoxNormalProcessorPass> m_passes = null;

    public NPVoxNormalProcessor()
    {
    }
    
    protected abstract void PerModelInit();

    protected abstract void OneTimeInit();

    public void OnEnable()
    {
        if ( m_passes == null )
        {
            m_passes = new List<NPVoxNormalProcessorPass>();
            OneTimeInit();
        }
    }

    public void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, out Vector3[] outNormals )
    {
        outNormals = null;

        PerModelInit();

        if ( m_passes.Count == 0 )
        {
            Debug.LogError( "NPVox: Normal Processor '" + GetType().ToString() + "' does not contain any passes!" );
        }

        foreach ( NPVoxNormalProcessorPass pass in m_passes )
        {
            if ( pass.IsEnabled )
            {
                pass.Process( model, tempdata, inNormals, out outNormals );
                inNormals = outNormals;
            }
        }

        if ( outNormals == null )
        {
            outNormals = inNormals;
        }
    }

    public void OnDestroy()
    {
        foreach( NPVoxNormalProcessorPass pass in m_passes )
        {
            ScriptableObject.DestroyImmediate( pass, true );
        }

        m_passes.Clear();
    }

    public virtual void OnGUI()
    {
    }

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
}
