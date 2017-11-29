using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NPVoxModelUtils
{
    public static byte AddBrightenColor(NPVoxModel model)
    {
        byte color = FindUnusedColor(model);
        if(color != 0)
        {
            Color32 brightenColor32 = model.Colortable[color];
            model.Colortable[color] = NPVoxModelUtils.BrightenColor(brightenColor32);
        }
        return color;
    }

    public static bool[] GetUsedColors(NPVoxModel model)
    {
        bool[] usedColors = new bool[model.Colortable.Length];
        foreach (NPVoxCoord coord in model.EnumerateVoxels())
        {
            usedColors[model.GetVoxel(coord)] = true;
        }
        return usedColors;
    }

    public static byte FindUnusedColor(NPVoxModel model)
    {
        bool[] usedColors = GetUsedColors(model);
        for (byte i = 1; i != 0; i++)
        {
            if(!usedColors[i])
            {
                return i;
            }
        }
        return 0;
    }


    public static byte FindUsedColor(ref bool[] usedColors, int num = 0)
    {
        for (byte i = 1; i != 0; i++)
        {
            if(usedColors[i] && num-- == 0)
            {
                return i;
            }
        }
        return 0;
    }

    public static byte FindUnusedColor(ref bool[] usedColors)
    {
        for (byte i = 1; i != 0; i++)
        {
            if(!usedColors[i])
            {
                usedColors[i] = true;
                return i;
            }
        }
        Debug.LogWarning("Colortable full");
        return 0;
    }

    public static Color32 BrightenColor(Color32 returnColor)
    {
        int count = 0;
        count += returnColor.r * 8 > 100 ? 1 : 0;
        count += returnColor.g * 8 > 100 ? 1 : 0;
        count += returnColor.b * 8 > 100 ? 1 : 0;
        if( count > 1 )
        {
            returnColor.r = returnColor.g = returnColor.b = 0;
        }
        else
        {
            returnColor.r = (byte)(Mathf.Min(255, returnColor.g * 8));
            returnColor.g = (byte)(Mathf.Min(255, returnColor.b * 8));
            returnColor.b = (byte)(Mathf.Min(255, returnColor.r * 8));
//            returnColor.r = returnColor.g = returnColor.b = 254;
        }
        return returnColor;
    }

}
