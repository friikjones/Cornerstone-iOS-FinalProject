using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using DungeonArchitect.Builders.GridFlow.Graphs.Exec.NodeHandlers;
using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.Builders.GridFlow.Graphs.Preview.Tilemap;

namespace DungeonArchitect.Editors.GridFlow
{
    #region Base classes
    public abstract class GridFlowInspectorBase : Editor
    {
        protected SerializedObject sobject;
        Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();

        public GridFlowInspectorMonoScriptProperty<T> CreateScriptProperty<T>(string className)
        {
            return new GridFlowInspectorMonoScriptProperty<T>(className);
        }

        public void DrawProperty(string name)
        {
            DrawProperty(name, false);
        }

        public void DrawProperties(params string[] names)
        {
            foreach (var name in names)
            {
                DrawProperty(name, false);
            }
        }

        SerializedProperty GetProperty(string name)
        {
            if (properties.ContainsKey(name))
            {
                return properties[name];
            }

            if (!name.Contains("."))
            {
                var property = sobject.FindProperty(name);
                properties.Add(name, property);
                return property;
            }
            else
            {
                var tokens = name.Split(".".ToCharArray());
                var property = GetProperty(tokens[0]);
                for (int i = 1; i < tokens.Length; i++)
                {
                    property = property.FindPropertyRelative(tokens[i]);
                }
                properties[name] = property;
                return property;
            }
        }

        public void DrawProperty(string name, bool includeChildren)
        {
            var property = GetProperty(name);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, includeChildren);
            }
            else
            {
                Debug.LogError("Invalid property name: " + name);
            }
        }

        protected virtual void OnEnable()
        {
            sobject = new SerializedObject(target);
        }

        public abstract void HandleInspectorGUI();

        public override void OnInspectorGUI()
        {
            sobject.Update();

            HandleInspectorGUI();

            sobject.ApplyModifiedProperties();
        }
    }

    public class GridFlowExecNodeHandlerInspectorBase : GridFlowInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            var attribute = GridFlowExecNodeInfoAttribute.GetHandlerAttribute(target.GetType());
            var title = "Node Settings";
            if (attribute != null)
            {
                title = attribute.Title;
            }
            GUILayout.Label(title, EditorStyles.boldLabel);
            DrawProperty("description");
        }
    }

    [CustomEditor(typeof(GridFlowPreviewTilemapGraphNode), false)]
    public class GridFlowPreviewTilemapGraphNodeInspector : Editor
    {
        SerializedObject sobject;
        SerializedProperty tileRenderSize;

        protected virtual void OnEnable()
        {
            sobject = new SerializedObject(target);
            tileRenderSize = sobject.FindProperty("tileRenderSize");
        }

        public override void OnInspectorGUI()
        {
            sobject.Update();

            GUILayout.Label("Tilemap Preview Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(tileRenderSize);
            if (EditorGUI.EndChangeCheck())
            {
                var node = target as GridFlowPreviewTilemapGraphNode;
                node.RequestRecreatePreview = true;
            }

            sobject.ApplyModifiedProperties();
        }

    }

    #endregion

    #region Abstract Node Inspectors

    [CustomEditor(typeof(GridFlowExecNodeHandler_CreateGrid), false)]
    public class GridFlowExecNodeHandler_CreateGridInspector : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Grid Info", EditorStyles.boldLabel);
            DrawProperties("resolution");
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_CreateKeyLock), false)]
    public class GridFlowExecNodeHandler_CreateKeyLockInspector : GridFlowExecNodeHandlerInspectorBase
    {
        GridFlowExecNodePlacementSettingInspector placementInspector;
        protected override void OnEnable()
        {
            base.OnEnable();

            var handler = target as GridFlowExecNodeHandler_CreateKeyLock;
            placementInspector = new GridFlowExecNodePlacementSettingInspector(this, "placementSettings", "Key Placement", handler.placementSettings);
        }

        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Branch Info", EditorStyles.boldLabel);
            DrawProperties("keyBranch", "lockBranch");

            GUILayout.Label("Marker Names", EditorStyles.boldLabel);
            DrawProperties("keyMarkerName", "lockMarkerName");

            placementInspector.Draw(this);
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_CreateMainPath), false)]
    public class GridFlowExecNodeHandler_CreateMainPathInspector : GridFlowExecNodeHandlerInspectorBase
    {
        GridFlowExecNodePlacementSettingInspector startPlacementInspector;
        GridFlowExecNodePlacementSettingInspector goalPlacementInspector;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            var handler = target as GridFlowExecNodeHandler_CreateMainPath;
            startPlacementInspector = new GridFlowExecNodePlacementSettingInspector(this, "startPlacementSettings", "Start Item Placement", handler.startPlacementSettings);
            goalPlacementInspector = new GridFlowExecNodePlacementSettingInspector(this, "goalPlacementSettings", "Goal Item Placement", handler.goalPlacementSettings);
        }

        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Path Info", EditorStyles.boldLabel);
            DrawProperties("pathSize", "pathName", "nodeColor");

            GUILayout.Label("Marker Names", EditorStyles.boldLabel);
            DrawProperties("startMarkerName", "goalMarkerName");

            GUILayout.Label("Start / Goal Nodes", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "You can give a different path name to the start / goal nodes. This way when other branches connect to this main path, they don't connect to the start / goal nodes. Leave it blank to make it part of the main branch", 
                MessageType.Info);
            DrawProperties("startNodePathName", "goalNodePathName");

            startPlacementInspector.Draw(this);
            goalPlacementInspector.Draw(this);

            GUILayout.Label("Advanced", EditorStyles.boldLabel);
            DrawProperties("drawDebug");
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_CreatePath), false)]
    public class GridFlowExecNodeHandler_CreatePathInspector : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Path Info", EditorStyles.boldLabel);
            DrawProperties("minPathSize", "maxPathSize", "pathName", "nodeColor");

            GUILayout.Label("Branching Info", EditorStyles.boldLabel);
            DrawProperties("startFromPath", "endOnPath");

            GUILayout.Label("Advanced", EditorStyles.boldLabel);
            DrawProperties("drawDebug");
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_SpawnItems), false)]
    public class GridFlowExecNodeHandler_SpawnItemsInspector : GridFlowExecNodeHandlerInspectorBase
    {
        GridFlowExecNodePlacementSettingInspector placementInspector;
        protected override void OnEnable()
        {
            base.OnEnable();

            var handler = target as GridFlowExecNodeHandler_SpawnItems;
            placementInspector = new GridFlowExecNodePlacementSettingInspector(this, "placementSettings", "Placement Method", handler.placementSettings);
        }

        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            var handler = target as GridFlowExecNodeHandler_SpawnItems;

            GUILayout.Label("Spawn Info", EditorStyles.boldLabel);
            DrawProperty("paths", true);
            DrawProperties("itemType", "markerName");

            if (handler.itemType == GridFlowGraphItemType.Custom)
            {
                DrawProperty("customItemInfo", true);
            }
            DrawProperties("minCount", "maxCount");

            GUILayout.Label("Spawn Method", EditorStyles.boldLabel);
            DrawProperty("spawnMethod");
            if (handler.spawnMethod == GridFlowExecNodeHandler_SpawnItemMethod.CurveDifficulty)
            {
                DrawProperty("spawnDistributionCurve");
            }
            if (handler.spawnMethod != GridFlowExecNodeHandler_SpawnItemMethod.RandomRange)
            {
                DrawProperty("spawnDistributionVariance");
            }
            DrawProperties("minSpawnDifficulty", "spawnProbability");

            placementInspector.Draw(this);

            GUILayout.Label("Debug Info", EditorStyles.boldLabel);
            DrawProperty("showDifficulty");
            if (handler.showDifficulty)
            {
                DrawProperty("difficultyInfoColor");
            }

        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_FinalizeGraph), false)]
    public class GridFlowExecNodeHandler_FinalizeGraphInspector : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("One-way Doors", EditorStyles.boldLabel);
            DrawProperties("oneWayDoorPromotionWeight");

            GUILayout.Label("Advanced", EditorStyles.boldLabel);
            DrawProperties("debugDraw");
        }
    }

    #endregion

    #region Tilemap Node Inspectors

    [CustomEditor(typeof(GridFlowExecNodeHandler_InitializeTilemap), false)]
    public class GridFlowExecNodeHandlerInspector_InitializeTilemap : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Layout Settings", EditorStyles.boldLabel);
            DrawProperties("tilemapSizePerNode", "perturbAmount", "corridorLaneWidth", "layoutPadding");

            GUILayout.Label("Cave Settings", EditorStyles.boldLabel);
            DrawProperties("caveThickness", "caveAutomataNeighbors", "caveAutomataIterations");

            GUILayout.Label("Color Settings", EditorStyles.boldLabel);
            DrawProperties("roomColorSaturation", "roomColorBrightness");
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_CreateTilemapElevations), false)]
    public class GridFlowExecNodeHandler_CreateTilemapElevationsInspector : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Marker", EditorStyles.boldLabel);
            DrawProperties("markerName");

            GUILayout.Label("Noise Settings", EditorStyles.boldLabel);
            DrawProperties("noiseOctaves", "noiseFrequency", "noiseValuePower", "numSteps");

            GUILayout.Label("Height data", EditorStyles.boldLabel);
            DrawProperties("minHeight", "maxHeight", "seaLevel");

            GUILayout.Label("Colors", EditorStyles.boldLabel);
            DrawProperties("landColor", "seaColor", "minColorMultiplier");
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_CreateTilemapOverlay), false)]
    public class GridFlowExecNodeHandlerInspector_CreateTilemapOverlay : GridFlowExecNodeHandlerInspectorBase
    {
        GridFlowInspectorMonoScriptProperty<IGridFlowTilemapOverlayGenerator> generatorProperty;
        protected override void OnEnable()
        {
            base.OnEnable();

            var handler = target as GridFlowExecNodeHandler_CreateTilemapOverlay;
            generatorProperty = CreateScriptProperty<IGridFlowTilemapOverlayGenerator>(handler.generatorScriptClass);
        }

        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            var handler = target as GridFlowExecNodeHandler_CreateTilemapOverlay;

            DrawProperties("markerName", "color");

            GUILayout.Label("Generation Settings", EditorStyles.boldLabel);
            DrawProperty("generationMethod");
            if (handler.generationMethod == GridFlowExecNodeHandler_CreateTilemapOverlayGenMethod.Noise)
            {
                // Show noise settings
                DrawProperty("noiseSettings", true);
            }
            else if (handler.generationMethod == GridFlowExecNodeHandler_CreateTilemapOverlayGenMethod.Script)
            {
                // Show script settings
                generatorProperty.Draw(className => handler.generatorScriptClass = className);
            }

            DrawProperty("overlayBlocksTile");

            GUILayout.Label("Merge Settings", EditorStyles.boldLabel);
            DrawProperty("mergeConfig", true);
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_MergeTilemaps), false)]
    public class GridFlowExecNodeHandler_MergeTilemapsInspector : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_OptimizeTilemap), false)]
    public class GridFlowExecNodeHandlerInspector_OptimizeTilemap : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Discard distant background tiles", EditorStyles.boldLabel);
            DrawProperties("discardDistanceFromLayout");

        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_FinalizeTilemap), false)]
    public class GridFlowExecNodeHandler_FinalizeTilemapInspector : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();

            GUILayout.Label("Advanced", EditorStyles.boldLabel);
            DrawProperties("debugUnwalkableCells");
        }
    }

    [CustomEditor(typeof(GridFlowExecNodeHandler_Result), false)]
    public class GridFlowExecNodeHandler_ResultInspector : GridFlowExecNodeHandlerInspectorBase
    {
        public override void HandleInspectorGUI()
        {
            base.HandleInspectorGUI();
        }
    }

    #endregion

    #region Utility

    class GridFlowExecNodePlacementSettingInspector
    {
        GridFlowInspectorMonoScriptProperty<IGridFlowItemPlacementStrategy> scriptProperty;
        string settingsVariableName;
        string title;
        GridFlowItemPlacementSettings settings;
        public GridFlowExecNodePlacementSettingInspector(GridFlowExecNodeHandlerInspectorBase inspector, string settingsVariableName, string title, GridFlowItemPlacementSettings settings)
        {
            this.settingsVariableName = settingsVariableName;
            this.settings = settings;
            this.title = title;
            scriptProperty = inspector.CreateScriptProperty<IGridFlowItemPlacementStrategy>(settings.placementScriptClass);
        }

        public void Draw(GridFlowExecNodeHandlerInspectorBase inspector)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);

            inspector.DrawProperties(settingsVariableName + ".placementMethod");
            if (settings.placementMethod == GridFlowItemPlacementMethod.Script)
            {
                scriptProperty.Draw(className => settings.placementScriptClass = className);
            }
            if (settings.placementMethod != GridFlowItemPlacementMethod.Script)
            {
                inspector.DrawProperties(settingsVariableName + ".avoidPlacingNextToDoors");
            }
            if (settings.placementMethod != GridFlowItemPlacementMethod.RandomTile)
            {
                inspector.DrawProperties(settingsVariableName + ".fallbackToRandomPlacement");
            }
        }
    }

    public class GridFlowInspectorMonoScriptProperty<T>
    {
        public string ClassName { get; private set; }
        public MonoScript ScriptCache { get; set; }

        public GridFlowInspectorMonoScriptProperty(string propertyValue)
        {
            ClassName = propertyValue;
            UpdateScriptCache();
        }

        public void Draw(System.Action<string> ClassSetter)
        {
            var newScript = EditorGUILayout.ObjectField("Script", ScriptCache, typeof(MonoScript), false) as MonoScript;
            if (newScript != ScriptCache)
            {
                if (newScript == null)
                {
                    ClassName = null;
                }
                else
                {
                    if (!newScript.GetClass().GetInterfaces().Contains(typeof(T)))
                    {
                        // The script doesn't implement the interface
                        ClassName = null;
                    }
                    else
                    {
                        ClassName = newScript.GetClass().AssemblyQualifiedName;
                    }
                }
                UpdateScriptCache();
            }

            ClassSetter(ClassName);
        }

        public void Destroy()
        {
            if (ScriptCache != null)
            {
                ScriptableObject.DestroyImmediate(ScriptCache);
                ScriptCache = null;
            }
        }

        void UpdateScriptCache()
        {
            if (ClassName == null || ClassName.Length == 0)
            {
                ScriptCache = null;
                return;
            }

            var type = System.Type.GetType(ClassName);
            if (type == null)
            {
                ScriptCache = null;
                return;
            }

            var instance = ScriptableObject.CreateInstance(type);
            ScriptCache = MonoScript.FromScriptableObject(instance);
            ScriptableObject.DestroyImmediate(instance);
        }
    }

    #endregion

}
