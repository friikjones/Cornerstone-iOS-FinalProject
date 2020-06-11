using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.UI
{
    public interface UIStyleManager
    {
        GUIStyle GetToolbarButtonStyle();
        GUIStyle GetButtonStyle();
        GUIStyle GetBoxStyle();
        GUIStyle GetLabelStyle();
        GUIStyle GetBoldLabelStyle();
        Font GetFontStandard();
        Font GetFontBold();
        Font GetFontMini();
    }
}