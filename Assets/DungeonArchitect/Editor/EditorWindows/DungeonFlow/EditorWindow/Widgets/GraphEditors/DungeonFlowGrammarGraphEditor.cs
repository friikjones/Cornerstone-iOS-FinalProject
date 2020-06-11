using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonArchitect.Grammar;
using DungeonArchitect.Graphs;
using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets;
using DungeonArchitect.Graphs.Layouts;
using DungeonArchitect.Graphs.Layouts.Spring;
using UnityEditor;
using DungeonArchitect.UI.Widgets.GraphEditors;

namespace DungeonArchitect.Editors.DungeonFlow
{
    public enum DungeonFlowGrammarGraphEditorAction
    {
        CreateTaskNode,
        CreateWildcard,
        CreateCommentNode
    }

    public class DungeonFlowGrammarGraphEditorContextMenuData
    {
        public DungeonFlowGrammarGraphEditorContextMenuData(UISystem uiSystem, DungeonFlowGrammarGraphEditorAction action)
            : this(uiSystem, action, null)
        {
        }

        public DungeonFlowGrammarGraphEditorContextMenuData(UISystem uiSystem, DungeonFlowGrammarGraphEditorAction action, object userData)
        {
            this.uiSystem = uiSystem;
            this.Action = action;
            this.UserData = userData;
        }

        public UISystem uiSystem;
        public DungeonFlowGrammarGraphEditorAction Action;
        public object UserData;
    }

    public class DungeonFlowGrammarGraphSchema : GraphSchema
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

    public class DungeonFlowGrammarGraphEditor : GraphEditor
    {
        public DungeonFlowAsset FlowAsset { get; private set; }

        public void SetBranding(string branding)
        {
            if (EditorStyle != null)
            {
                EditorStyle.branding = branding;
            }
        }

        public override void Init(Graph graph, Rect editorBounds, UnityEngine.Object assetObject, UISystem uiSystem)
        {
            FlowAsset = assetObject as DungeonFlowAsset;

            base.Init(graph, editorBounds, assetObject, uiSystem);
        }

        T GetDragData<T>() where T : Object
        {
            var dragDropData = DragAndDrop.GetGenericData(NodeListViewConstants.DragDropID);
            if (dragDropData != null && dragDropData is T)
            {
                return dragDropData as T;
            }
            return null;
        }

        public override GraphSchema GetGraphSchema()
        {
            return new DungeonFlowGrammarGraphSchema();
        }

        protected override IGraphLinkRenderer CreateGraphLinkRenderer()
        {
            return new StraightLineGraphLinkRenderer();
        }

        public override void OnNodeSelectionChanged(UISystem uiSystem)
        {
            base.OnNodeSelectionChanged(uiSystem);

            // Fetch all selected nodes
            var selectedNodes = from node in graph.Nodes
                                where node.Selected
                                select node;

            if (selectedNodes.Count() == 0)
            {
                uiSystem.Platform.ShowObjectProperty(graph);
            }
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
            // We are dragging. Check if we support this data type
            return GetDragData<GrammarNodeType>() != null;
        }
        
        public override void Draw(UISystem uiSystem, UIRenderer renderer)
        {
            base.Draw(uiSystem, renderer);
            var bounds = new Rect(Vector2.zero, WidgetBounds.size);

            bool isDragging = (uiSystem != null && uiSystem.IsDragDrop);
            if (isDragging && IsDragDataSupported())
            {
                // Show the drag drop overlay
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
            var nodeType = GetDragData<GrammarNodeType>();
            if (nodeType != null)
            {
                if (uiSystem != null)
                {
                    uiSystem.RequestFocus(this);
                }

                CreateNewTaskNode(nodeType, e.mousePosition, true, uiSystem);
            }

            DragAndDrop.AcceptDrag();
        }

        int FindNextAvailableIndex()
        {
            if (graph.Nodes.Count == 0)
            {
                return 0;
            }

            // Find an appropriate execution index for this node
            var usedIndices = new HashSet<int>();
            foreach (var graphNode in graph.Nodes)
            {
                if (graphNode is GrammarTaskNode)
                {
                    var taskNode = graphNode as GrammarTaskNode;
                    usedIndices.Add(taskNode.executionIndex);
                }
            }

            for (int i = 0; i < usedIndices.Count + 1; i++)
            {
                if (!usedIndices.Contains(i))
                {
                    return i;
                }
            }
            return 0;
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

        GrammarTaskNode CreateNewTaskNode(GrammarNodeType nodeType, Vector2 mousePosition, bool selectAfterCreate, UISystem uiSystem)
        {
            int index = FindNextAvailableIndex();
            var node = CreateNode<GrammarTaskNode>(mousePosition, uiSystem);
            node.NodeType = nodeType;
            node.executionIndex = index;
            node.DisplayExecutionIndex = true;

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

            if (selectAfterCreate)
            {
                BringToFront(node);
                SelectNode(node, uiSystem);
            }

            return node;
        }

        protected override GraphContextMenu CreateContextMenu()
        {
            return new DungeonFlowGrammarGraphContextMenu();
        }

        protected override void InitializeNodeRenderers(GraphNodeRendererFactory nodeRenderers)
        {
            nodeRenderers.RegisterNodeRenderer(typeof(GrammarTaskNode), new GrammarTaskNodeRenderer());
            nodeRenderers.RegisterNodeRenderer(typeof(CommentNode), new CommentNodeRenderer(EditorStyle.commentTextColor));
        }

        protected override void OnMenuItemClicked(object userdata, GraphContextMenuEvent e)
        {
            var data = userdata as DungeonFlowGrammarGraphEditorContextMenuData;
            if (data == null) return;

            var mouseScreen = lastMousePosition;
            if (data.Action == DungeonFlowGrammarGraphEditorAction.CreateTaskNode)
            {
                if (data.UserData != null && data.UserData is GrammarNodeType)
                {
                    var nodeType = data.UserData as GrammarNodeType;
                    var node = CreateNewTaskNode(nodeType, lastMousePosition, true, e.uiSystem);
                    CreateLinkBetweenPins(e.sourcePin, node.InputPin, e.uiSystem);
                }
            }
            else if (data.Action == DungeonFlowGrammarGraphEditorAction.CreateWildcard)
            {
                var nodeType = FlowAsset.wildcardNodeType;
                if (nodeType != null)
                {
                    var node = CreateNewTaskNode(nodeType, lastMousePosition, true, e.uiSystem);
                    CreateLinkBetweenPins(e.sourcePin, node.InputPin, e.uiSystem);
                }
            }
            else if (data.Action == DungeonFlowGrammarGraphEditorAction.CreateCommentNode)
            {
                CreateCommentNode(mouseScreen, e.uiSystem);
            }
        }

        protected override void DrawHUD(UISystem uiSystem, UIRenderer renderer, Rect bounds) { }

        protected override string GetGraphNotInitializedMessage()
        {
            return "Graph not initialize";
        }

        protected override GraphEditorStyle CreateEditorStyle()
        {
            var editorStyle = base.CreateEditorStyle();
            editorStyle.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            return editorStyle;
        }

    }

    [CustomEditor(typeof(GrammarGraph), true)]
    public class GrammarGraphInspector : Editor
    {
        SerializedObject sobject;
        SerializedProperty useProceduralScript;

        protected virtual void OnEnable()
        {
            sobject = new SerializedObject(target);
            useProceduralScript = sobject.FindProperty("useProceduralScript");
        }

        ScriptableObject generatorInstanceCache = null;
        MonoScript generatorMonoScriptCache = null;

        void UpdateGeneratorInstance(string scriptClassName)
        {
            if (scriptClassName == null || scriptClassName.Length == 0)
            {
                generatorInstanceCache = null;
                generatorMonoScriptCache = null;
                return;
            }

            var type = System.Type.GetType(scriptClassName);
            if (type == null)
            {
                generatorInstanceCache = null;
                generatorMonoScriptCache = null;
                return;
            }

            if (generatorInstanceCache == null || generatorInstanceCache.GetType() != type)
            {
                generatorInstanceCache = CreateInstance(type);
                generatorMonoScriptCache = MonoScript.FromScriptableObject(generatorInstanceCache);
            }
        }

        public override void OnInspectorGUI()
        {
            sobject.Update();

            var grammar = target as GrammarGraph;
            GUILayout.Label("Grammar Properties", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use a script to generate your graph", MessageType.Info);
            EditorGUILayout.PropertyField(useProceduralScript);
            if (grammar.useProceduralScript)
            {
                UpdateGeneratorInstance(grammar.generatorScriptClass);
                var newScript = EditorGUILayout.ObjectField("Script", generatorMonoScriptCache, typeof(MonoScript), false) as MonoScript;
                if (newScript != generatorMonoScriptCache)
                {
                    if (newScript == null)
                    {
                        grammar.generatorScriptClass = null;
                    }
                    else
                    {
                        if (!newScript.GetClass().GetInterfaces().Contains(typeof(IGrammarGraphBuildScript)))
                        {
                            // The script doesn't implement the interface
                            grammar.generatorScriptClass = null;
                        }
                        else
                        {
                            grammar.generatorScriptClass = newScript.GetClass().AssemblyQualifiedName;
                        }
                    }
                }

                EditorGUILayout.HelpBox("Warning: Clicking build will replace your existing graph!", MessageType.Warning);
                var generatorScript = generatorInstanceCache as IGrammarGraphBuildScript;
                if (generatorScript == null)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Build"))
                {
                    // Find the active editor window and grab the flow asset that is being modified 
                    var editorWindow = EditorWindow.GetWindow<DungeonFlowEditorWindow>();
                    if (editorWindow != null && editorWindow.FlowAsset != null)
                    {
                        var flowAsset = editorWindow.FlowAsset;
                        var productionRule = GetProductionRule(flowAsset, grammar);
                        if (productionRule != null && generatorScript != null)
                        {
                            ExecuteGeneratorScript(generatorScript, grammar, flowAsset, editorWindow.uiSystem.Platform);
                            PerformLayout(grammar);
                            FocusEditorOnGrammar(editorWindow, grammar, productionRule);
                        }
                    }
                }
                GUI.enabled = true;
            }

            sobject.ApplyModifiedProperties();
        }

        void PerformLayout(GrammarGraph graph)
        {
            var config = new GraphLayoutSpringConfig();
            var layout = new GraphLayoutSpring<GraphNode>(config);
            var nodes = graph.Nodes.ToArray();
            nodes = nodes.Where(n => !(n is CommentNode)).ToArray();
            layout.Layout(nodes, new DefaultGraphLayoutNodeActions(graph));
        }

        void ExecuteGeneratorScript(IGrammarGraphBuildScript generatorScript, GrammarGraph grammar, DungeonFlowAsset flowAsset, UIPlatform platform)
        {
            var graphBuilder = new EditorGraphBuilder(grammar, flowAsset, platform);
            GrammarGraphBuilder grammarBuilder = new GrammarGraphBuilder(grammar, flowAsset.nodeTypes, graphBuilder);
            grammarBuilder.ClearGraph();
            generatorScript.Generate(grammarBuilder);
        }

        // Focus on the graph editor panel
        void FocusEditorOnGrammar(DungeonFlowEditorWindow editorWindow, GrammarGraph grammar, GrammarProductionRule productionRule)
        {
            var rulePanel = editorWindow.GetRuleListPanel();
            rulePanel.ListView.SetSelectedItem(editorWindow.uiSystem, productionRule);
            editorWindow.ForceUpdateWidgetFromCache(rulePanel);

            var productionRuleEditor = editorWindow.GetProductionRuleWidget();
            DungeonFlowEditorHighlighter.HighlightObjects(editorWindow.uiSystem, productionRuleEditor, grammar);

            // Center the camera
            foreach (var graphEditor in productionRuleEditor.GetGraphEditors())
            {
                if (graphEditor.Graph == grammar)
                {
                    editorWindow.AddDeferredCommand(new EditorCommand_InitializeGraphCameras(graphEditor));
                }
            }
        }

        GrammarProductionRule GetProductionRule(DungeonFlowAsset flowAsset, GrammarGraph grammar)
        {
            // make sure this object belongs to the asset
            if (flowAsset != null)
            {
                foreach (var rule in flowAsset.productionRules)
                {
                    if (grammar == rule.LHSGraph)
                    {
                        return rule;
                    }

                    foreach (var rhs in rule.RHSGraphs)
                    {
                        if (rhs.graph == grammar)
                        {
                            return rule;
                        }
                    }
                }
            }
            return null;
        }

    }

}
