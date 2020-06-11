using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Utils
{
    public class ColorUtils
    {
        public static Color BrightenColor(Color color, float saturationMultiplier, float brightnessMultiplier)
        {
            float H, S, V;
            Color.RGBToHSV(color, out H, out S, out V);
            S *= saturationMultiplier;
            V = Mathf.Clamp01(V * brightnessMultiplier);

            return Color.HSVToRGB(H, S, V);
        }
    }
}
