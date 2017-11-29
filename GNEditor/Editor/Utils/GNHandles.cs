using UnityEngine;
using UnityEditor;

public class GNHandles
{
    public static Bounds DrawBoundsSelection(Bounds previous, Vector3 cellOffset, float cellSize)
    {
        NPVoxToUnity npVoxToUnity = new NPVoxToUnity(null, Vector3.one * cellSize, cellOffset - Vector3.one * 0.5f * cellSize);
        NPVoxBox previousBox = new NPVoxBox(
            npVoxToUnity.ToVoxCoord(previous.min + Vector3.one * cellSize / 2), 
            npVoxToUnity.ToVoxCoord(previous.max - Vector3.one * cellSize / 2)
        );
        NPVoxBox newBox = NPVoxHandles.DrawBoxSelection(npVoxToUnity, previousBox, true);
        if (!previousBox.Equals(newBox))
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.SetMinMax(
                npVoxToUnity.ToUnityPosition(newBox.LeftDownBack) - Vector3.one * cellSize / 2, 
                npVoxToUnity.ToUnityPosition(newBox.RightUpForward) + Vector3.one * cellSize / 2
            );
            return bounds;
        }
        else
        {
            return previous;
        }
    }
}