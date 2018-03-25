using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class NPVoxNormalProcessorList : ScriptableObject, ICloneable, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<NPVoxNormalProcessor> m_processorList = null;

    [HideInInspector]
    public bool RequiresMigration = true;

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
    }

    public void OnEnable()
    {
        if ( m_processorList == null )
        {
            m_processorList = new List<NPVoxNormalProcessor>();
        }
        else
        {
            //Debug.Log( "NPVoxNormalProcessorList in object '" + UnityEditor.AssetDatabase.GetAssetPath( this ) + "' is already initialized" );
        }
    }

    public void AddToAsset( string path )
    {
        hideFlags = HideFlags.HideInHierarchy;

        UnityEditor.AssetDatabase.AddObjectToAsset(this, path);

        foreach ( NPVoxNormalProcessor processor in m_processorList )
        {
            processor.AddToAsset(path);
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }

    public NPVoxNormalProcessor AddProcessor( Type processorType )
    {
        NPVoxNormalProcessor newProcessor = ScriptableObject.CreateInstance( processorType ) as NPVoxNormalProcessor;
        if ( !newProcessor )
        {
            Debug.LogError( "NPVoxNormalProcessorList: Type parameter '" + processorType.ToString() + "' is not a subclass of NPVoxNormalProcessor!" );
            return null;
        }

        m_processorList.Add( newProcessor );
        newProcessor.hideFlags = HideFlags.HideInHierarchy;

        return newProcessor;
    }

    public PROCESSOR_TYPE AddProcessor<PROCESSOR_TYPE>() where PROCESSOR_TYPE : NPVoxNormalProcessor, new()
    {
        PROCESSOR_TYPE newProcessor = ScriptableObject.CreateInstance< PROCESSOR_TYPE >();
        m_processorList.Add( newProcessor );

        newProcessor.hideFlags = HideFlags.HideInHierarchy;

        return newProcessor;
    }

    public void DestroyProcessor( NPVoxNormalProcessor processor )
    {
        m_processorList.Remove( processor );

        processor.Passes.Clear();

        ScriptableObject.DestroyImmediate( processor, true );
        UnityEditor.EditorUtility.SetDirty( this );
    }

    public List<NPVoxNormalProcessor> GetProcessors()
    {
        return m_processorList;
    }

    public void Run(NPVoxModel model, NPVoxMeshData[] tempdata, Vector3[] inNormals, Vector3[] outNormals)
    {
        inNormals.CopyTo( outNormals, 0 );

        foreach ( NPVoxNormalProcessor processor in m_processorList )
        {
            processor.InitOutputBuffer( outNormals.Length );
            processor.Process(model, tempdata, outNormals, outNormals);
        }
    }

    public void Run( NPVoxModel model, NPVoxMeshData[] tempdata, Vector3[] inNormals, Vector3[] outNormals, NPVoxNormalProcessor end )
    {
        inNormals.CopyTo( outNormals, 0 );

        foreach ( NPVoxNormalProcessor processor in m_processorList )
        {
            processor.InitOutputBuffer( inNormals.Length );
            processor.Process( model, tempdata, outNormals, outNormals );
            if ( processor == end )
            {
                break;
            }
        }
    }

    public void MoveProcessorUp( NPVoxNormalProcessor processor )
    {
        int index = m_processorList.FindIndex( item => item == processor );
        if ( index > 0 )
        {
            m_processorList.Remove( processor );
            m_processorList.Insert( index - 1, processor );
            UnityEditor.EditorUtility.SetDirty( this );
        }

        bool bInform = false;
        foreach( NPVoxNormalProcessor p in m_processorList )
        {
            if ( bInform || p == processor )
            {
                bInform = true;
                p.OnListChanged( this );
            }
        }
    }

    public void MoveProcessorDown( NPVoxNormalProcessor processor )
    {
        int index = m_processorList.FindIndex( item => item == processor );
        if ( index >= 0 && index < m_processorList.Count - 1 )
        {
            m_processorList.Remove( processor );
            m_processorList.Insert( index + 1, processor );
            UnityEditor.EditorUtility.SetDirty( this );
        }

        bool bInform = false;
        foreach ( NPVoxNormalProcessor p in m_processorList )
        {
            if ( bInform || p == processor )
            {
                bInform = true;
                p.OnListChanged( this );
            }
        }
    }

    public NPVoxNormalProcessor GetPreviousInList( NPVoxNormalProcessor _processor )
    {
        NPVoxNormalProcessor previous = null;
        foreach ( NPVoxNormalProcessor processor in m_processorList )
        {
            if ( processor == _processor )
            {
                return previous;
            }

            previous = processor;
        }
        return previous;
    }

    public NPVoxNormalProcessor GetNextInList( NPVoxNormalProcessor _processor )
    {
        NPVoxNormalProcessor previous = null;
        foreach ( NPVoxNormalProcessor processor in m_processorList )
        {
            if ( previous != null )
            {
                return processor;
            }

            if ( processor == _processor )
            {
                previous = processor;
            }
        }

        return null;
    }

    public object Clone()
    {
        NPVoxNormalProcessorList clone = ScriptableObject.CreateInstance<NPVoxNormalProcessorList>();
        foreach( NPVoxNormalProcessor processor in m_processorList )
        {
            clone.m_processorList.Add(processor.Clone() as NPVoxNormalProcessor);
        }
        return clone;
    }
}
