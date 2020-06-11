using DungeonArchitect.Editors.DungeonFlow;
using DungeonArchitect.Grammar;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DungeonArchitect.Editors.LaunchPad.Actions.Impl
{
    public class LaunchPadActionOpenSnapFlow : LaunchPadActionBase
    {
        string path;
        public LaunchPadActionOpenSnapFlow(string path)
        {
            this.path = path;
        }

        public override Texture2D GetIcon()
        {
            return ScreenPageLoader.LoadImageAsset("icons/font_awesome/icon_graph");
        }

        public override string GetText()
        {
            return "Open Snap\r\nFlow Graph";
        }

        public override void Execute()
        {
            var flowAsset = AssetDatabase.LoadAssetAtPath<DungeonFlowAsset>(path);
            if (flowAsset != null)
            {
                var window = EditorWindow.GetWindow<DungeonFlowEditorWindow>();
                if (window != null)
                {
                    window.Init(flowAsset);
                }
            }
        }
    }
}