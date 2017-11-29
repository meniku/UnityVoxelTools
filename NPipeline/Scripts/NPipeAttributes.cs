using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
public class NPipeAppendableAttribute : System.Attribute
{
    public string name;
    public bool attached;
    public bool separate;
    public System.Type sourceType;

    public NPipeAppendableAttribute(string name, System.Type sourceType, bool attached, bool separate)
    {
        this.name = name;
        this.attached = attached;
        this.separate = separate;
        this.sourceType = sourceType;
    }
}


[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
public class NPipeStartingAttribute : System.Attribute
{
    public string name;
    public bool attached;

    public NPipeStartingAttribute(string name, bool attached)
    {
        this.name = name;
        this.attached = attached;
    }
}

public class NPipeSelectorAttribute : PropertyAttribute
{
    public System.Type Type;

    public NPipeSelectorAttribute(System.Type type)
    {
        this.Type = type;
    }
}