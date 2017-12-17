using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class NPVoxNormalProcessorList
{
    private List<NPVoxNormalProcessor> m_processorList;

    public NPVoxNormalProcessorList()
    {
        m_processorList = new List<NPVoxNormalProcessor>();
    }

    public NPVoxNormalProcessor AddProcessor( Type processorType )
    {
        NPVoxNormalProcessor newProcessor = ScriptableObject.CreateInstance( processorType ) as NPVoxNormalProcessor;
        if ( !newProcessor )
        {
            Debug.LogError( "NPVoxNormalProcessorList: Type parameter '" + processorType.ToString() + "' is not a subclass of NPVoxNormalProcessor!" );
        }

        m_processorList.Add( newProcessor );
        return newProcessor;
    }

    public NPVoxNormalProcessor AddProcessor<PROCESSOR_TYPE>() where PROCESSOR_TYPE : NPVoxNormalProcessor, new()
    {
        PROCESSOR_TYPE newProcessor = ScriptableObject.CreateInstance< PROCESSOR_TYPE >();
        m_processorList.Add( newProcessor );
        return newProcessor;
    }

    public void DestroyProcessor( NPVoxNormalProcessor processor )
    {
        m_processorList.Remove( processor );
        ScriptableObject.DestroyImmediate( processor );
    }

    public List<NPVoxNormalProcessor> GetProcessors()
    {
        return m_processorList;
    }
}
