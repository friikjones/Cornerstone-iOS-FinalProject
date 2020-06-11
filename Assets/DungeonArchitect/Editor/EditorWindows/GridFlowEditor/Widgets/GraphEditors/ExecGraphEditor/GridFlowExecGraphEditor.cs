using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonArchitect.Graphs;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;
using DungeonArchitect.UI.Widgets.GraphEditors;
using DungeonArchitect.UI;

namespace DungeonArchitect.Editors.GridFlow
{
    public class GridFlowExecGraphEditor : GraphEditor
    {
        public GridFlowExecGraphEditorConfig ExecConfig { get; private set; }

        public override void Init(Graph graph, Rect editorBounds, UnityEngine.Object assetObject, UISystem uiSystem)
        {
            base.Init(graph, editorBounds, assetObject, uiSystem);
            EditorStyle.branding = "Execution Flow";
            ExecConfig = CreateInstance<GridFlowExecGraphEditorConfig>();
        }

        protected override GraphContextMenu CreateContextMenu()
        {
            return new GridFlowExecGraphContextMenu();
        }

        public override GraphSchema GetGraphSchema()
        {
            return new GridFlowExecGraphSchema();
        }

        protected override void InitializeNodeRenderers(GraphNodeRendererFactory nodeRenderers)
        {
            nodeRenderers.RegisterNodeRenderer(typeof(GridFlowExecRuleGraphNode), new GridFlowExecRuleNodeRenderer());
            nodeRenderers.RegisterNodeRenderer(typeof(GridFlowExecResultGraphNode), new GridFlowExecResultNodeRenderer());
            nodeRenderers.RegisterNodeRenderer(typeof(CommentNode), new CommentNodeRenderer(EditorStyle.commentTextColor));
        }

        protected override IGraphLinkRenderer CreateGraphLinkRenderer()
        {
            return new StraightLineGraphLinkRenderer();
        }

        private GridFlowExecNodeHandler GetNodeHandler(GraphNode node)
        {
            if (node != null)
            {
                var ruleNode = node as GridFlowExecRuleGraphNode;
                if (ruleNode != null)
                {
                    return ruleNode.nodeHandler;
                }
            }
            return null;
        }

        public override void OnNodeSelectionChanged(UISystem uiSystem)
        {
            base.OnNodeSelectionChanged(uiSystem);

            // Fetch the first selected node
            var selectedNodes = (from node in graph.Nodes
                                where node.Selected
                                select node).ToArray();

            if (selectedNodes.Length > 0)
            {
                var node = selectedNodes[0];
                if (node is GridFlowExecRuleGraphNode)
                {
                    var ruleNode = node as GridFlowExecRuleGraphNode;
                    uiSystem.Platform.ShowObjectProperty(ruleNode.nodeHandler);
                }
                else
                {
                    uiSystem.Platform.ShowObjectProperty(node);
                }
            }
            else
            {
                uiSystem.Platform.ShowObjectProperty(ExecConfig);
            }
        }

        protected override void OnMenuItemClicked(object userdata, GraphContextMenuEvent e)
        {
            var menuContext = userdata as GridFlowExecGraphContextMenuUserData;

            if (menuContext != null)
            {
                if (menuContext.Action == GridFlowExecGraphEditorAction.CreateRuleNode)
                {
                    CreateExecRuleNode(lastMousePosition, menuContext.NodeHandlerType, e.uiSystem);
                }
                else if (menuContext.Action == GridFlowExecGraphEditorAction.CreateCommentNode)
                {
                    CreateCommentNode(lastMousePosition, e.uiSystem);
                }
            }
        }

        protected override string GetGraphNotInitializedMessage()
        {
            return "Graph not initialized";
        }

        void CreateExecRuleNode(Vector2 screenPos, System.Type nodeHandlerType, UISystem uiSystem)
        {
            if (nodeHandlerType == null)
            {
                return;
            }

            var nodeHandler = ScriptableObject.CreateInstance(nodeHandlerType) as GridFlowExecNodeHandler;
            if (nodeHandler != null)
            {
                nodeHandler.hideFlags = HideFlags.HideInHierarchy;
                uiSystem.Platform.AddObjectToAsset(nodeHandler, assetObject);

                var worldPos = camera.ScreenToWorld(screenPos);
                var execNode = CreateNode<GridFlowExecRuleGraphNode>(worldPos, uiSystem);
                execNode.nodeHandler = nodeHandler;
                execNode.Position = worldPos;

                BringToFront(execNode);
                SelectNode(execNode, uiSystem);
            }
        }

        void CreateCommentNode(Vector2 screenPos, UISystem uiSystem)
        {
            var worldPos = camera.ScreenToWorld(screenPos);
            var commentNode = CreateNode<CommentNode>(worldPos, uiSystem);
            commentNode.Position = worldPos;
            commentNode.background = new Color(0.224f, 1.0f, 0.161f, 0.7f);
            BringToFront(commentNode);
            SelectNode(commentNode, uiSystem);
        }
    }


    public class GridFlowExecGraphSchema : GraphSchema
    {
        public override bool CanCreateLink(GraphPin output, GraphPin input, out string errorMessage)
        {
            errorMessage = "";
            if (output == null || input == null)
            {
                errorMessage = "Invalid connection";
                return false;
            }

            if (input.Node != null)
            {
                input = input.Node.InputPin;
            }

            var sourceNode = output.Node;
            var destNode = input.Node;

            if (sourceNode is GridFlowExecResultGraphNode)
            {
                errorMessage = "Not Allowed: Cannot connect from Result node";
                return false;
            }

            var graph = sourceNode.Graph;
            foreach (var link in graph.Links)
            {
                if (link.Input.Node == input.Node && link.Output.Node == output.Node)
                {
                    errorMessage = "Not Allowed: Already connected";
                    return false;
                }
            }

            return true;
        }
    }

    public enum GridFlowExecGraphEditorAction
    {
        CreateCommentNode,
        CreateRuleNode
    }

    class GridFlowExecGraphContextMenuUserData
    {
        public GridFlowExecGraphContextMenuUserData(UISystem uiSystem, GridFlowExecGraphEditorAction action)
            : this(uiSystem, action, null)
        {
        }

        public GridFlowExecGraphContextMenuUserData(UISystem uiSystem, GridFlowExecGraphEditorAction action, System.Type nodeHandlerType)
        {
            this.uiSystem = uiSystem;
            this.Action = action;
            this.NodeHandlerType = nodeHandlerType;
        }

        public GridFlowExecGraphEditorAction Action { get; set; }
        public System.Type NodeHandlerType { get; set; }
        public UISystem uiSystem { get; set; }
    }

    class GridFlowExecGraphContextMenu : GraphContextMenu
    {
        struct MenuItemInfo
        {
            public MenuItemInfo(string title, float weight, System.Type handlerType)
            {
                this.title = title;
                this.weight = weight;
                this.handlerType = handlerType;
            }

            public string title;
            public float weight;
            public System.Type handlerType;
        }

        public override void Show(GraphEditor graphEditor, GraphPin sourcePin, Vector2 mouseWorld, UISystem uiSystem)
        {
            var menu = uiSystem.Platform.CreateContextMenu();
            var handlerTypes = GetNodeHandlerTypes();
            var items = new List<MenuItemInfo>();
            foreach (var handlerType in handlerTypes)
            {
                string nodeTitle;
                var menuAttribute = GridFlowExecNodeInfoAttribute.GetHandlerAttribute(handlerType);
                var weight = 0.0f;
                if (menuAttribute != null)
                {
                    nodeTitle = menuAttribute.MenuPrefix + menuAttribute.Title;
                    weight = menuAttribute.Weight;
                }
                else {
                    nodeTitle = handlerType.ToString();
                }
                items.Add(new MenuItemInfo(nodeTitle, weight, handlerType));
            }
            items.Sort((a, b) => a.weight < b.weight ? -1 : 1);
            foreach (var item in items)
            {
                menu.AddItem(item.title, HandleContextMenu, new GridFlowExecGraphContextMenuUserData(uiSystem, GridFlowExecGraphEditorAction.CreateRuleNode, item.handlerType));
            }

            menu.AddSeparator("");
            menu.AddItem("Add Comment Node", HandleContextMenu, new GridFlowExecGraphContextMenuUserData(uiSystem, GridFlowExecGraphEditorAction.CreateCommentNode));
            menu.Show();
        }

        void HandleContextMenu(object action)
        {
            var item = action as GridFlowExecGraphContextMenuUserData;
            DispatchMenuItemEvent(action, BuildEvent(null, item.uiSystem));
        }

        System.Type[] GetNodeHandlerTypes()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(GridFlowExecNodeHandler)); // Search the runtime module
            var handlers = from t in assembly.GetTypes()
                           where t.IsClass && t.IsSubclassOf(typeof(GridFlowExecNodeHandler))
                           select t;

            return handlers.ToArray();
        }
    }
}
