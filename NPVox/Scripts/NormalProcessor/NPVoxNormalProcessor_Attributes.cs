using UnityEngine;
using System.Collections.Generic;


public enum NPVoxNormalProcessorType
{
    Generator,     // Generates normals from scratch -> Should be the first in a processor list, as it overrides previous results
    Modifier,    // Modifies existing normals -> Should not be the first in processor list
}


[System.AttributeUsage( System.AttributeTargets.Class | System.AttributeTargets.Struct )]
public class NPVoxAttributeNormalProcessorListItem : System.Attribute
{
    public string Name;
    public System.Type ClassType;
    NPVoxNormalProcessorType ProcessorType;
    public int ListPriority;

    public NPVoxAttributeNormalProcessorListItem( string editorName, System.Type classType, NPVoxNormalProcessorType processorType )
    {
        Name = editorName;
        ClassType = classType;
        ProcessorType = processorType;
        ListPriority = processorType == NPVoxNormalProcessorType.Generator ? 0 : 1;
    }

    public string EditorName
    {
        get { return ProcessorType.ToString() + ": " + Name; }
    }
}
