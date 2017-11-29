using UnityEngine;

[System.Serializable]
public class NPVoxFaces
{
    public sbyte Left = 0;
    public sbyte Right = 0;
    public sbyte Up = 0;
    public sbyte Down = 0;
    public sbyte Forward = 0;
    public sbyte Back = 0;
    
    public NPVoxFaces Clone()
    {
        NPVoxFaces clone = new NPVoxFaces( );
        clone.Left = Left;
        clone.Right = Right;
        clone.Up = Up;
        clone.Down = Down;
        clone.Forward = Forward;
        clone.Back = Back;
        return clone;
    }
    
    public NPVoxFaces(sbyte left = 0, sbyte right = 0, sbyte up = 0, sbyte down = 0, sbyte back = 0, sbyte forward = 0 )
    {
        Left = left;
        Right = right;
        Up = up;
        Down = down;
        Back = back;
        Forward = forward;
    }
}