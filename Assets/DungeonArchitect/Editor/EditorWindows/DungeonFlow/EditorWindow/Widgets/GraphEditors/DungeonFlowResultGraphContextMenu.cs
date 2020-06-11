
using System.Collections;
using System.Collections.Generic;
using DungeonArchitect.Graphs;
using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets.GraphEditors;
using UnityEditor;
using UnityEngine;

namespace DungeonArchitect.Editors.DungeonFlow
{
    public class DungeonFlowResultGraphContextMenu : GraphContextMenu
    {
        class ItemInfo
        {
            public ItemInfo(UISystem uiSystem, DungeonFlowResultGraphEditorAction action)
            {
                this.uiSystem = uiSystem;
                this.action = action;
            }

            public UISystem uiSystem;
            public DungeonFlowResultGraphEditorAction action;
        }
        public override void Show(GraphEditor graphEditor, GraphPin sourcePin, Vector2 mouseWorld, UISystem uiSystem)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Comment Node"), false, HandleContextMenu, new ItemInfo(uiSystem, DungeonFlowResultGraphEditorAction.CreateCommentNode));
            menu.ShowAsContext();
        }

        void HandleContextMenu(object userdata)
        {
            var item = userdata as ItemInfo;
            DispatchMenuItemEvent(item.action, BuildEvent(null, item.uiSystem));
        }
    }
}
