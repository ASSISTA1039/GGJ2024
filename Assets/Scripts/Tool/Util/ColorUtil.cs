using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


public enum ColorType
{
    RGB,
    RGBA
}


public static  class ColorUtil
{


    private const string hexRegex = "^#?(?:[0-9a-fA-F]{3,4}){1,2}$";

    //color转Hex
    public static string ColorToHex(Color32 color, ColorType colorType)
    {

        long num = 0;
        string hexStr = "";

        if (colorType == ColorType.RGB)
        {
            num = 0xFFFFFF & (ColorRGBAToInt(color) >> 8);
            hexStr = "#" + num.ToString("X6");
        }
        else
        {
            num = 0xFFFFFFFF & (ColorRGBAToInt(color));
            hexStr = "#" + num.ToString("X8");
        }

        return hexStr;
    }

    static public int ColorRGBAToInt(Color c)
    {
        int retVal = 0;
        retVal |= Mathf.RoundToInt(c.r * 255f) << 24;
        retVal |= Mathf.RoundToInt(c.g * 255f) << 16;
        retVal |= Mathf.RoundToInt(c.b * 255f) << 8;
        retVal |= Mathf.RoundToInt(c.a * 255f);

        return retVal;
    }


    //Hex转Color
    public static void HexToColor(string hex, out Color32 color, ColorType colorType)
    {
        // Check if this is a valid hex string (# is optional)
        color = Color.black;
        if (System.Text.RegularExpressions.Regex.IsMatch(hex, hexRegex))
        {
            int startIndex = hex.StartsWith("#") ? 1 : 0;
            if (colorType == ColorType.RGBA)
            {
                color = new Color32(byte.Parse(hex.Substring(startIndex, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(hex.Substring(startIndex + 2, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(hex.Substring(startIndex + 4, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(hex.Substring(startIndex + 6, 2), NumberStyles.AllowHexSpecifier));
            }
            else if (colorType == ColorType.RGB)
            {
                color = new Color32(byte.Parse(hex.Substring(startIndex, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(hex.Substring(startIndex + 2, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(hex.Substring(startIndex + 4, 2), NumberStyles.AllowHexSpecifier),
                    255);
            }
        }
    }
}