using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Editors.LaunchPad.Actions.Impl
{
    public class LaunchPadActionDocumentation : LaunchPadActionBase
    {
        string path;
        public LaunchPadActionDocumentation(string path)
        {
            this.path = path;
        }

        public override Texture2D GetIcon()
        {
            return ScreenPageLoader.LoadImageAsset("icons/font_awesome/icon_docs");
        }

        public override string GetText()
        {
            return "Open Docs";
        }

        public override void Execute()
        {
            OpenLink(path);
        }
    }
}