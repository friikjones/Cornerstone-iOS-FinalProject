using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DungeonArchitect.Graphs.Layouts;
using DungeonArchitect.Graphs.Layouts.Layered;
using DungeonArchitect.Graphs.Layouts.Spring;
using DungeonArchitect.Utils;

namespace DungeonArchitect.Editors.DungeonFlow
{
    public class DungeonFlowResultGraphEditorConfig : ScriptableObject
    {
        public GraphLayoutType layoutType;
        public GraphLayoutLayeredConfig configLayered;
        public GraphLayoutSpringConfig configSpring;

        private void OnEnable()
        {
            if (configLayered == null)
            {
                configLayered = new GraphLayoutLayeredConfig();
            }
            if (configSpring == null)
            {
                configSpring = new GraphLayoutSpringConfig();
            }
        }

        public void SaveState(KeyValueData editorData)
        {
            editorData.Set("layoutType", (int)layoutType);

            if (configLayered != null)
            {
                editorData.Set("layered.separation", configLayered.separation);
            }

            if (configSpring != null)
            {
                editorData.Set("spring.interNodeDistance", configSpring.interNodeDistance);
                editorData.Set("spring.interNodeTension", configSpring.interNodeTension);
                editorData.Set("spring.springDistance", configSpring.springDistance);
                editorData.Set("spring.springTension", configSpring.springTension);
                editorData.Set("spring.iterations", configSpring.iterations);
                editorData.Set("spring.timeStep", configSpring.timeStep);
            }
        }

        public void LoadState(KeyValueData editorData)
        {
            int layoutTypeValue = 0;
            if (editorData.GetInt("layoutType", ref layoutTypeValue))
            {
                layoutType = (GraphLayoutType)layoutTypeValue;
            }

            if (configLayered == null)
            {
                configLayered = new GraphLayoutLayeredConfig();
            }

            editorData.GetVector2("layered.separation", ref configLayered.separation);

            if (configSpring == null)
            {
                configSpring = new GraphLayoutSpringConfig();
            }

            editorData.GetFloat("spring.interNodeDistance", ref configSpring.interNodeDistance);
            editorData.GetFloat("spring.interNodeTension", ref configSpring.interNodeTension);
            editorData.GetFloat("spring.springDistance", ref configSpring.springDistance);
            editorData.GetFloat("spring.springTension", ref configSpring.springTension);
            editorData.GetInt("spring.iterations", ref configSpring.iterations);
            editorData.GetFloat("spring.timeStep", ref configSpring.timeStep);
        }
    }


    [CustomEditor(typeof(DungeonFlowResultGraphEditorConfig), true)]
    public class DungeonFlowResultGraphEditorConfigInspector : Editor
    {
        SerializedObject sobject;
        SerializedProperty layoutType;

        SerializedProperty configLayered_Separation;

        SerializedProperty configSpring_springDistance;
        SerializedProperty configSpring_nodeDistance;

        protected virtual void OnEnable()
        {
            sobject = new SerializedObject(target);
            layoutType = sobject.FindProperty("layoutType");

            var configLayered = sobject.FindProperty("configLayered");
            configLayered_Separation = configLayered.FindPropertyRelative("separation");

            var configSpring = sobject.FindProperty("configSpring");
            configSpring_springDistance = configSpring.FindPropertyRelative("springDistance");
            configSpring_nodeDistance = configSpring.FindPropertyRelative("interNodeDistance");
        }

        public override void OnInspectorGUI()
        {
            sobject.Update();


            GUILayout.Label("Layout Config", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(layoutType);
            GUILayout.Space(10);

            var targetConfig = target as DungeonFlowResultGraphEditorConfig;
            if (targetConfig.layoutType == GraphLayoutType.Layered)
            {
                EditorGUILayout.PropertyField(configLayered_Separation);
            }
            else if (targetConfig.layoutType == GraphLayoutType.Spring)
            {
                EditorGUILayout.PropertyField(configSpring_springDistance);
                EditorGUILayout.PropertyField(configSpring_nodeDistance);
            }

            sobject.ApplyModifiedProperties();
        }
    }
}
