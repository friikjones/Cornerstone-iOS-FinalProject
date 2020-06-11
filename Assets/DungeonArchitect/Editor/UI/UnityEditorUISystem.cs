using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.UI.Impl.UnityEditor
{
    public class UnityEditorUISystem : UISystem
    {
        protected override UIPlatform CreatePlatformInstance()
        {
            return new UnityEditorUIPlatform();
        }

        protected override UIStyleManager CreateStyleManagerInstance()
        {
            return new UnityEditorUIStyleManager();
        }

        protected override UIUndoSystem CreateUndoSystemInstance()
        {
            return new UnityEditorUIUndoSystem(this);
        }
    }
}