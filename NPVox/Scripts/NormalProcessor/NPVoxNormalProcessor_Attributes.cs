using UnityEngine;
using System.Collections.Generic;


public enum NPVoxNormalProcessorType
{
    Generative,     // Generates normals from scratch -> Must be the first in a processor list, otherwise it would override previous results
    Progressive,    // Modifies existing normals -> Must NOT be the first in processor list
}


[System.AttributeUsage( System.AttributeTargets.Class | System.AttributeTargets.Struct )]
public class NPVoxAttributeNormalProcessorListItem : System.Attribute
{
    public string m_editorName;
    public System.Type m_classType;
    NPVoxNormalProcessorType m_processorType;

    public NPVoxAttributeNormalProcessorListItem( string editorName, System.Type classType, NPVoxNormalProcessorType processorType )
    {
        m_editorName = editorName;
        m_classType = classType;
        m_processorType = processorType;
    }
}
