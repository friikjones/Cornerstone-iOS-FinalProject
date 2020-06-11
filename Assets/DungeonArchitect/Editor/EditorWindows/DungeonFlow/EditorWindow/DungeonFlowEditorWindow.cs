using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonArchitect.Grammar;
using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets;
using DungeonArchitect.Graphs;
using DungeonArchitect.UI.Widgets.GraphEditors;
using DungeonArchitect.UI.Impl.UnityEditor;

namespace DungeonArchitect.Editors.DungeonFlow
{
    public class DungeonFlowEditorWindow : EditorWindow
    {
        ProductionRuleWidget ruleEditor;
        RuleListPanel ruleListPanel;
        NodeListPanel nodeListPanel;
        ErrorListPanel errorListPanel;

        GraphPanel<DungeonFlowResultGraphEditor> resultGraphPanel; 
        GraphPanel<DungeonFlowExecutionGraphEditor> execGraphPanel;

        DungeonFlowErrorList errorList;
        UIRenderer renderer;
        public UISystem uiSystem { get; private set; }
        public DungeonFlowAsset FlowAsset { get; set; }

        List<IDeferredUICommand> deferredCommands = new List<IDeferredUICommand>();

        readonly static string CMD_EXEC_GRAMMAR = "ExecuteGrammar";

        public void Init(DungeonFlowAsset flowAsset)
        {
            titleContent = new GUIContent("Dungeon Flow Editor");
            this.FlowAsset = flowAsset;
            ruleEditor = new ProductionRuleWidget();
            CreateUISystem();

            // Build the result graph
            {
                var resultGraph = flowAsset ? flowAsset.resultGraph : null;
                resultGraphPanel = new GraphPanel<DungeonFlowResultGraphEditor>(resultGraph, flowAsset, uiSystem);
                resultGraphPanel.Border.SetTitle("Result Graph");
                resultGraphPanel.Border.SetColor(new Color(0.2f, 0.3f, 0.2f));
                LoadResultGraphEditorState();
            }

            // Build the execution graph
            {
                var execToolbar = new ToolbarWidget();
                execToolbar.ButtonSize = 24;
                execToolbar.Padding = 4;
                execToolbar.Background = new Color(0, 0, 0, 0);
                execToolbar.AddButton(CMD_EXEC_GRAMMAR, UIResourceLookup.ICON_PLAY_16x);
                execToolbar.ButtonPressed += ExecToolbar_ButtonPressed;

                var graph = flowAsset ? flowAsset.executionGraph : null;
                execGraphPanel = new GraphPanel<DungeonFlowExecutionGraphEditor>(graph, flowAsset, uiSystem, execToolbar);
                execGraphPanel.Border.SetTitle("Execution Graph");
                execGraphPanel.Border.SetColor(new Color(0.2f, 0.2f, 0.5f));
            }

            // Build the rule list view
            {
                ruleListPanel = new RuleListPanel(flowAsset);
                ruleListPanel.ListView.SelectionChanged += RuleListView_SelectionChanged;
                ruleListPanel.ListView.ItemClicked += RuleListView_ItemClicked;
            }

            // Build the node list view
            {
                nodeListPanel = new NodeListPanel(flowAsset);
                nodeListPanel.ListView.SelectionChanged += NodeListView_SelectionChanged;
                nodeListPanel.ListView.ItemClicked += NodeListView__ItemClicked;
            }

            // Build the error list panel
            {
                errorList = new DungeonFlowErrorList();
                errorListPanel = new ErrorListPanel(errorList);
                errorListPanel.ListView.ItemDoubleClicked += ErrorListView_ItemDoubleClicked;
            }

            BuildLayout();
            
            // Select the first rule for modification
            ruleListPanel.ListView.SetSelectedIndex(0);
        }

        void CreateUISystem()
        {
            uiSystem = new UnityEditorUISystem();
            renderer = new UnityEditorUIRenderer();
        }

        private void ExecToolbar_ButtonPressed(UISystem uiSystem, string id)
        {
            if (id == CMD_EXEC_GRAMMAR)
            {
                ExecuteGraphGrammar(uiSystem);
            }
        }

        private void ExecuteGraphGrammar(UISystem uiSystem)
        {
            var settings = new GraphGrammarProcessorSettings();
            settings.seed = Random.Range(0, 100000);
            settings.runGraphGenerationScripts = false;
            var processor = new GraphGrammarProcessor(FlowAsset, settings);
            processor.Build();

            if (FlowAsset != null && FlowAsset.resultGraph != null)
            {
                resultGraphPanel.GraphEditor.RefreshGraph(FlowAsset.resultGraph, processor.Grammar.ResultGraph, uiSystem);
                SaveResultGraphEditorState();
            }
        }

        private void RuleListView_ItemClicked(GrammarProductionRule rule)
        {
            UpdateProductionRuleGraphCameras(rule);
            Selection.activeObject = rule;
        }

        private void NodeListView__ItemClicked(GrammarNodeType nodeType)
        {
            Selection.activeObject = nodeType;
        }

        private void ErrorListView_ItemDoubleClicked(DungeonFlowErrorEntry Item)
        {
            if (Item.Action != null)
            {
                Item.Action.Execute(this);
            }
        }

        private void NodeListView_SelectionChanged(GrammarNodeType nodeType)
        {
        }

        private void RuleListView_SelectionChanged(GrammarProductionRule rule)
        {
            UpdateProductionRuleGraphCameras(rule);
        }

        void UpdateProductionRuleGraphCameras(GrammarProductionRule rule)
        {
            if (ruleEditor != null)
            {
                ruleEditor.Init(FlowAsset, rule, uiSystem);
                ruleEditor.UpdateWidget(uiSystem, ruleEditor.WidgetBounds);

                deferredCommands.Add(new EditorCommand_InitializeGraphCameras(ruleEditor));
            }
        }


        public IWidget GetRootLayout() { return uiSystem.Layout; }
        public ProductionRuleWidget GetProductionRuleWidget() { return ruleEditor; }
        public RuleListPanel GetRuleListPanel() { return ruleListPanel; }
        public NodeListPanel GetNodeListPanel() { return nodeListPanel; }
        public ErrorListPanel GetErrorListPanel() { return errorListPanel; }
        public GraphPanel<DungeonFlowExecutionGraphEditor> GetExecGraphPanel() { return execGraphPanel; }
        public GraphPanel<DungeonFlowResultGraphEditor> GetResultGraphPanel() { return resultGraphPanel; }

        public void AddDeferredCommand(IDeferredUICommand command)
        {
            deferredCommands.Add(command);
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


        void ProcessDeferredCommands()
        {
            // Execute the deferred UI commands
            foreach (var command in deferredCommands)
            {
                command.Execute(uiSystem);
            }

            deferredCommands.Clear();
        }

        void PerformValidation()
        {
            if (errorListPanel == null)
            {
                return;
            }

            int selectedIndex = errorListPanel.ListView.GetSelectedIndex();
            var scrollPosition = errorListPanel.ListView.ScrollView.ScrollPosition;

            errorList.Errors.Clear();
            DungeonFlowValidator.Validate(FlowAsset, errorList);

            // Notify the list view that the data has changed and restore the selected index after a reload
            errorListPanel.ListView.NotifyDataChanged();
            errorListPanel.ListView.SetSelectedIndex(selectedIndex);
            errorListPanel.ListView.ScrollView.ScrollPosition = scrollPosition;
        }

        void BuildLayout()
        {
            var layout = new Splitter(SplitterDirection.Horizontal)
                .AddWidget(
                    new Splitter(SplitterDirection.Vertical)
                    .AddWidget(ruleListPanel)
                    .AddWidget(nodeListPanel)
                )
                .AddWidget(
                    new Splitter(SplitterDirection.Vertical)
                    .AddWidget(ruleEditor, 5)
                    .AddWidget(errorListPanel)
                , 2)
                .AddWidget(
                    new Splitter(SplitterDirection.Vertical)
                    .AddWidget(execGraphPanel)
                    .AddWidget(resultGraphPanel)
                , 2)
            ;

            uiSystem.SetLayout(layout);

            deferredCommands.Add(new EditorCommand_InitializeGraphCameras(layout));
        }


        void OnEnable()
        {
            this.wantsMouseMove = true;

            Init(FlowAsset);

            var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
            graphEditors.ForEach(g => g.OnEnable());
        }

        void OnDisable()
        {
            var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
            graphEditors.ForEach(g => g.OnDisable());
        }

        void OnDestroy()
        {
            var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
            graphEditors.ForEach(g => {
                g.OnDisable();
                g.OnDestroy();
            });
        }

        void Update()
        {
            if (uiSystem == null || renderer == null)
            {
                CreateUISystem();
            }

            var graphEditors = WidgetUtils.GetWidgetsOfType<GraphEditor>(uiSystem.Layout);
            graphEditors.ForEach(g => g.Update());
        }

        void OnInspectorUpdate()
        {
            PerformValidation();

            Repaint();
        }

        public void ForceUpdateWidgetFromCache(IWidget widget)
        {
            widget.UpdateWidget(uiSystem, widget.WidgetBounds);
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

        private void SaveResultGraphEditorState()
        {
            if (FlowAsset != null && FlowAsset.resultGraph != null && FlowAsset.resultGraph.editorData != null)
            {
                var editorData = FlowAsset.resultGraph.editorData;
                if (resultGraphPanel != null && resultGraphPanel.GraphEditor != null)
                {
                    var config = resultGraphPanel.GraphEditor.ResultGraphPanelConfig;
                    if (config != null)
                    {
                        config.SaveState(editorData);
                    }
                }
            }
        }

        private void LoadResultGraphEditorState()
        {
            if (FlowAsset != null && FlowAsset.resultGraph != null && FlowAsset.resultGraph.editorData != null)
            {
                var editorData = FlowAsset.resultGraph.editorData;
                if (resultGraphPanel != null && resultGraphPanel.GraphEditor != null)
                {
                    var config = resultGraphPanel.GraphEditor.ResultGraphPanelConfig;
                    if (config != null)
                    {
                        config.LoadState(editorData);
                    }
                }
            }
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

        public static void ShowEditor()
        {
            DungeonFlowEditorWindow window = EditorWindow.GetWindow<DungeonFlowEditorWindow>(); 
            window.Init(null);
        }
    }


    public enum DungeonFlowEditorHighlightID
    {
        RulePanel,
        NodePanel,
        ProductionAddRHSButton
    }

    class DungeonFlowEditorHighlighter
    {
        private static void TraverseTree(IWidget widget, System.Action<IWidget> visit)
        {
            if (widget == null) return;

            visit(widget);

            var children = widget.GetChildWidgets();
            if (children != null)
            {
                foreach (var child in children)
                {
                    TraverseTree(child, visit);
                }
            }
        }

        public static void HighlightObjects(UISystem uiSystem, IWidget root, object objectOfInterest)
        {
            if (objectOfInterest == null)
            {
                return;
            }

            TraverseTree(root, widget => {
                if (widget is HighlightWidget)
                {
                    var highlightWidget = widget as HighlightWidget;
                    var highlightObject = highlightWidget.ObjectOfInterest;
                    if (objectOfInterest.Equals(highlightObject))
                    {
                        highlightWidget.Activate(uiSystem);
                    }
                }
            });
        }

        public static void HighlightObjects(UISystem uiSystem, IWidget root, object[] objectsOfInterest)
        {
            if (objectsOfInterest == null)
            {
                return;
            }

            TraverseTree(root, widget => {
                if (widget is HighlightWidget)
                {
                    var highlightWidget = widget as HighlightWidget;
                    var highlightObject = highlightWidget.ObjectOfInterest;
                    if (objectsOfInterest.Contains(highlightObject))
                    {
                        highlightWidget.Activate(uiSystem);
                    }
                }
            });
        }
    }
}
