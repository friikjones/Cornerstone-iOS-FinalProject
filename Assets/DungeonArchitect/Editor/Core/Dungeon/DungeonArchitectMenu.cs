using DungeonArchitect.Editors.DungeonFlow;
using DungeonArchitect.Editors.GridFlow;
using DungeonArchitect.Editors.LaunchPad;
using DungeonArchitect.Editors.SpatialConstraints;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DungeonArchitect.Editors
{
    public class DungeonArchitectMenu
    {
        [MenuItem("Dungeon Architect/Launch Pad", priority = 1000)]
        public static void OpenWindow_LaunchPad()
        {
            LaunchPadWindow.OpenWindow();
        }

        //------------------- Create Menu -------------------
        [MenuItem("Dungeon Architect/Create/Dungeon Theme", priority = 101)]
        public static void CreateThemeAssetInBrowser()
        {
            DungeonEditorHelper.CreateThemeAssetInBrowser();
        }

        [MenuItem("Dungeon Architect/Create/Flow Graphs/Snap Graph", priority = 102)]
        public static void CreateDungeonFlowAssetInBrowser()
        {
            DungeonEditorHelper.CreateDungeonFlowAssetInBrowser();
        }

        [MenuItem("Dungeon Architect/Create/Flow Graphs/Grid Graph", priority = 103)]
        public static void CreateDungeonGridCyclicAssetInBrowser()
        {
            DungeonEditorHelper.CreateDungeonGridCyclicAssetInBrowser();
        }

        [MenuItem("Dungeon Architect/Create/Landscape/Landscape Restoration Cache", priority = 104)]
        public static void CreateDungeonLandscapeRestCacheInBrowser()
        {
            DungeonEditorHelper.CreateDungeonLandscapeRestCacheInBrowser();
        }

        //------------------- Windows Menu -------------------
        [MenuItem("Dungeon Architect/Windows/Theme Editor", priority = 111)]
        public static void OpenWindow_ThemeEditor()
        {
            DungeonThemeEditorWindow.ShowEditor();
        }

        [MenuItem("Dungeon Architect/Windows/Grid Flow", priority = 112)]
        public static void OpenWindow_GridFlow()
        {
            DungeonGridFlowEditorWindow.ShowWindow();
        }

        [MenuItem("Dungeon Architect/Windows/SnapFlow", priority = 113)]
        public static void OpenWindow_SnapFlow()
        {
            DungeonFlowEditorWindow.ShowEditor();
        }
        [MenuItem("Dungeon Architect/Windows/Spatial Constraints", priority = 114)]
        public static void OpenWindow_SpatialConstraints()
        {
            SpatialConstraintsEditorWindow.ShowWindow();
        }
    }
}
