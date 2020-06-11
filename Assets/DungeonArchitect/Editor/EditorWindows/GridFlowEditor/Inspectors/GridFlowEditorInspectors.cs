using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonArchitect.Builders.GridFlow;

namespace DungeonArchitect.Editors.GridFlow
{
    [CustomEditor(typeof(GridFlowExecGraphEditorConfig), true)]
    public class GridFlowExecGraphEditorConfigInspector : Editor
    {
        SerializedObject sobject;
        SerializedProperty randomizeSeed;
        SerializedProperty seed;
        SerializedProperty dungeonObject;
        

        protected virtual void OnEnable()
        {
            sobject = new SerializedObject(target);
            randomizeSeed = sobject.FindProperty("randomizeSeed");
            seed = sobject.FindProperty("seed");
            dungeonObject = sobject.FindProperty("dungeonObject");
        }

        public override void OnInspectorGUI()
        {
            sobject.Update();
            var config = target as GridFlowExecGraphEditorConfig;

            GUILayout.Label("Linked Dungeon", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dungeonObject);

            GUILayout.Label("Preview Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomizeSeed);
            bool enableSeedEdit = true;
            if (config.dungeonObject == null)
            {
                if (config.randomizeSeed)
                {
                    enableSeedEdit = false;
                }
            }
            else
            {
                enableSeedEdit = false;
                var dungeonConfig = config.dungeonObject.gameObject.GetComponent<GridFlowDungeonConfig>();
                if (dungeonConfig != null)
                {
                    int seed = (int)dungeonConfig.Seed;
                    config.seed = seed;
                    EditorGUILayout.HelpBox("The seed cannot be modified, it is taken from the linked dungeon game object", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Invalid dungeon prefab configuration. Cannot find GridFlowDungeonConfig component", MessageType.Info);
                }
            }

            GUI.enabled = enableSeedEdit;
            EditorGUILayout.PropertyField(seed);
            GUI.enabled = true;

            sobject.ApplyModifiedProperties();
        }

    }

}
