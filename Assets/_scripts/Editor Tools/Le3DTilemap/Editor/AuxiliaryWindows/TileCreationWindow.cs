using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public class TileCreationWindow : EditorWindow {

        private enum SetupMode { None, InPlace, Variant }
        private SetupMode setupMode = SetupMode.Variant;

        private bool isDynamic;

        public event System.Action<TileData> OnTileCreation;

        private Editor preview;
        private GameObject prefab;
        private string tileName = "No Prefab Selected";
        private string prefabName = "No Prefab Selected";
        private Vector2 globalScroll;

        private Texture2D iconHelp, iconGood;

        public static TileCreationWindow ShowAuxiliary(GameObject prefab) {
            TileCreationWindow window = GetWindow<TileCreationWindow>("New Tile Data");
            window.prefab = prefab;
            if (prefab) window.tileName = prefab.name;
            window.ShowAuxWindow();
            return window;
        }

        void OnEnable() {
            AssemblyReloadEvents.beforeAssemblyReload += AssemblyCleanup;
            EditorUtils.LoadIcon(ref iconHelp, EditorUtils.ICON_HELP);
            EditorUtils.LoadIcon(ref iconGood, EditorUtils.ICON_CHECK_BLUE);
        }

        private void ProcessPrefab(GameObject prefab) {
            TileInfo info = prefab.GetComponentInChildren<TileInfo>();
            tileName = prefab.name;
            prefabName = prefab.name;
        }

        void OnGUI() {
            AssetUtils.InvalidNameCondition tileNameValid = AssetUtils.ValidateFilename(tileName);
            AssetUtils.InvalidNameCondition prefabNameValid = AssetUtils.ValidateFilename(prefabName);
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(globalScroll)) {
                globalScroll = scrollScope.scrollPosition;
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.Height(150))) {
                    EditorGUILayout.GetControlRect(GUILayout.Width(1));
                    using (new EditorGUILayout.VerticalScope()) {
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));
                        if (prefab is null) {
                            GUIUtils.DrawScopeCenteredText("Select a GameObject Prefab");
                        } else {
                            if (preview is null ) {
                                Editor.CreateCachedEditor(prefab, null, ref preview);
                            } preview.DrawPreview(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true),
                                                                                 GUILayout.ExpandHeight(true)));
                        } EditorGUILayout.GetControlRect(GUILayout.Height(1));
                    } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                }
                
                EditorGUILayout.GetControlRect(GUILayout.Height(2));
                GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                EditorGUILayout.GetControlRect(false, 4);

                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.GetControlRect(GUILayout.Width(2));
                    using (new EditorGUILayout.VerticalScope()) {
                        using (new EditorGUILayout.HorizontalScope()) {
                            EditorGUILayout.GetControlRect(GUILayout.Width(1));
                            GUILayout.Label("Prefab:");
                            using (var scope = new EditorGUI.ChangeCheckScope()) {
                                prefab = EditorGUILayout.ObjectField(prefab, typeof(GameObject), false) as GameObject;
                                if (scope.changed) {
                                    if (prefab != null) {
                                        ProcessPrefab(prefab);
                                    } else {
                                        tileName = "No Prefab Selected";
                                        prefabName = "No Prefab Selected";
                                    }
                                }
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        }

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        GUI.enabled = prefab != null;
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Tile Settings", UIStyles.CenteredLabelBold);
                        } EditorGUIUtility.labelWidth = 80;
                        using (new EditorGUILayout.HorizontalScope()) {
                            EditorGUILayout.GetControlRect(GUILayout.Width(1));
                            using (new EditorGUILayout.VerticalScope()) {
                                using (new EditorGUILayout.HorizontalScope()) {
                                    GUILayout.Label("Tile Name:");
                                    GUI.color = tileNameValid == AssetUtils.InvalidNameCondition.None ? Color.white
                                                                                                     : UIColors.DarkRed;
                                    tileName = EditorGUILayout.TextField(tileName, GUILayout.Width(165));
                                    DrawTooltipLabel(tileNameValid);
                                    GUI.color = Color.white;
                                }
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        }

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        GUI.enabled = prefab != null;
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Setup Mode", UIStyles.CenteredLabelBold);
                        } using (new EditorGUILayout.HorizontalScope()) {
                            GUIContent content = new GUIContent("None", "Do not perform any prefab setup");
                            GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                            Rect rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                       GUILayout.Width(90),
                                                                       GUILayout.ExpandWidth(true));
                            rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                            DrawSetupButton(rect, SetupMode.None, content, style);
                            content = new GUIContent("In-Place", "Modify the pre-existing target prefab");
                            style = new(GUI.skin.button) { margin = { right = 0, left = 0 } };
                            rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                  GUILayout.Width(90),
                                                                  GUILayout.ExpandWidth(true));
                            rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                            DrawSetupButton(rect, SetupMode.InPlace, content, style);
                            content = new GUIContent("Variant", "Create a new prefab containing the target");
                            style = new(GUI.skin.button) { margin = { left = 0 } };
                            rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                  GUILayout.Width(90),
                                                                  GUILayout.ExpandWidth(true));
                            rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                            DrawSetupButton(rect, SetupMode.Variant, content, style);
                            GUI.backgroundColor = Color.white;
                        }

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Prefab Settings", UIStyles.CenteredLabelBold);
                        } using (new EditorGUILayout.VerticalScope()) {
                            EditorGUILayout.GetControlRect(false, 2, GUILayout.Width(1));
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Prefab Name:");
                                GUI.color = prefabNameValid == AssetUtils.InvalidNameCondition.None ? Color.white
                                                                                                 : UIColors.DarkRed;
                                prefabName = EditorGUILayout.TextField(prefabName, GUILayout.Width(165));
                                DrawTooltipLabel(prefabNameValid);
                                GUI.color = Color.white;
                            } using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Engine Complexity:");
                                GUIContent content = new GUIContent("Static", "Static Tiles are:\n"
                                                                  + "- Merged into larger meshes for optimization\n"
                                                                  + "- Significantly more performant than dynamic tiles\n"
                                                                  + "Static Tiles cannot:\n"
                                                                  + "- Move, rotate, or scale <b><i>at runtime</i></b>\n"
                                                                  + "- Perform animations involving transform changes");
                                GUIStyle style = new(GUI.skin.button) { margin = { right = 0, left = 0 } };
                                Rect rect = EditorGUILayout.GetControlRect(false, 18, style,
                                                                           GUILayout.Width(92),
                                                                           GUILayout.ExpandWidth(true));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawComplexityButton(rect, false, content, style);
                                content = new GUIContent("Dynamic", "Dynamic Tiles are:\n"
                                                       + "- Excluded from mesh-merging optimization\n"
                                                       + "- Significantly less performant than static tiles\n"
                                                       + "Tiles should be Dynamic if:\n"
                                                       + "- They must be moved, rotated, or scaled <b><i>at runtime</i></b>\n"
                                                       + "- They contain animations that involve transform changes");
                                style = new(GUI.skin.button) { margin = { left = 0 } };
                                rect = EditorGUILayout.GetControlRect(false, 18, style,
                                                                      GUILayout.Width(92),
                                                                      GUILayout.ExpandWidth(true));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawComplexityButton(rect, true, content, style);
                                GUI.backgroundColor = Color.white;
                            } EditorGUILayout.GetControlRect(false, 2, GUILayout.Width(1));
                        }

                        GUI.enabled = true;
                        EditorGUIUtility.labelWidth = 0;

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Messages", UIStyles.CenteredLabelBold);
                        } using (new EditorGUILayout.HorizontalScope()) {
                            EditorGUILayout.GetControlRect(GUILayout.Width(1));
                            using (new EditorGUILayout.VerticalScope()) {
                                DisplayMessages();
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        }
                    } EditorGUILayout.GetControlRect(GUILayout.Width(2));
                }
            } using (new EditorGUILayout.HorizontalScope()) {
                GUI.color = UIColors.Green;
                if (GUILayout.Button("Create")) {
                    TileData newAsset = AssetUtils.CreateAsset<TileData>("New Tile Data", tileName.Trim());
                    if (newAsset) {
                        newAsset.Prefab = prefab;
                        EditorUtility.SetDirty(newAsset);
                        OnTileCreation?.Invoke(newAsset);
                        Close();
                    }
                } GUI.color = UIColors.Red;
                if (GUILayout.Button("Cancel")) Close();
            }
        }

        private void DrawSetupButton(Rect rect, SetupMode mode, 
                                     GUIContent content, GUIStyle style) {
            GUI.backgroundColor = GUI.enabled && setupMode == mode ? UIColors.DefinedBlue 
                                                                   : Color.white;
            if (GUI.Button(rect, content, style)) setupMode = mode;
        }

        private void DrawComplexityButton(Rect rect, bool isDynamic,
                                          GUIContent content, GUIStyle style) {
            GUI.backgroundColor = this.isDynamic == isDynamic 
                                                 && GUI.enabled ? UIColors.DefinedBlue
                                                                : Color.white;
            if (GUI.Button(rect, content, style)) this.isDynamic = isDynamic;
        }

        private void DisplayMessages() {

            /*GUIUtils.DrawCustomHelpBox(" Missing Editor Node. Won't be added to Palette;",
                                                       EditorUtils.FetchIcon("d_P4_Offline"));*/
            GUIUtils.DrawCustomHelpBox(" Complexity: No collider edits required;", iconGood);
            /*GUIUtils.DrawCustomHelpBox(" Complexity: Additional collider edits required;",
                                       EditorUtils.FetchIcon("d_P4_OutOfSync"));*/
        }

        private void DrawTooltipLabel(AssetUtils.InvalidNameCondition nameValidity) {
            string tooltip = GUI.enabled ? nameValidity switch {
                AssetUtils.InvalidNameCondition.None => "Name is valid",
                AssetUtils.InvalidNameCondition.Empty => "Name cannot be empty or whitespace",
                _ => "Naming convention violated:"
                   + "\n- First letter must be capitalized or an underscore"
                   + "\n- Cannot contain invalid characters" } : "";
            GUIStyle style = new(EditorStyles.label) { contentOffset = new(0, -2) };
            GUILayout.Label(new GUIContent(iconHelp, tooltip),
                            style, GUILayout.Width(19), GUILayout.Height(19));
        }

        void AssemblyCleanup() {
            AssemblyReloadEvents.beforeAssemblyReload -= AssemblyCleanup;
            DestroyImmediate(preview);
            Close();
        }

        void OnDisable() {
            OnTileCreation = null;
            DestroyImmediate(preview);
            Resources.UnloadUnusedAssets();
        }
    }
}