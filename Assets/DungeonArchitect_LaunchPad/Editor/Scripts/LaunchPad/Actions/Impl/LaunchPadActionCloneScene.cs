using DungeonArchitect.Builders.GridFlow;
using DungeonArchitect.Builders.Snap;
using DungeonArchitect.Grammar;
using DungeonArchitect.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonArchitect.Editors.LaunchPad.Actions.Impl
{
    public class LaunchPadActionCloneScene : LaunchPadActionBase
    {
        string templatePath;
        bool resourcePath;
        public LaunchPadActionCloneScene(string templatePath, bool resourcePath)
        {
            this.templatePath = templatePath;
            this.resourcePath = resourcePath;
        }

        public override Texture2D GetIcon()
        {
            return ScreenPageLoader.LoadImageAsset("icons/create_scene");
        }

        public override string GetText()
        {
            return "Clone Scene";
        }

        protected virtual bool ShouldRebuildDungeon() { return false; }

        public override void Execute()
        {
            var deferredCommands = new List<ILaunchPadAction>();

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("Bailing out of clone");
                return;
            }

            string targetPath;
            if (CloneAsset(templatePath, resourcePath, out targetPath, "Create a Scene from Template"))
            {
                PingAsset(targetPath);
                EditorSceneManager.OpenScene(targetPath);
                {
                    var dungeons = GameObject.FindObjectsOfType<Dungeon>();

                    var destFileInfo = new System.IO.FileInfo(targetPath);
                    var destFilename = destFileInfo.Name;
                    var destFolder = targetPath.Substring(0, targetPath.Length - destFilename.Length);

                    foreach (var dungeon in dungeons)
                    {
                        var clonedThemes = new List<Graph>();
                        foreach (var theme in dungeon.dungeonThemes)
                        {
                            // Clone the themes
                            string destThemePath;
                            if (CloneTemplateReferencedAsset(theme, destFolder, out destThemePath))
                            {
                                var clonedTheme = AssetDatabase.LoadAssetAtPath<Graph>(destThemePath);
                                clonedThemes.Add(clonedTheme);
                            }
                            else
                            {
                                Debug.Log("Failed to copy referenced theme files while cloning the scene");
                            }
                        }
                        dungeon.dungeonThemes.Clear();
                        dungeon.dungeonThemes.AddRange(clonedThemes);
                        EditorUtility.SetDirty(dungeon);

                        if (clonedThemes.Count > 0 && clonedThemes[0] != null)
                        {
                            var themePath = AssetDatabase.GetAssetPath(clonedThemes[0]);
                            deferredCommands.Add(new LaunchPadActionOpenTheme(themePath));
                        }

                        // Snap flow asset
                        {
                            var snapConfig = dungeon.gameObject.GetComponent<SnapConfig>();
                            if (snapConfig != null && snapConfig.dungeonFlow != null)
                            {
                                string clonedFlowAssetPath;
                                if (CloneTemplateReferencedAsset(snapConfig.dungeonFlow, destFolder, out clonedFlowAssetPath))
                                {
                                    var clonedFlowAsset = AssetDatabase.LoadAssetAtPath<DungeonFlowAsset>(clonedFlowAssetPath);
                                    snapConfig.dungeonFlow = clonedFlowAsset;

                                    if (clonedFlowAsset != null)
                                    {
                                        var graphPath = AssetDatabase.GetAssetPath(clonedFlowAsset);
                                        deferredCommands.Add(new LaunchPadActionOpenSnapFlow(graphPath));
                                    }
                                }
                                else
                                {
                                    Debug.Log("Failed to copy snap flow asset while cloning the scene");
                                }
                            }
                        }

                        // Grid flow asset
                        {
                            var config = dungeon.gameObject.GetComponent<GridFlowDungeonConfig>();
                            if (config != null && config.flowAsset != null)
                            {
                                string clonedFlowAssetPath;
                                if (CloneTemplateReferencedAsset(config.flowAsset, destFolder, out clonedFlowAssetPath))
                                {
                                    var clonedFlowAsset = AssetDatabase.LoadAssetAtPath<DungeonGridFlowAsset>(clonedFlowAssetPath);
                                    config.flowAsset = clonedFlowAsset;

                                    if (clonedFlowAsset != null)
                                    {
                                        var graphPath = AssetDatabase.GetAssetPath(clonedFlowAsset);
                                        deferredCommands.Add(new LaunchPadActionOpenGridFlow(graphPath));
                                    }
                                }
                                else
                                {
                                    Debug.Log("Failed to copy grid flow asset while cloning the scene");
                                }
                            }
                        }

                        if (ShouldRebuildDungeon())
                        {
                            dungeon.Build(new EditorDungeonSceneObjectInstantiator());
                        }
                    }
                    if (dungeons.Length > 0)
                    {
                        Selection.activeGameObject = dungeons[0].gameObject;
                    }
                    var currentScene = SceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(currentScene);
                    EditorSceneManager.SaveScene(currentScene);

                    // Ping the cloned scene asset
                    {
                        Object sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
                        EditorGUIUtility.PingObject(sceneAsset);
                    }

                    foreach (var command in deferredCommands)
                    {
                        command.Execute();
                    }
                }
            }
        }
    }

    public class LaunchPadActionCloneSceneAndBuild : LaunchPadActionCloneScene
    {
        public LaunchPadActionCloneSceneAndBuild(string templatePath, bool resourcePath) : base(templatePath, resourcePath) { }
        protected override bool ShouldRebuildDungeon() { return true; }
    }
}
