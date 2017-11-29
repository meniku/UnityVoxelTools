using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;


[System.Serializable]
public class NPVoxBox
{
    public static NPVoxBox INVALID
    {
        get
        {
            return new NPVoxBox( NPVoxCoord.INVALID, NPVoxCoord.INVALID );
        }
    }

    [SerializeField]
    private NPVoxCoord leftDownBack;
    public NPVoxCoord LeftDownBack
    {
        get
        {
            return leftDownBack;
        }
        set
        {
            if (value.X > rightUpForward.X)
            {
                rightUpForward.X = value.X;
            }
            if (value.Y > rightUpForward.Y)
            {
                rightUpForward.Y = value.Y;
            }
            if (value.Z > rightUpForward.Z)
            {
                rightUpForward.Z = value.Z;
            }
            leftDownBack = value;
        }
    }

    [SerializeField]
    private NPVoxCoord rightUpForward;
    public NPVoxCoord RightUpForward
    {
        get
        {
            return rightUpForward;
        }
        set
        {
            if (value.X < leftDownBack.X)
            {
                leftDownBack.X = value.X;
            }
            if (value.Y < leftDownBack.Y)
            {
                leftDownBack.Y = value.Y;
            }
            if (value.Z < leftDownBack.Z)
            {
                leftDownBack.Z = value.Z;
            }
            rightUpForward = value;
        }
    }

    public NPVoxCoord LeftDownForward
    {
        get { return new NPVoxCoord(leftDownBack.X, leftDownBack.Y, rightUpForward.Z); }
        set
        {
            if (value.X > rightUpForward.X)
            {
                rightUpForward.X = value.X;
            }
            leftDownBack.X = value.X;

            if (value.Y > rightUpForward.Y)
            {
                rightUpForward.Y = value.Y;
            }
            leftDownBack.Y = value.Y;

            if (value.Z < leftDownBack.Z)
            {
                this.leftDownBack.Z = value.Z;
            }
            rightUpForward.Z = value.Z;
        }
    }
    public NPVoxCoord LeftUpBack
    {
        get { return new NPVoxCoord(leftDownBack.X, rightUpForward.Y, leftDownBack.Z); }
        set
        {
            if (value.X > rightUpForward.X)
            {
                rightUpForward.X = value.X;
            }
            leftDownBack.X = value.X;

            if (value.Y < leftDownBack.Y)
            {
                leftDownBack.Y = value.Y;
            }
            rightUpForward.Y = value.Y;

            if (value.Z > rightUpForward.Z)
            {
                this.rightUpForward.Z = value.Z;
            }
            leftDownBack.Z = value.Z;
        }
    }

    public NPVoxCoord LeftUpForward
    {
        get { return new NPVoxCoord(leftDownBack.X, rightUpForward.Y, rightUpForward.Z); }
        set
        {
            if (value.X > rightUpForward.X)
            {
                rightUpForward.X = value.X;
            }
            leftDownBack.X = value.X;

            if (value.Y < leftDownBack.Y)
            {
                leftDownBack.Y = value.Y;
            }
            rightUpForward.Y = value.Y;

            if (value.Z < leftDownBack.Z)
            {
                this.leftDownBack.Z = value.Z;
            }
            rightUpForward.Z = value.Z;
        }
    }
    public NPVoxCoord RightDownBack
    {
        get { return new NPVoxCoord(rightUpForward.X, leftDownBack.Y, leftDownBack.Z); }
        set
        {
            if (value.X < leftDownBack.X)
            {
                leftDownBack.X = value.X;
            }
            rightUpForward.X = value.X;

            if (value.Y > rightUpForward.Y)
            {
                rightUpForward.Y = value.Y;
            }
            leftDownBack.Y = value.Y;

            if (value.Z > rightUpForward.Z)
            {
                this.rightUpForward.Z = value.Z;
            }
            leftDownBack.Z = value.Z;
        }
    }
    public NPVoxCoord RightDownForward
    {
        get { return new NPVoxCoord(rightUpForward.X, leftDownBack.Y, rightUpForward.Z); }
        set
        {
            if (value.X < leftDownBack.X)
            {
                leftDownBack.X = value.X;
            }
            rightUpForward.X = value.X;

            if (value.Y > rightUpForward.Y)
            {
                rightUpForward.Y = value.Y;
            }
            leftDownBack.Y = value.Y;

            if (value.Z < leftDownBack.Z)
            {
                this.leftDownBack.Z = value.Z;
            }
            rightUpForward.Z = value.Z;
        }
    }
    public NPVoxCoord RightUpBack
    {
        get { return new NPVoxCoord(rightUpForward.X, rightUpForward.Y, leftDownBack.Z); }
        set
        {
            if (value.X < leftDownBack.X)
            {
                leftDownBack.X = value.X;
            }
            rightUpForward.X = value.X;

            if (value.Y < leftDownBack.Y)
            {
                leftDownBack.Y = value.Y;
            }
            rightUpForward.Y = value.Y;

            if (value.Z > rightUpForward.Z)
            {
                this.rightUpForward.Z = value.Z;
            }
            leftDownBack.Z = value.Z;
        }
    }


    public sbyte Left
    {
        get { return leftDownBack.X; }
        set
        {
            if (value > rightUpForward.X)
            {
                rightUpForward.X = value;
            }
            leftDownBack.X = value;
        }
    }

    public sbyte Right
    {
        get { return rightUpForward.X; }
        set
        {
            if (value < leftDownBack.X)
            {
                leftDownBack.X = value;
            }
            rightUpForward.X = value;
        }
    }

    public sbyte Down
    {
        get { return leftDownBack.Y; }
        set
        {
            if (value > rightUpForward.Y)
            {
                rightUpForward.Y = value;
            }
            leftDownBack.Y = value;
        }
    }

    public sbyte Up
    {
        get { return rightUpForward.Y; }
        set
        {
            if (value < leftDownBack.Y)
            {
                leftDownBack.Y = value;
            }
            rightUpForward.Y = value;
        }
    }

    public sbyte Back
    {
        get { return leftDownBack.Z; }
        set
        {
            if (value > rightUpForward.Z)
            {
                rightUpForward.Z = value;
            }
            leftDownBack.Z = value;
        }
    }

    public sbyte Forward
    {
        get { return rightUpForward.Z; }
        set
        {
            if (value < leftDownBack.Z)
            {
                leftDownBack.Z = value;
            }
            rightUpForward.Z = value;
        }
    }
    public NPVoxCoord Size
    {
        get
        {
            return new NPVoxCoord(
                (sbyte)(rightUpForward.X - leftDownBack.X + 1),
                (sbyte)(rightUpForward.Y - leftDownBack.Y + 1),
                (sbyte)(rightUpForward.Z - leftDownBack.Z + 1)
            );
        }
    }

    public NPVoxCoord Center
    {
        get
        {
            NPVoxCoord size = Size;
            Assert.IsTrue(size.X % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
            Assert.IsTrue(size.Y % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
            Assert.IsTrue(size.Z % 2 == 1, "Center is not representable in NPVoxCoords for this Box");

            return leftDownBack + new NPVoxCoord(
                (sbyte)(size.X / 2),
                (sbyte)(size.Y / 2),
                (sbyte)(size.Z / 2)
            );
        }
    }
    public NPVoxCoord RoundedCenter
    {
        get
        {
            NPVoxCoord size = Size;
            return leftDownBack + new NPVoxCoord(
                (sbyte)(Mathf.Round(((float)size.X - 1f) / 2f)),
                (sbyte)(Mathf.Round(((float)size.Y - 1f) / 2f)),
                (sbyte)(Mathf.Round(((float)size.Z - 1f) / 2f))
            );
        }
    }

    public NPVoxBox(NPVoxCoord leftDownBack, NPVoxCoord rightUpForward)
    {
//        if ( rightUpForward.X < leftDownBack.X )
//        {
//            Debug.Log( "WTf" );
//        }
        Assert.IsTrue(rightUpForward.X >= leftDownBack.X, rightUpForward.X + " is < than " + leftDownBack.X);
        Assert.IsTrue(rightUpForward.Y >= leftDownBack.Y);
        Assert.IsTrue(rightUpForward.Z >= leftDownBack.Z);
        this.leftDownBack = leftDownBack;
        this.rightUpForward = rightUpForward;
    }

    public static NPVoxBox FromCenterSize(NPVoxCoord center, NPVoxCoord size)
    {
        Assert.IsTrue(size.X % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
        Assert.IsTrue(size.Y % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
        Assert.IsTrue(size.Z % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
        NPVoxCoord SizeHalf = new NPVoxCoord(
            (sbyte)(size.X / 2),
            (sbyte)(size.Y / 2),
            (sbyte)(size.Z / 2)
        );
        return new NPVoxBox(center - SizeHalf, center + SizeHalf);
    }

    public bool Contains(NPVoxCoord coord)
    {
        return
            coord.X >= leftDownBack.X && coord.X <= rightUpForward.X &&
            coord.Y >= leftDownBack.Y && coord.Y <= rightUpForward.Y &&
            coord.Z >= leftDownBack.Z && coord.Z <= rightUpForward.Z;
    }


    public override bool Equals(System.Object other)
    {
        NPVoxBox o = other as NPVoxBox;
        if (o == null)
        {
            return false;
        }
        return o.LeftDownBack.Equals(this.leftDownBack) && o.RightUpForward.Equals(this.rightUpForward);
    }

    public override int GetHashCode()
    {
        throw new System.Exception("not implemented");
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
                    yield return new NPVoxCoord((sbyte)(leftDownBack.X + x), (sbyte)(leftDownBack.Y + y), (sbyte)(leftDownBack.Z + z));
                }
            }
        }
    }

    public void EnlargeToInclude(NPVoxCoord coord)
    {
        if (coord.X < this.Left)
        {
            this.Left = coord.X;
        }
        if (coord.Y < this.Down)
        {
            this.Down = coord.Y;
        }
        if (coord.Z < this.Back)
        {
            this.Back = coord.Z;
        }
        if (coord.X > this.Right)
        {
            this.Right = coord.X;
        }
        if (coord.Y > this.Up)
        {
            this.Up = coord.Y;
        }
        if (coord.Z > this.Forward)
        {
            this.Forward = coord.Z;
        }
    }

    public void Clamp(NPVoxBox max) 
    {
        if(Left < max.Left) Left = max.Left;
        if(Right > max.Right) Right = max.Right;
        if(Down < max.Down) Down = max.Down;
        if(Up > max.Up) Up = max.Up;
        if(Back < max.Back) Back = max.Back;
        if(Forward > max.Forward) Forward = max.Forward;
    }

    public NPVoxBox Clone()
    {
        return new NPVoxBox(LeftDownBack, RightUpForward);
    }

    public Vector3 SaveCenter
    {
        get
        {
            NPVoxCoord size = Size;
            return new Vector3(
                leftDownBack.X + ((float)size.X - 1f) / 2f,
                leftDownBack.Y + ((float)size.Y - 1f) / 2f,
                leftDownBack.Z + ((float)size.Z - 1f) / 2f
            );
        }
        set
        {
            NPVoxCoord size = Size;
            NPVoxCoord newLeftDownBack = new NPVoxCoord(
                (sbyte)(Mathf.Round(value.x - ((float)size.X - 1f) / 2f)),
                (sbyte)(Mathf.Round(value.y - ((float)size.Y - 1f) / 2f)),
                (sbyte)(Mathf.Round(value.z - ((float)size.Z - 1f) / 2f))
            );
            NPVoxCoord delta = newLeftDownBack - leftDownBack;
            leftDownBack = newLeftDownBack;
            rightUpForward = rightUpForward + delta;
        }
    }
}