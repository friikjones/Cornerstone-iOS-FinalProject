using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonArchitect.Graphs;
using DungeonArchitect.UI.Widgets;
using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.Builders.GridFlow.Graphs.Preview.Abstract;
using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets.GraphEditors;

namespace DungeonArchitect.Editors.GridFlow.Abstract
{
    public delegate void OnGridFlowAbstractItemSelectionChanged(GridFlowItem item);

    public class GridFlowPreviewAbstractGraphEditor : GraphEditor
    {
        private Dictionary<System.Guid, Rect> itemScreenPositions = new Dictionary<System.Guid, Rect>();
        GridFlowItem selectedItem = null;

        public event OnGridFlowAbstractItemSelectionChanged ItemSelectionChanged;

        public override void Init(Graph graph, Rect editorBounds, UnityEngine.Object assetObject, UISystem uiSystem)
        {
            base.Init(graph, editorBounds, assetObject, uiSystem);
            EditorStyle.branding = "Result";
            selectedItem = null;
        }

        protected override GraphContextMenu CreateContextMenu()
        {
            return new DungeonGridFlowAbstractGraphContextMenu();
        }

        protected override void InitializeNodeRenderers(GraphNodeRendererFactory nodeRenderers)
        {
            var abstractNodeRenderer = new GridFlowPreviewAbstractNodeRenderer();
            abstractNodeRenderer.GridFlowAbstractGraphItemRendered += OnGraphItemRendered;
            nodeRenderers.RegisterNodeRenderer(typeof(GridFlowPreviewAbstractGraphNode), abstractNodeRenderer);
            nodeRenderers.RegisterNodeRenderer(typeof(CommentNode), new CommentNodeRenderer(EditorStyle.commentTextColor));
        }

        protected override IGraphLinkRenderer CreateGraphLinkRenderer()
        {
            var linkRenderer = new GridFlowPreviewAbstractLinkRenderer();
            linkRenderer.GridFlowAbstractGraphItemRendered += OnGraphItemRendered;
            return linkRenderer;
        }

        protected override void OnMenuItemClicked(object userdata, GraphContextMenuEvent e)
        {
            var action = (DungeonGridFlowAbstractGraphEditorAction)userdata;

            if (action == DungeonGridFlowAbstractGraphEditorAction.CreateCommentNode)
            {
                var mouseScreen = lastMousePosition;
                CreateCommentNode(mouseScreen, e.uiSystem);
            }
        }

        protected override string GetGraphNotInitializedMessage()
        {
            return "Graph not initialized";
        }

        private void OnGraphItemRendered(GridFlowItem item, Rect screenBounds)
        {
            itemScreenPositions[item.itemId] = screenBounds;
        }

        public void SelectNodeAtCoord(IntVector2 nodeCoord, UISystem uiSystem)
        {
            GridFlowPreviewAbstractGraphNode target = null;
            foreach (var node in graph.Nodes)
            {
                var previewNode = node as GridFlowPreviewAbstractGraphNode;
                if (previewNode != null)
                {
                    if (previewNode.AbstractNodeState.GridCoord.Equals(nodeCoord))
                    {
                        target = previewNode;
                        break;
                    }
                }
            }

            if (target != null)
            {
                SelectNode(target, uiSystem);
            }
        }

        public override void HandleInput(Event e, UISystem uiSystem)
        {
            base.HandleInput(e, uiSystem);


            var buttonId = 0;
            if (e.type == EventType.MouseDown && e.button == buttonId)
            {
                // A button was pressed. Check if any of the items were clicked
                GridFlowItem newSelectedItem = null;
                var items = GridFlowPreviewAbstractGraphUtils.GetAllItems(graph as GridFlowPreviewAbstractGraph);
                foreach (var item in items)
                {
                    if (itemScreenPositions.ContainsKey(item.itemId))
                    {
                        var itemBounds = itemScreenPositions[item.itemId];
                        if (itemBounds.Contains(lastMousePosition))
                        {
                            newSelectedItem = item;
                            break;
                        }
                    }
                }

                if (selectedItem != newSelectedItem)
                {
                    SelectNodeItem(newSelectedItem);
                }
            }
        }

        public void SelectNodeItem(System.Guid itemId)
        {
            if (itemId == System.Guid.Empty)
            {
                SelectNodeItem(null);
                return;
            }

            var items = GridFlowPreviewAbstractGraphUtils.GetAllItems(graph as GridFlowPreviewAbstractGraph);
            foreach (var item in items)
            {
                if (item.itemId == itemId)
                {
                    SelectNodeItem(item);
                    break;
                }
            }
        }

        public void SelectNodeItem(GridFlowItem newSelectedItem)
        {
            if (selectedItem != newSelectedItem)
            {
                var oldSelectedItem = selectedItem;
                selectedItem = newSelectedItem;

                if (oldSelectedItem != null)
                {
                    oldSelectedItem.editorSelected = false;
                }

                if (newSelectedItem != null)
                {
                    newSelectedItem.editorSelected = true;
                }

                if (ItemSelectionChanged != null)
                {
                    ItemSelectionChanged.Invoke(selectedItem);
                }
            }
        }

        public override void Draw(UISystem uiSystem, UIRenderer renderer)
        {
            itemScreenPositions.Clear();

            base.Draw(uiSystem, renderer);

            // Draw the graph item references
            if (graph != null)
            {
                foreach (var node in graph.Nodes)
                {
                    var previewNode = node as GridFlowPreviewAbstractGraphNode;
                    if (previewNode != null)
                    {
                        foreach (var item in previewNode.AbstractNodeState.Items)
                        {
                            foreach (var refItem in item.referencedItemIds)
                            {
                                // Draw a reference between the two bounds
                                if (itemScreenPositions.ContainsKey(item.itemId) && itemScreenPositions.ContainsKey(refItem))
                                {
                                    var startBounds = itemScreenPositions[item.itemId];
                                    var endBounds = itemScreenPositions[refItem];

                                    DrawReferenceLink(renderer, startBounds, endBounds);
                                }
                            }
                        }
                    }
                }
            }
        }

        void DrawReferenceLink(UIRenderer renderer, Rect start, Rect end)
        {
            var centerA = start.center;
            var centerB = end.center;
            var radiusA = Mathf.Max(start.size.x, start.size.y) * 0.5f;
            var radiusB = Mathf.Max(end.size.x, end.size.y) * 0.5f;
            var vecAtoB = (centerB - centerA);
            var lenAtoB = vecAtoB.magnitude;
            var dirAtoB = vecAtoB / lenAtoB;

            var startPos = centerA + dirAtoB * radiusA;
            var endPos = centerA + dirAtoB * (lenAtoB - radiusB);
            StraightLineGraphLinkRenderer.DrawLine(renderer, startPos, endPos, camera, Color.red, 2);
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

    public enum DungeonGridFlowAbstractGraphEditorAction
    {
        CreateCommentNode
    }

    public class DungeonGridFlowAbstractGraphContextMenu : GraphContextMenu
    {
        class ItemInfo
        {
            public ItemInfo(UISystem uiSystem, DungeonGridFlowAbstractGraphEditorAction action)
            {
                this.uiSystem = uiSystem;
                this.action = action;
            }

            public UISystem uiSystem;
            public DungeonGridFlowAbstractGraphEditorAction action;
        }

        public override void Show(GraphEditor graphEditor, GraphPin sourcePin, Vector2 mouseWorld, UISystem uiSystem)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Comment Node"), false, HandleContextMenu, new ItemInfo(uiSystem, DungeonGridFlowAbstractGraphEditorAction.CreateCommentNode));
            menu.ShowAsContext();
        }

        void HandleContextMenu(object userdata)
        {
            var item = userdata as ItemInfo;
            DispatchMenuItemEvent(item.action, BuildEvent(null, item.uiSystem));
        }
    }

}
