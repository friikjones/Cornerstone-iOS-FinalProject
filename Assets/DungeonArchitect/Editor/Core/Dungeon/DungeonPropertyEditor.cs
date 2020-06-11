//$ Copyright 2016, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DungeonArchitect.Splatmap;

namespace DungeonArchitect.Editors
{
    public class DungeonPropertyEditorHook
    {
        public delegate void OnDungeonBuilt(Dungeon dungeon);
        public event OnDungeonBuilt DungeonBuilt;

        private DungeonPropertyEditorHook() { }
        private static DungeonPropertyEditorHook instance;
        public static DungeonPropertyEditorHook Get()
        {
            if (instance == null)
            {
                instance = new DungeonPropertyEditorHook();
            }
            return instance;
        }
        public static void NotifyDungeonBuilt(Dungeon dungeon)
        {
            if (Get().DungeonBuilt != null)
            {
                Get().DungeonBuilt.Invoke(dungeon);
            }
        }
    }
    /// <summary>
    /// Custom property editor for the dungeon game object
    /// </summary>
    [CustomEditor(typeof(Dungeon))]
	public class DungeonPropertyEditor : Editor {

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			
			if (GUILayout.Button ("Build Dungeon")) {
				BuildDungeon();
			}
			if (GUILayout.Button ("Destroy Dungeon")) {
				DestroyDungeon();
			}
		}

        protected virtual void OnEnable()
        {
            //EditorApplication.update += EditorUpdate;
        }
        
        protected virtual void OnDisable()
        {
            //EditorApplication.update -= EditorUpdate;
        }

        void EditorUpdate()
        {
            var dungeon = target as Dungeon;
            dungeon.Update();
        }
        
        void BuildDungeon() {
            // Make sure we have a theme defined
            bool valid = false;
			Dungeon dungeon = target as Dungeon;
			if (dungeon != null) {
				if (HasValidThemes(dungeon)) {
                    var config = dungeon.Config;
                    if (config != null)
                    {
                        string configErrorMessage = "";
                        if (config.HasValidConfig(ref configErrorMessage))
                        {
                            valid = true;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Dungeon Architect", configErrorMessage, "Ok");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Dungeon Architect", "Missing dungeon config component in dungeon game object", "Ok");
                    }
				} 
				else {
					//Highlighter.Highlight ("Inspector", "Dungeon Themes");

					// Notify the user that at least one theme needs to be set
					EditorUtility.DisplayDialog("Dungeon Architect", "Please assign at least one Dungeon Theme before building", "Ok");
				}
			}

            if (valid)
            {
                // Create the splat maps for this dungeon, if necessary
                var splatComponent = dungeon.GetComponent<DungeonSplatmap>();
                SplatmapPropertyEditor.CreateSplatMapAsset(splatComponent);

                // Build the dungeon
                //Undo.RecordObjects(new Object[] { dungeon, dungeon.ActiveModel }, "Dungeon Built");
                dungeon.Build(new EditorDungeonSceneObjectInstantiator());
                DungeonEditorHelper.MarkSceneDirty();
                DungeonPropertyEditorHook.NotifyDungeonBuilt(dungeon);

                // Mark the splatmaps as dirty
                if (splatComponent != null && splatComponent.splatmap != null)
                {
                    EditorUtility.SetDirty(splatComponent.splatmap);
                }
            }
		}

		IEnumerator StopHighlighter() {
			yield return new WaitForSeconds(2);
			Highlighter.Stop();
		}

		void DestroyDungeon() {
			Dungeon dungeon = target as Dungeon;
            if (dungeon != null)
            {
                //Undo.RecordObjects(new Object[] { dungeon, dungeon.ActiveModel }, "Dungeon Destroyed");
                dungeon.DestroyDungeon();
                EditorUtility.SetDirty(dungeon.gameObject);
            }
		}

        bool HasValidThemes(Dungeon dungeon) {
            var builder = dungeon.gameObject.GetComponent<DungeonBuilder>();
            if (builder != null && !builder.IsThemingSupported())
            {
                // Theming is not supported in this builder. empty theme configuration would do
                return true;
            }

            if (dungeon.dungeonThemes == null) return false;
			foreach (var theme in dungeon.dungeonThemes) {
				if (theme != null) {
					return true;
				}
			}
			return false;
		}

	}
}
