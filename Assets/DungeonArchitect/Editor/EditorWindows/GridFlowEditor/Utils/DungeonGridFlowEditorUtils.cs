using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonArchitect.Graphs;
using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;
using DungeonArchitect.UI.Widgets.GraphEditors;
using DungeonArchitect.UI;

namespace DungeonArchitect.Editors.GridFlow
{
    public class DungeonGridFlowEditorUtils
    {
        #region Asset Management
        public static void InitAsset(DungeonGridFlowAsset asset, UIPlatform platform)
        {
            if (asset == null) return;

            asset.execGraph = CreateAssetObject<GridFlowExecGraph>(asset);

            InitializeExecutionGraph(asset, platform);
        }

        private static T CreateAssetObject<T>(DungeonGridFlowAsset asset) where T : ScriptableObject
        {
            var obj = ScriptableObject.CreateInstance<T>();
            obj.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        private static void InitializeExecutionGraph(DungeonGridFlowAsset asset, UIPlatform platform)
        {
            // Create an entry node in the execution graph
            var resultNode = CreateGraphNode<GridFlowExecResultGraphNode>(Vector2.zero, asset.execGraph, asset, platform);
            resultNode.Position = Vector2.zero;
            asset.execGraph.resultNode = resultNode;
            resultNode.nodeHandler = CreateAssetObject<GridFlowExecNodeHandler_Result>(asset);
        }

        static T CreateGraphNode<T>(Vector2 position, Graph graph, DungeonGridFlowAsset asset, UIPlatform platform) where T : GraphNode
        {
            var node = GraphOperations.CreateNode(graph, typeof(T), null);
            GraphEditorUtils.AddToAsset(platform, asset, node);
            return node as T;
        }
        #endregion
    }
}
