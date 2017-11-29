using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct NPVoxSocket
{
    [SerializeField]
    public NPVoxCoord Anchor;
    [SerializeField]
    public Vector3 EulerAngles;
    [SerializeField]
    public string Name;

    public void SetInvalid()
    {
        Name = "InvalidSocket";
    }

    public bool IsInvalid()
    {
        return Name == "InvalidSocket";
    }
}