using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPVoxSocketAttachment
{
    public string targetSocketName = null;
    public string sourceSocketName = null;
    public NPVoxMeshOutput meshFactory = null;
    public bool visible = true;

    [System.NonSerialized]
    public NPVoxIMeshFactory outputMeshFactory = null;
}
