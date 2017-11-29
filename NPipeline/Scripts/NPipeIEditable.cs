[System.Flags]
public enum NPipeEditFlags
{
    NONE = 0,
    INPUT = 1,
    TOOLS = 2,
    STORAGE_MODE = 4,
}

public interface NPipeIEditable
{
#if UNITY_EDITOR
    bool DrawInspector(NPipeEditFlags flags);
#endif
}