

public interface NPipeIImportable
{
    void Import();
    void Invalidate(bool includeSources = false);
    bool IsValid();
    bool IsTemplate();
    void Destroy();
    string GetTypeName();
    string GetInstanceName();
    UnityEngine.Object Clone();
    void IncludeSubAssets(string path); 

#if UNITY_EDITOR
    double GetLastInvalidatedTime();
#endif
}