using UnityEngine;
using System.Collections.Generic;

public class NPVoxModel : ScriptableObject
{
    [SerializeField]
    private uint version = 0;
    [SerializeField]
    private NPVoxCoord size;
    [SerializeField]
    protected Color32[] colortable;
    [SerializeField]
    protected byte[] voxels;
    [SerializeField]
    protected byte[] voxelGroups = null;
    [SerializeField]
    protected byte numVoxelGroups = 1;
    [SerializeField]
    public NPVoxSocket[] Sockets = new NPVoxSocket[]{};

    private List<NPVoxCoord> voxelListCache = null;

    public byte NumVoxelGroups
    {
        get
        {
            return numVoxelGroups;
        }
        set
        {
            this.numVoxelGroups = value;
        }
    }

    [SerializeField]
    private int numVoxels;

    public static NPVoxModel NewInvalidInstance(NPVoxModel reuse = null, string message = "Created an invalid instance")
    {
        Debug.LogWarning(message);
        return NewInstance(NPVoxCoord.ZERO, reuse);
    }

    public static NPVoxModel NewInstance(NPVoxCoord size, NPVoxModel reuse = null)
    {
        NPVoxModel VoxModel = reuse != null ? reuse : ScriptableObject.CreateInstance<NPVoxModel>();
        VoxModel.name = "Model";
        VoxModel.Initialize(size);
        return VoxModel;
    }

    public static NPVoxModel NewInstance(NPVoxModel parent, NPVoxModel reuse = null)
    {
        return NewInstance(parent, parent.Size, reuse);
    }

    public static NPVoxModel NewInstance(NPVoxModel parent, NPVoxCoord size, NPVoxModel reuse = null)
    {
        NPVoxModel voxModel = null;
        if (reuse == null || reuse.GetType() != parent.GetType())
        {
            if (reuse != null)
            {
                Object.DestroyImmediate(reuse);
            }

            voxModel = (NPVoxModel)parent.GetType()
                .GetMethod("NewInstance", new System.Type[]{ typeof(NPVoxCoord), typeof(NPVoxModel) })
                .Invoke(null, new object[] { size, reuse });
        }
        else
        {
            voxModel = reuse;
        }

        voxModel.name = "Model";
        voxModel.Initialize(size);
        return voxModel;
    }

    public virtual void Initialize(NPVoxCoord size)
    {
        this.numVoxels = -1;
        this.size = size;
        this.colortable = null;
        this.voxelGroups = null;
        this.numVoxelGroups = 1;
        this.voxels = new byte[size.X * size.Y * size.Z];
        this.Sockets = new NPVoxSocket[]{ };
        this.version++;
        this.voxelListCache = null;
    }

    public virtual void CopyOver(NPVoxModel source)
    {
        this.numVoxels = source.numVoxels;
        this.size = source.size;
        this.voxels = (byte[])source.voxels.Clone();
        this.numVoxelGroups = source.numVoxelGroups;
        this.voxelGroups =  source.voxelGroups != null ? (byte[])source.voxelGroups.Clone() : null;
        this.colortable = source.colortable != null ? (Color32[]) source.colortable.Clone() : null;
        this.Sockets = source.Sockets != null ? (NPVoxSocket[]) source.Sockets.Clone() : null;
        this.voxelListCache = null;
    }

    public uint GetVersion()
    {
        return this.version;
    }

    public bool IsValid
    {
        get {
            return this.numVoxels > 0 && this.colortable != null;
        } 
    }

    public NPVoxCoord Size
    {
        get
        {
            return size;
        }
    }

    public sbyte SizeX
    {
        get
        {
            return size.X;
        }
    }
    public sbyte SizeY
    {
        get
        {
            return size.Y;
        }
    }
    public sbyte SizeZ
    {
        get
        {
            return size.Z;
        }
    }

    public int NumVoxels
    {
        set
        {
//            if (this.numVoxels == -1)
            {
                this.numVoxels = value;
            }
        }
        get
        {
            return this.numVoxels;
        }
    }

    public Color32[] Colortable
    {
        set
        {
            this.colortable = value;
        }
        get
        {
            return this.colortable;
        }
    }

    public void SetVoxel(NPVoxCoord coord, byte color)
    {
        if (IsInside(coord))
        {
            voxels[GetIndex(coord)] = color;
        }
    }

    public byte GetVoxel(NPVoxCoord coord)
    {
        return this.voxels[GetIndex(coord)];
    }

#if UNITY_EDITOR
    public virtual Color32 GetColor(NPVoxCoord coord)
#else
    public Color32 GetColor(NPVoxCoord coord)
#endif
    {
        return this.colortable[this.voxels[GetIndex(coord)]];
    }

    public bool HasVoxel(NPVoxCoord coord)
    {
        return IsInside(coord) && this.voxels[GetIndex(coord)] != 0;
    }

    public bool HasVoxelFast(NPVoxCoord coord)
    {
        return this.voxels[GetIndex(coord)] != 0;
    }

    public void UnsetVoxel(NPVoxCoord coord)
    {
        if (IsInside(coord))
        {
            this.voxels[GetIndex(coord)] = 0;
        }
    }

    public bool IsInside(NPVoxCoord coord)
    {
        return coord.Valid &&
            coord.X < size.X &&
            coord.Y < size.Y &&
            coord.Z < size.Z;
    }

    public void InitVoxelGroups()
    {
        this.voxelGroups = new byte[size.X * size.Y * size.Z];
    }

    public bool HasVoxelGroups()
    {
        return this.voxelGroups != null && this.voxelGroups.Length > 0;
    }

    public void SetVoxelGroup(NPVoxCoord coord, byte group)
    {
        if (IsInside(coord))
        {
            voxelGroups[GetIndex(coord)] = group;
        }
    }

    public byte GetVoxelGroup(NPVoxCoord coord)
    {
        return this.voxelGroups[GetIndex(coord)];
    }

    protected int GetIndex(NPVoxCoord coord)
    {
        return (coord.X + coord.Z * size.X + coord.Y * size.X * size.Z);
    }

    public NPVoxCoord LoopCoord(NPVoxCoord coord, NPVoxFaces loop)
    {
        if (loop.Right != 0)
        {
            if (loop.Right < 0 && coord.X >= SizeX)
            {
                coord.X = (sbyte)(coord.X + loop.Right);
            }

            coord.X = (sbyte)(coord.X % SizeX);
        }
        if (loop.Left != 0)
        {
            if (loop.Left < 0 && coord.X < 0)
            {
                coord.X = (sbyte)(coord.X - loop.Left);
            }

            while (coord.X < 0) coord.X += SizeX;
        }
        if (loop.Up != 0)
        {
            if (loop.Up < 0 && coord.Y >= SizeY)
            {
                coord.Y = (sbyte)(coord.Y + loop.Up);
            }

            coord.Y = (sbyte)(coord.Y % SizeY);
        }
        if (loop.Down != 0)
        {
            if (loop.Down < 0 && coord.Y < 0)
            {
                coord.Y = (sbyte)(coord.Y - loop.Down);
            }

            while (coord.Y < 0) coord.Y += SizeY;
        }
        if (loop.Forward != 0)
        {
            if (loop.Forward < 0 && coord.Z >= SizeZ)
            {
                coord.Z = (sbyte)(coord.Z + loop.Forward);
            }

            coord.Z = (sbyte)(coord.Z % SizeZ);
        }
        if (loop.Back != 0)
        {
            if (loop.Back < 0 && coord.Z < 0)
            {
                coord.Z = (sbyte)(coord.Z - loop.Back);
            }

            while (coord.Z < 0) coord.Z += SizeZ;
        }
        return coord;
    }

    public IEnumerable<NPVoxCoord> Enumerate()
    {
        NPVoxCoord size = Size;

        for (sbyte x = 0; x < size.X; x++)
        {
            for (sbyte y = 0; y < size.Y; y++)
            {
                for (sbyte z = 0; z < size.Z; z++)
                {
                    yield return new NPVoxCoord(x, y, z);
                }
            }
        }
    }

    public void InvalidateVoxelCache()
    {
        if (voxelListCache != null)
        {
            voxelListCache = null;
        }
    }

    public IEnumerable<NPVoxCoord> EnumerateVoxels()
    {
//        if (voxelListCache != null)
//        {
//            return voxelListCache;
//        }
//        voxelListCache = new List<NPVoxCoord>();
        NPVoxCoord size = Size;

        for (sbyte x = 0; x < size.X; x++)
        {
            for (sbyte y = 0; y < size.Y; y++)
            {
                for (sbyte z = 0; z < size.Z; z++)
                {
                    NPVoxCoord coord = new NPVoxCoord(x, y, z);
                    if (HasVoxel(coord))
                    {
//                        voxelListCache.Add(coord);
                        yield return coord;
                    }
                }
            }
        }

//        return voxelListCache;
    }

    public NPVoxBox Clamp(NPVoxBox box)
    {
        if (IsInside(box.LeftDownBack) && IsInside(box.RightUpForward))
        {
            return box;
        }
        else
        {
            return new NPVoxBox(Clamp(box.LeftDownBack), Clamp(box.RightUpForward));
        }
    }

    public NPVoxCoord Clamp(NPVoxCoord coord)
    {
        if (IsInside(coord))
        {
            return coord;
        }
        else
        {
            NPVoxCoord size = this.size;
            return new NPVoxCoord(
                (sbyte)(coord.X < 0 ? 0 : (coord.X >= size.X ? size.X - 1 : coord.X)),
                (sbyte)(coord.Y < 0 ? 0 : (coord.Y >= size.Y ? size.Y - 1 : coord.Y)),
                (sbyte)(coord.Z < 0 ? 0 : (coord.Z >= size.Z ? size.Z - 1 : coord.Z))
            );
        }
    }

    public NPVoxBox BoundingBox
    {
        get
        {
            return new NPVoxBox(NPVoxCoord.ZERO, Size - NPVoxCoord.ONE);
        }
    }

    public NPVoxBox BoundingBoxMinimal
    {
        get
        {
            NPVoxBox box = null;
            foreach (NPVoxCoord coord in this.EnumerateVoxels())
            {
                if ( voxels[GetIndex(coord)] == 0 )
                {
                    continue;
                }

                if ( box == null )
                {
                    box = new NPVoxBox(coord, coord);
                }
                else
                {
                    box.EnlargeToInclude(coord);
                }
            }
            return box;
        }
    }

    public void RecalculateNumVoxels(bool withWarning = false)
    {
        InvalidateVoxelCache();
        int numVoxels = 0;
        foreach(NPVoxCoord coord in this.EnumerateVoxels())
        {
            numVoxels++;
        }
        if (withWarning && this.numVoxels != numVoxels)
        {
            Debug.LogWarning("NumVoxels was wrong: " + this.numVoxels + " Correct: " +numVoxels);
        }
        this.numVoxels = numVoxels;
    }

    public NPVoxSocket GetSocketByName(string socketName)
    {
        foreach (NPVoxSocket socket in this.Sockets)
        {
            if (socket.Name == socketName)
            {
                return socket;
            }
        }

        NPVoxSocket invalidSocket = new NPVoxSocket();
        invalidSocket.SetInvalid();
        return invalidSocket;
    }

    public void SetSocket(NPVoxSocket socket)
    {
        for (int i = 0; i < this.Sockets.Length; i++)
        {
            if (this.Sockets[i].Name == socket.Name)
            {
                this.Sockets[i] = socket;
                return;
            }
        }
    }

    public string[] SocketNames
    {
        get
        {
            string[] socketnames = new string[this.Sockets.Length];
            int i = 0;
            foreach (NPVoxSocket socket in this.Sockets)
            {
                socketnames[i++] = socket.Name;
            }
            return socketnames;
        }
    }
}