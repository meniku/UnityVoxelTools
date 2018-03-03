using UnityEngine;
using System.Collections;

[System.Serializable]
public struct NPVoxCoord
{
    public static NPVoxCoord INVALID = new NPVoxCoord(127, 127, 127);

    public static NPVoxCoord ONE = new NPVoxCoord(1, 1, 1);
    public static NPVoxCoord ZERO = new NPVoxCoord(0, 0, 0);
    public static NPVoxCoord RIGHT = new NPVoxCoord(1, 0, 0);
    public static NPVoxCoord LEFT = new NPVoxCoord(-1, 0, 0);
    public static NPVoxCoord UP = new NPVoxCoord(0, 1, 0);
    public static NPVoxCoord DOWN = new NPVoxCoord(0, -1, 0);
    public static NPVoxCoord FORWARD = new NPVoxCoord(0, 0, 1);
    public static NPVoxCoord BACK = new NPVoxCoord(0, 0, -1);

    [SerializeField]
    private sbyte x;
    public sbyte X
    {
        get
        {
            return x;
        }
        set
        {
            x = value;
        }
    }

    [SerializeField]
    private sbyte y;
    public sbyte Y
    {
        get
        {
            return y;
        }
        set
        {
            y = value;
        }
    }

    [SerializeField]
    private sbyte z;
    public sbyte Z
    {
        get
        {
            return z;
        }
        set
        {
            z = value;
        }
    }

    public NPVoxCoord(sbyte x = 0, sbyte y = 0, sbyte z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public bool Valid
    {
        get
        {
            return
                this.x >= 0 && this.x <= 126 &&
                this.y >= 0 && this.y <= 126 &&
                this.z >= 0 && this.z <= 126;
        }
    }

    public override string ToString()
    {
        return string.Format("NPVoxCoord({0},{1},{2})", X, Y, Z);
    }

    public static NPVoxCoord operator +(NPVoxCoord a, NPVoxCoord b)
    {
        return new NPVoxCoord((sbyte)(a.X + b.X), (sbyte)(a.Y + b.Y), (sbyte)(a.Z + b.Z));
    }

    public static NPVoxCoord operator -(NPVoxCoord a, NPVoxCoord b)
    {
        return new NPVoxCoord((sbyte)(a.X - b.X), (sbyte)(a.Y - b.Y), (sbyte)(a.Z - b.Z));
    }

    public float Length()
    {
        return Mathf.Sqrt((float)(X * X) + (float)(Y * Y) + (float)(Z * Z));
    }

    public static float Distance(NPVoxCoord a, NPVoxCoord b)
    {
        return (a - b).Length();
    }

    public NPVoxCoord WithX(sbyte x)
    {
        return new NPVoxCoord(x, Y, Z);
    }

    public NPVoxCoord WithY(sbyte y)
    {
        return new NPVoxCoord(X, y, Z);
    }
    public NPVoxCoord WithZ(sbyte z)
    {
        return new NPVoxCoord(X, Y, z);
    }

    public bool Equals( NPVoxCoord other )
    {
        return x == other.X && z == other.z && y == other.y;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
