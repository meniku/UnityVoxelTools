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
    public NPVoxNormalProcessorType ProcessorType;
    public int ListPriority;

    public NPVoxAttributeNormalProcessorListItem( string _editorName, System.Type _classType, NPVoxNormalProcessorType _processorType )
    {
        Name = _editorName;
        ClassType = _classType;
        ProcessorType = _processorType;
        ListPriority = _processorType == NPVoxNormalProcessorType.Generator ? 0 : 1;
    }

    public string EditorName
    {
        get { return ProcessorType.ToString() + ": " + Name; }
    }
}

[System.AttributeUsage( System.AttributeTargets.Class )]
public class NPVoxAttributeNormalProcessorPreview : System.Attribute
{
    public System.Type ProcessorType;

    public NPVoxAttributeNormalProcessorPreview( System.Type _processorType )
    {
        ProcessorType = _processorType;
    }
}
