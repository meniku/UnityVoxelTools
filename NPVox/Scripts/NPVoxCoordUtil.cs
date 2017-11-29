using UnityEngine;

public class NPVoxCoordUtil
{
    public static Vector3 ToVector(NPVoxCoord coord)
    {
        return new Vector3(coord.X, coord.Y, coord.Z);
    }

    public static NPVoxCoord ToCoord(Vector3 vector)
    {
        return new NPVoxCoord((sbyte)Mathf.Round(vector.x), (sbyte)Mathf.Round(vector.y), (sbyte)Mathf.Round(vector.z));
    }
}