using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class NPVoxNormalProcessorList : ScriptableObject
{
    [SerializeField]
    private List<NPVoxNormalProcessor> m_processorList = null;

    public NPVoxNormalProcessorList()
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

        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        UnityEditor.AssetDatabase.AddObjectToAsset( newProcessor, path );
        UnityEditor.EditorUtility.SetDirty( newProcessor );

        return newProcessor;
    }

    public NPVoxNormalProcessor AddProcessor<PROCESSOR_TYPE>() where PROCESSOR_TYPE : NPVoxNormalProcessor, new()
    {
        PROCESSOR_TYPE newProcessor = ScriptableObject.CreateInstance< PROCESSOR_TYPE >();
        m_processorList.Add( newProcessor );

        newProcessor.hideFlags = HideFlags.HideInHierarchy;

        string path = UnityEditor.AssetDatabase.GetAssetPath( this );
        UnityEditor.AssetDatabase.AddObjectToAsset( newProcessor, path );
        UnityEditor.EditorUtility.SetDirty( newProcessor );

        return newProcessor;
    }

    public void DestroyProcessor( NPVoxNormalProcessor processor )
    {
        m_processorList.Remove( processor );
        ScriptableObject.DestroyImmediate( processor, true );
        UnityEditor.EditorUtility.SetDirty( this );
    }

    public List<NPVoxNormalProcessor> GetProcessors()
    {
        return m_processorList;
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
    }
}
