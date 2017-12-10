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
        NPVoxNormalProcessor newProcessor = Activator.CreateInstance( processorType ) as NPVoxNormalProcessor;
        m_processorList.Add( newProcessor );
        return newProcessor;
    }

    public NPVoxNormalProcessor AddProcessor<PROCESSOR_TYPE>() where PROCESSOR_TYPE : NPVoxNormalProcessor, new()
    {
        PROCESSOR_TYPE newProcessor = new PROCESSOR_TYPE();
        m_processorList.Add( newProcessor );
        return newProcessor;
    }

    public void RemoveProcessor( NPVoxNormalProcessor processor )
    {
        m_processorList.Remove( processor );
    }

    public List<NPVoxNormalProcessor> GetProcessors()
    {
        return m_processorList;
    }
}
