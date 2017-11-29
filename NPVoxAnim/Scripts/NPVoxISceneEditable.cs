public interface NPVoxISceneEditable
{
    #if UNITY_EDITOR
    string[] GetSceneEditingTools();
    System.Func<NPVoxISceneEditable, bool> DrawSceneTool(NPVoxToUnity npVoxToUnity, UnityEngine.Transform transform, int tool);
    void ResetSceneTools();
    #endif
}