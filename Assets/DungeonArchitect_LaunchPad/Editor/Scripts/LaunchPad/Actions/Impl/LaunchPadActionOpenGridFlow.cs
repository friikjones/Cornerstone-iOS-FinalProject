using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.Editors.GridFlow;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DungeonArchitect.Editors.LaunchPad.Actions.Impl
{
    public class LaunchPadActionOpenGridFlow : LaunchPadActionBase
    {
        string path;
        bool readOnly;

        public LaunchPadActionOpenGridFlow(string path)
            : this(path, false)
        {
        }

        public LaunchPadActionOpenGridFlow(string path, bool readOnly)
        {
            this.path = path;
            this.readOnly = readOnly;
        }

        public override Texture2D GetIcon()
        {
            return ScreenPageLoader.LoadImageAsset("icons/font_awesome/icon_gridflow");
        }

        public override string GetText()
        {
            return "Open Grid\r\nFlow Graph";
        }

        public override void Execute()
        {
            var flowAsset = AssetDatabase.LoadAssetAtPath<DungeonGridFlowAsset>(path);
            if (flowAsset != null)
            {
                var window = EditorWindow.GetWindow<DungeonGridFlowEditorWindow>();
                if (window != null)
                {
                    window.Init(flowAsset);
                    window.HandleExecuteButtonPressed();
                    window.SetReadOnly(readOnly);
                }
            }
        }
    }
}