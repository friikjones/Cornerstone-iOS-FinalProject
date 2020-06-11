using DungeonArchitect.Graphs;
using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets.GraphEditors;
using UnityEngine;

namespace DungeonArchitect.Editors.DungeonFlow
{
    

    public class DungeonFlowExecutionGraphContextMenu : GraphContextMenu
    {
        public override void Show(GraphEditor graphEditor, GraphPin sourcePin, Vector2 mouseWorld, UISystem uiSystem)
        {
            this.sourcePin = sourcePin;
            var execEditor = graphEditor as DungeonFlowExecutionGraphEditor;
            var flowAsset = (execEditor != null) ? execEditor.FlowAsset : null;

            var menu = uiSystem.Platform.CreateContextMenu();
            if (flowAsset != null && flowAsset.productionRules.Length > 0)
            {
                foreach (var rule in flowAsset.productionRules)
                {
                    string text = "Add Rule: " + rule.ruleName;
                    menu.AddItem(text, HandleContextMenu, new DungeonFlowExecutionGraphEditorMenuData(uiSystem, DungeonFlowExecutionGraphEditorAction.CreateRuleNode, rule));
                }
                menu.AddSeparator("");
            }
            menu.AddItem("Add Comment Node", HandleContextMenu, new DungeonFlowExecutionGraphEditorMenuData(uiSystem, DungeonFlowExecutionGraphEditorAction.CreateCommentNode));
            menu.Show();
        }

        void HandleContextMenu(object action)
        {
            var item = action as DungeonFlowExecutionGraphEditorMenuData;
            DispatchMenuItemEvent(action, BuildEvent(null, item.uiSystem));
        }
    }
}
