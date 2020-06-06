using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonArchitect.Graphs;
using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;
using DungeonArchitect.Builders.GridFlow.Graphs.Preview.Abstract;
using DungeonArchitect.Builders.GridFlow.Graphs.Preview.Tilemap;
using DungeonArchitect.Editors.GridFlow.Abstract;
using DungeonArchitect.Editors.GridFlow.Tilemap;
using DungeonArchitect.Builders.GridFlow.Graphs.Abstract;
using DungeonArchitect.Builders.GridFlow.Tilemap;
using DungeonArchitect.UI.Widgets.GraphEditors;
using DungeonArchitect.UI.Impl.UnityEditor;

namespace DungeonArchitect.Editors.GridFlow
{
    public class DungeonGridFlowEditorWindow : EditorWindow
    {
        GraphPanel<GridFlowExecGraphEditor> execGraphPanel;
        GraphPanel<GridFlowPreviewAbstractGraphEditor> resultAbstractGraphPanel;
        GraphPanel<GridFlowPreviewTilemapGraphEditor> resultTilemapPanel;

        BorderWidget resultHost;
        IWidget resultLayoutAbstractGraph;
        IWidget resultLayoutTilemap;
        IWidget resultLayoutEmpty;

        UISystem uiSystem;
        UIRenderer renderer;
        List<IDeferredUICommand> deferredCommands = new List<IDeferredUICommand>();

        GridFlowPreviewAbstractGraph previewAbstractGraph;
        GridFlowPreviewTilemapGraph previewTilemap;
        GridFlowExecNodeStates previewNodeStates = null;

        GridFlowAbstractNodeState selectedAbstractNodeState = null;
        GridFlowItem selectedAbstractItem = null;

        bool tilemapDirty = false;
        GridFlowTilemap renderedTilemap = null;
        GridFlowAbstractGraph renderedAbstractGraph = null;
        IntVector2 lastTilemapCellClicked = IntVector2.Zero;

        readonly static string CMD_EXEC_GRAMMAR = "CmdExecuteGrammar";
        readonly static string CMD_SHOW_SETTINGS = "CmdSettings";

        public DungeonGridFlowAsset flowAsset;

        public void Init(DungeonGridFlowAsset flowAsset)
        {
            CreateUISystem();
            this.flowAsset = flowAsset;
            titleContent = new GUIContent("Grid Flow Editor");

            if (flowAsset == null)
            {
                uiSystem.ClearLayout();
                return;
            }

            // Build the exec graph panel
            {
                var execToolbar = new ToolbarWidget();
                execToolbar.ButtonSize = 24;
                execToolbar.Padding = 4;
                execToolbar.Background = new Color(0, 0, 0, 0);
                execToolbar.AddButton(CMD_EXEC_GRAMMAR, UIResourceLookup.ICON_PLAY_16x);
                execToolbar.AddButton(CMD_SHOW_SETTINGS, UIResourceLookup.ICON_SETTINGS_16x);
                execToolbar.ButtonPressed += ExecToolbar_ButtonPressed;

                execGraphPanel = new GraphPanel<GridFlowExecGraphEditor>(flowAsset.execGraph, flowAsset, uiSystem, execToolbar);
                execGraphPanel.GraphEditor.Events.OnNodeSelectionChanged.Event += OnExecNodeSelectionChanged;
                execGraphPanel.Border.SetTitle("Execution Graph");
                execGraphPanel.Border.SetColor(new Color(0.2f, 0.2f, 0.5f));
            }

            // Build the result graph panels
            {
                previewAbstractGraph = CreateInstance<GridFlowPreviewAbstractGraph>();
                resultAbstractGraphPanel = new GraphPanel<GridFlowPreviewAbstractGraphEditor>(previewAbstractGraph, null, uiSystem);
                resultAbstractGraphPanel.Border.SetTitle("Result: Layout Graph");
                resultAbstractGraphPanel.Border.SetColor(new Color(0.2f, 0.3f, 0.2f));
                resultAbstractGraphPanel.GraphEditor.EditorStyle.branding = "Layout";
                resultAbstractGraphPanel.GraphEditor.EditorStyle.displayAssetFilename = false;
                resultAbstractGraphPanel.GraphEditor.ItemSelectionChanged += AbstractGraphEditor_ItemSelectionChanged;
                resultAbstractGraphPanel.GraphEditor.Events.OnNodeSelectionChanged.Event += AbstractGraphEditor_OnNodeSelectionChanged;
                resultAbstractGraphPanel.GraphEditor.Events.OnNodeDoubleClicked.Event += AbstractGraphEditor_OnNodeDoubleClicked;

                previewTilemap = CreateInstance<GridFlowPreviewTilemapGraph>();
                resultTilemapPanel = new GraphPanel<GridFlowPreviewTilemapGraphEditor>(previewTilemap, null, uiSystem);
                resultTilemapPanel.Border.SetTitle("Result: Tilemap");
                resultTilemapPanel.Border.SetColor(new Color(0.3f, 0.2f, 0.2f));
                resultTilemapPanel.GraphEditor.TileClicked += TilemapGraphEditor_TileClicked;
                resultTilemapPanel.GraphEditor.Events.OnNodeDoubleClicked.Event += TilemapGraphEditor_OnNodeDoubleClicked;
                resultTilemapPanel.GraphEditor.EditorStyle.branding = "Tilemap";
                resultTilemapPanel.GraphEditor.EditorStyle.displayAssetFilename = false;
            }

            BuildLayout();
        }

        public void SetReadOnly(bool readOnly)
        {
            if (execGraphPanel != null)
            {
                execGraphPanel.GraphEditor.SetReadOnly(readOnly);
            }

            if (resultAbstractGraphPanel != null)
            {
                resultAbstractGraphPanel.GraphEditor.SetReadOnly(readOnly);
            }

            if (resultTilemapPanel != null)
            {
                resultTilemapPanel.GraphEditor.SetReadOnly(readOnly);
            }
        }

        private void TilemapGraphEditor_OnNodeDoubleClicked(object sender, GraphNodeEventArgs e)
        {
            if (renderedTilemap != null)
            {
                bool valid = false;
                var bounds = new Bounds();
                var cellId = lastTilemapCellClicked.y * renderedTilemap.Width + lastTilemapCellClicked.x;
                var items = GameObject.FindObjectsOfType<DungeonSceneProviderData>();
                GameObject selectedObject = null;
                foreach (var item in items)
                {
                    if (item.userData == cellId)
                    {
                        var renderer = item.gameObject.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            if (!valid)
                            {
                                valid = true;
                                bounds = renderer.bounds;
                            }
                            else
                            {
                                bounds.Encapsulate(renderer.bounds);
                            }
                        }

                        selectedObject = item.gameObject;
                    }
                }

                if (valid && SceneView.lastActiveSceneView != null)
                {
                    bounds.center += new Vector3(0, bounds.extents.y, 0);
                    bounds.extents = new Vector3(bounds.extents.x, 1, bounds.extents.z);
                    
                    SceneView.lastActiveSceneView.Frame(bounds, false);
                }
                if (selectedObject != null)
                {
                    Selection.activeGameObject = selectedObject;
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

        private void AbstractGraphEditor_OnNodeDoubleClicked(object sender, GraphNodeEventArgs e)
        {

        }

        void CreateUISystem()
        {
            uiSystem = new UnityEditorUISystem();
            renderer = new UnityEditorUIRenderer();
        }

        private void TilemapGraphEditor_TileClicked(GridFlowTilemap tilemap, int tileX, int tileY)
        {
            var cell = tilemap.Cells[tileX, tileY];
            if (cell.CellType == GridFlowTilemapCellType.Floor)
            {
                var nodeCoord = cell.NodeCoord;
                resultAbstractGraphPanel.GraphEditor.SelectNodeAtCoord(nodeCoord, uiSystem);
            }

            resultAbstractGraphPanel.GraphEditor.SelectNodeItem(cell.Item);

            lastTilemapCellClicked.Set(tileX, tileY);
        }

        private void AbstractGraphEditor_OnNodeSelectionChanged(object sender, GraphNodeEventArgs e)
        {
            var previewNode = (e.Nodes.Length == 1) ? e.Nodes[0] as GridFlowPreviewAbstractGraphNode : null;
            var previewState = (previewNode != null) ? previewNode.AbstractNodeState : null;
            
            if (previewState != selectedAbstractNodeState)
            {
                selectedAbstractNodeState = previewState;
            }
            tilemapDirty = true;
        }

        private void AbstractGraphEditor_ItemSelectionChanged(GridFlowItem item)
        {
            if (item != selectedAbstractItem)
            {
                selectedAbstractItem = item;
                tilemapDirty = true;
            }
        }

        private void OnExecNodeSelectionChanged(object sender, GraphNodeEventArgs e)
        {
            var selectedNode = (e.Nodes.Length > 0) ? e.Nodes[0] as GridFlowExecRuleGraphNode : null;
            GridFlowExecRuleGraphNode targetNode = (selectedNode != null) ? selectedNode : flowAsset.execGraph.resultNode;
            UpdatePreview(targetNode);
        }

        void BuildLayout()
        {
            resultHost = new BorderWidget()
                .SetPadding(0, 0, 0, 0);

            resultLayoutAbstractGraph = resultAbstractGraphPanel;

            resultLayoutTilemap =
                new Splitter(SplitterDirection.Horizontal)
                    .AddWidget(resultAbstractGraphPanel, 1)
                    .AddWidget(resultTilemapPanel, 1);

            resultLayoutEmpty = new BorderWidget()
                .SetPadding(0, 0, 0, 0)
                .SetColor(new Color(0.15f, 0.15f, 0.15f))
                .SetContent(
                    new LabelWidget("Selected node has no data")
                        .SetColor(new Color(1, 1, 1, 0.5f))
                        .SetFontSize(24)
                        .SetTextAlign(TextAnchor.MiddleCenter));

            resultHost.SetContent(resultLayoutAbstractGraph);

            var layout = new Splitter(SplitterDirection.Vertical)
                    .AddWidget(execGraphPanel, 1)
                    .AddWidget(resultHost, 2);

            uiSystem.SetLayout(layout);

            deferredCommands.Add(new EditorCommand_InitializeGraphCameras(layout));
        }

        private void ExecToolbar_ButtonPressed(UISystem uiSystem, string id)
        {
            if (flowAsset == null)
            {
                Debug.Log("Cannot execute graph. Invalid flow asset");
                return;
            }

            if (id == CMD_EXEC_GRAMMAR)
            {
                HandleExecuteButtonPressed();
            }
            else if (id == CMD_SHOW_SETTINGS)
            {
                uiSystem.Platform.ShowObjectProperty(execGraphPanel.GraphEditor.ExecConfig);
            }
        }

        public void HandleExecuteButtonPressed()
        {
            ExecuteGraph();

            // Select the result node
            execGraphPanel.GraphEditor.SelectNode(flowAsset.execGraph.resultNode, uiSystem);

            UpdatePreview(flowAsset.execGraph.resultNode);
        }

        private void ExecuteGraph()
        {
            var execConfig = execGraphPanel.GraphEditor.ExecConfig;
            if (execConfig.randomizeSeed)
            {
                execConfig.seed = new System.Random().Next();
            }

            if (execConfig.dungeonObject != null)
            {
                var dungeonGameObject = execConfig.dungeonObject.gameObject;
                if (dungeonGameObject != null)
                {
                    if (execConfig.randomizeSeed)
                    {
                        var dungeonConfig = dungeonGameObject.GetComponent<GridFlowDungeonConfig>();
                        if (dungeonConfig != null)
                        {
                            dungeonConfig.Seed = (uint)execConfig.seed;
                        }
                    }
                    var dungeon = dungeonGameObject.GetComponent<Dungeon>();
                    if (dungeon != null)
                    {
                        dungeon.Build(new EditorDungeonSceneObjectInstantiator());
                        DungeonEditorHelper.MarkSceneDirty();
                    }
                }
            }

            var execGraph = flowAsset.execGraph;
            var random = new System.Random(execConfig.seed);

            GridFlowExecutor executor = new GridFlowExecutor();
            if (!executor.Execute(execGraph, random, 100, out previewNodeStates))
            {
                Debug.LogError("Failed to produce graph");
            }
        }

        void UpdatePreview(GridFlowExecRuleGraphNode node)
        {
            if (node == null || node.nodeHandler == null) return;
            if (previewNodeStates == null) return;
            var execState = previewNodeStates.Get(node.Id);

            if (execState == null)
            {
                resultHost.SetContent(resultLayoutEmpty);
                return;
            }

            if (execState is GridFlowExecNodeState_AbstractGraph)
            {
                // Show the result layout for abstract graphs
                resultHost.SetContent(resultLayoutAbstractGraph);

                // Build the abstract graph preview
                if (execState.AbstractGraph != null)
                {
                    GridFlowPreviewAbstractGraphBuilder.Build(execState.AbstractGraph, new NonEditorGraphBuilder(previewAbstractGraph));
                }

                renderedAbstractGraph = execState.AbstractGraph;
                renderedTilemap = null;
            }
            else if (execState is GridFlowExecNodeState_Tilemap)
            {
                // Show the result layout for tilemap
                resultHost.SetContent(resultLayoutTilemap);

                // Build the abstract graph preview
                if (execState.AbstractGraph != null)
                {
                    GridFlowPreviewAbstractGraphBuilder.Build(execState.AbstractGraph, new NonEditorGraphBuilder(previewAbstractGraph));
                }

                renderedAbstractGraph = execState.AbstractGraph;
                renderedTilemap = execState.Tilemap;

                // Build the tilemap preview graph
                if (execState.Tilemap != null)
                {
                    RebuildTilemap();
                }
            }

            // Request a layout update
            var windowBounds = new Rect(Vector2.zero, position.size);
            deferredCommands.Add(new EditorCommand_UpdateWidget(uiSystem.Layout, windowBounds));
            deferredCommands.Add(new EditorCommand_InitializeGraphCameras(resultHost));
        }

        void RebuildTilemap()
        {
            if (renderedTilemap != null)
            {
                var context = new GridFlowPreviewTilemapBuildContext();
                context.tilemap = renderedTilemap;
                context.abstractGraph = renderedAbstractGraph;
                context.graphBuilder = new NonEditorGraphBuilder(previewTilemap);
                context.selectedNodeState = selectedAbstractNodeState;
                context.selectedItem = selectedAbstractItem;
                GridFlowPreviewTilemapGraphBuilder.Build(context);

                resultTilemapPanel.GraphEditor.UpdateGridSpacing();
            }
        }

        void OnEnable()
        {
            this.wantsMouseMove = true;

            Init(flowAsset);

            var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
            graphEditors.ForEach(g => g.OnEnable());

            DungeonPropertyEditorHook.Get().DungeonBuilt += OnLinkedDungeonBuilt;
        }

        void OnDisable()
        {
            var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
            graphEditors.ForEach(g => g.OnDisable());

            DungeonPropertyEditorHook.Get().DungeonBuilt -= OnLinkedDungeonBuilt;
        }

        void OnDestroy()
        {
            var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
            graphEditors.ForEach(g => {
                g.OnDisable();
                g.OnDestroy();
            });

            if (previewAbstractGraph != null)
            {
                DestroyImmediate(previewAbstractGraph);
                previewAbstractGraph = null;
            }

            DungeonPropertyEditorHook.Get().DungeonBuilt -= OnLinkedDungeonBuilt;
        }

        void Update()
        {
            if (uiSystem == null || renderer == null)
            {
                CreateUISystem();
            }

            var windowStateInvalid = (previewAbstractGraph == null || previewTilemap == null);
            if (windowStateInvalid)
            {
                Init(flowAsset);
            }

            if (uiSystem != null)
            {
                var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
                graphEditors.ForEach(g => g.Update());
            }

            if (tilemapDirty)
            {
                RebuildTilemap();
                tilemapDirty = false;
            }
        }

        private void OnLinkedDungeonBuilt(Dungeon dungeon)
        {
            if (dungeon == null || flowAsset == null || flowAsset.execGraph == null || flowAsset.execGraph.resultNode == null) return;
            if (execGraphPanel == null) return;

            var linkedDungeonBuilder = execGraphPanel.GraphEditor.ExecConfig.dungeonObject;
            if (linkedDungeonBuilder == null || linkedDungeonBuilder.gameObject == null) return;
            var linkedDungeon = linkedDungeonBuilder.gameObject.GetComponent<Dungeon>();
            if (dungeon != linkedDungeon) return;

            previewNodeStates = linkedDungeonBuilder.ExecNodeStates;

            var resultNode = flowAsset.execGraph.resultNode;
            if (resultNode != null)
            {
                // Select the result node
                execGraphPanel.GraphEditor.SelectNode(resultNode, uiSystem);
                UpdatePreview(resultNode);
                uiSystem.Platform.ShowObjectProperty(dungeon.gameObject);
                Repaint();
            }

        }

        void UpdateDragDropState(Event e)
        {
            if (uiSystem != null)
            {
                if (e.type == EventType.DragUpdated)
                {
                    uiSystem.SetDragging(true);
                }
                else if (e.type == EventType.DragPerform || e.type == EventType.DragExited)
                {
                    uiSystem.SetDragging(false);
                }
            }
        }

        void ProcessDeferredCommands()
        {
            // Execute the deferred UI commands
            foreach (var command in deferredCommands)
            {
                command.Execute(uiSystem);
            }

            deferredCommands.Clear();
        }
        
        void OnGUI()
        {
            if (uiSystem == null || renderer == null)
            {
                CreateUISystem();
            }

            // Draw the background
            var bounds = new Rect(Vector2.zero, position.size);
            renderer.DrawRect(bounds, new Color(0.5f, 0.5f, 0.5f));

            if (uiSystem.Layout == null)
            {
                BuildLayout();
            }
            uiSystem.Update(bounds);
            uiSystem.Draw(renderer);

            ProcessDeferredCommands();

            HandleInput(Event.current);
        }

        void HandleInput(Event e)
        {
            if (uiSystem == null || renderer == null)
            {
                CreateUISystem();
            }

            if (uiSystem != null && uiSystem.Layout != null)
            {
                var layout = uiSystem.Layout;
                if (e.type == EventType.MouseDown || e.type == EventType.ScrollWheel)
                {
                    WidgetUtils.ProcessInputFocus(e.mousePosition, uiSystem, layout);
                }

                if (uiSystem.IsDragDrop)
                {
                    WidgetUtils.ProcessDragOperation(e, layout, uiSystem);
                }

                UpdateDragDropState(e);

                if (uiSystem.FocusedWidget != null)
                {
                    Vector2 resultMousePosition = Vector2.zero;
                    if (WidgetUtils.BuildWidgetEvent(e.mousePosition, layout, uiSystem.FocusedWidget, ref resultMousePosition))
                    {
                        Event widgetEvent = new Event(e);
                        widgetEvent.mousePosition = resultMousePosition;
                        uiSystem.FocusedWidget.HandleInput(widgetEvent, uiSystem);
                    }
                }
            }

            if (e.isScrollWheel)
            {
                Repaint();
            }

            switch (e.type)
            {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.KeyDown:
                case EventType.KeyUp:
                case EventType.MouseEnterWindow:
                case EventType.MouseLeaveWindow:
                    Repaint();
                    break;
            }
        }

        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<DungeonGridFlowEditorWindow>();
            window.Init(null);
        }
    }
}