using UnityEngine;
using System.Collections;

// from http://answers.unity3d.com/questions/402280/how-to-decompose-a-trs-matrix.html
public class NPipeArrayUtil
{
    public static int GetElementIndex<T>(T[] array, T element)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i].Equals(element))
            {
                return i;
            }
        }
        return -1;
    }
}
