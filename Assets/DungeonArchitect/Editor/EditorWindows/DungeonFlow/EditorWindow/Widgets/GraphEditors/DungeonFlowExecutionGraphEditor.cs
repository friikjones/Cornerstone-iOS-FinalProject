using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets;
using DungeonArchitect.Grammar;
using DungeonArchitect.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DungeonArchitect.UI.Widgets.GraphEditors;

namespace DungeonArchitect.Editors.DungeonFlow
{
    public enum DungeonFlowExecutionGraphEditorAction
    {
        CreateRuleNode,
        CreateCommentNode
    }

    class DungeonFlowExecutionGraphEditorMenuData
    {
        public DungeonFlowExecutionGraphEditorMenuData(UISystem uiSystem, DungeonFlowExecutionGraphEditorAction action)
            : this(uiSystem, action, null)
        {
        }
        public DungeonFlowExecutionGraphEditorMenuData(UISystem uiSystem, DungeonFlowExecutionGraphEditorAction action, GrammarProductionRule rule)
        {
            this.Action = action;
            this.uiSystem = uiSystem;
            this.Rule = rule;
        }

        public DungeonFlowExecutionGraphEditorAction Action;
        public UISystem uiSystem;
        public GrammarProductionRule Rule;
    }

    public class DungeonFlowExecutionGraphSchema : GraphSchema
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

            if (destNode is GrammarExecEntryNode)
            {
                errorMessage = "Not Allowed: Cannot connect to entry node";
                return false;
            }

            // Make sure we don't already have this connection
            foreach (var link in output.GetConntectedLinks())
            {
                if (link.Input == input)
                {
                    errorMessage = "Not Allowed: Already connected";
                    return false;
                }
            }

            return true;
        }
    }

    public class DungeonFlowExecutionGraphEditor : GraphEditor
    {
        public DungeonFlowAsset FlowAsset { get; private set; }
        public override void Init(Graph graph, Rect editorBounds, UnityEngine.Object assetObject, UISystem uiSystem)
        {
            FlowAsset = assetObject as DungeonFlowAsset;

            base.Init(graph, editorBounds, assetObject, uiSystem);
        }

        GrammarProductionRule GetDragData()
        {
            var dragDropData = DragAndDrop.GetGenericData(RuleListViewConstants.DragDropID);
            if (dragDropData != null && dragDropData is GrammarProductionRule)
            {
                return dragDropData as GrammarProductionRule;
            }
            return null;
        }

        public override GraphSchema GetGraphSchema()
        {
            return new DungeonFlowExecutionGraphSchema();
        }

        protected override IGraphLinkRenderer CreateGraphLinkRenderer()
        {
            return new StraightLineGraphLinkRenderer();
        }

        public override T CreateLink<T>(Graph graph, GraphPin output, GraphPin input, UISystem uiSystem)
        {
            if (input != null && input.Node != null)
            {
                input = input.Node.InputPin;
            }

            // If we have a link in the opposite direction, then break that link
            var sourceNode = output.Node;
            var destNode = input.Node;
            if (sourceNode != null && destNode != null)
            {
                var links = destNode.OutputPin.GetConntectedLinks();
                foreach (var link in links)
                {
                    if (link.Output.Node == input.Node && link.Input.Node == output.Node)
                    {
                        GraphOperations.DestroyLink(link, uiSystem.Undo);
                    }
                }

                if (sourceNode is GrammarExecEntryNode)
                {
                    // Destroy all outgoing links first
                    var outgoingLinks = output.GetConntectedLinks();
                    foreach (var link in outgoingLinks)
                    {
                        GraphOperations.DestroyLink(link, uiSystem.Undo);
                    }
                }
            }

            if (input.Node != null)
            {
                input = input.Node.InputPin;
                if (input != null)
                {
                    return base.CreateLink<T>(graph, output, input, uiSystem);
                }
            }
            return null;
        }

        bool IsDragDataSupported()
        {
            return GetDragData() != null;
        }

        public override void Draw(UISystem uiSystem, UIRenderer renderer)
        {
            base.Draw(uiSystem, renderer);


            bool isDragging = (uiSystem != null && uiSystem.IsDragDrop);
            if (isDragging && IsDragDataSupported())
            {
                // Show the drag drop overlay
                var bounds = new Rect(Vector2.zero, WidgetBounds.size);
                var dragOverlayColor = new Color(1, 0, 0, 0.25f);
                renderer.DrawRect(bounds, dragOverlayColor);
            }
        }

        public override void HandleInput(Event e, UISystem uiSystem)
        {
            base.HandleInput(e, uiSystem);

            switch (e.type)
            {
                case EventType.DragUpdated:
                    if (IsDragDataSupported())
                    {
                        HandleDragUpdate(e, uiSystem);
                    }
                    break;

                case EventType.DragPerform:
                    if (IsDragDataSupported())
                    {
                        HandleDragPerform(e, uiSystem);
                    }
                    break;
            }
        }

        private void HandleDragUpdate(Event e, UISystem uiSystem)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }

        private void HandleDragPerform(Event e, UISystem uiSystem)
        {
            // TODO: Create a node here
            var rule = GetDragData();
            if (rule != null)
            {
                if (uiSystem != null)
                {
                    uiSystem.RequestFocus(this);
                }

                var ruleNode = CreateNewExecRuleNode(rule, e.mousePosition, uiSystem);
                SelectNode(ruleNode, uiSystem);
            }

            DragAndDrop.AcceptDrag();
        }

        GrammarExecRuleNode CreateNewExecRuleNode(GrammarProductionRule rule, Vector2 mousePosition, UISystem uiSystem)
        {
            var node = CreateNode<GrammarExecRuleNode>(mousePosition, uiSystem);
            node.rule = rule;

            // Adjust the initial position of the placed node
            {
                var nodeRenderer = nodeRenderers.GetRenderer(node.GetType());
                if (nodeRenderer is GrammarNodeRendererBase)
                {
                    var grammarNodeRenderer = nodeRenderer as GrammarNodeRendererBase;
                    grammarNodeRenderer.UpdateNodeBounds(node, 1.0f);
                }

                var mouseWorld = camera.ScreenToWorld(mousePosition);
                var bounds = node.Bounds;
                bounds.position = mouseWorld - bounds.size / 2.0f;
                node.Bounds = bounds;
            }

            return node;

        }

        protected override GraphContextMenu CreateContextMenu()
        {
            return new DungeonFlowExecutionGraphContextMenu();
        }

        protected override void InitializeNodeRenderers(GraphNodeRendererFactory nodeRenderers)
        {
            nodeRenderers.RegisterNodeRenderer(typeof(CommentNode), new CommentNodeRenderer(EditorStyle.commentTextColor));
            nodeRenderers.RegisterNodeRenderer(typeof(GrammarExecRuleNode), new GrammarExecRuleNodeRenderer());
            nodeRenderers.RegisterNodeRenderer(typeof(GrammarExecEntryNode), new GrammarExecEntryNodeRenderer());
        }

        protected override void OnMenuItemClicked(object userdata, GraphContextMenuEvent e)
        {
            var data = userdata as DungeonFlowExecutionGraphEditorMenuData;

            var mouseScreen = lastMousePosition;
            if (data.Action == DungeonFlowExecutionGraphEditorAction.CreateRuleNode)
            {
                var ruleNode = CreateNewExecRuleNode(data.Rule, LastMousePosition, e.uiSystem);
                if (ruleNode != null)
                {
                    CreateLinkBetweenPins(e.sourcePin, ruleNode.InputPin, e.uiSystem);
                    SelectNode(ruleNode, e.uiSystem);
                }
            }
            else if (data.Action == DungeonFlowExecutionGraphEditorAction.CreateCommentNode)
            {
                CreateCommentNode(mouseScreen, e.uiSystem);
            }

        }

        protected override void DrawHUD(UISystem uiSystem, UIRenderer renderer, Rect bounds) { }

        void CreateCommentNode(Vector2 screenPos, UISystem uiSystem)
        {
            var worldPos = camera.ScreenToWorld(screenPos);
            var commentNode = CreateNode<CommentNode>(worldPos, uiSystem);
            commentNode.Position = worldPos;
            commentNode.background = new Color(0.224f, 1.0f, 0.161f, 0.7f);
            BringToFront(commentNode);
            SelectNode(commentNode, uiSystem);
        }

        protected override string GetGraphNotInitializedMessage()
        {
            return "Graph not initialize";
        }

        protected override GraphEditorStyle CreateEditorStyle()
        {
            var editorStyle = base.CreateEditorStyle();
            editorStyle.branding = "Execution Graph";
            editorStyle.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            return editorStyle;
        }
    }
}
