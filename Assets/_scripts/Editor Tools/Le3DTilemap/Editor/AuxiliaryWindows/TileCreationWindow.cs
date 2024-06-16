using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public class TileCreationWindow : EditorWindow {

        public event System.Action<TileData> OnTileCreation;

        private Editor preview;
        private GameObject prefab;
        private string tileName = "No Tile Selected";
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

        void OnGUI() {
            AssetUtils.InvalidNameCondition nameValidity = AssetUtils.ValidateFilename(tileName);
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
                } EditorGUILayout.GetControlRect(GUILayout.Height(2));
                GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                EditorGUILayout.GetControlRect(GUILayout.Height(1));
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.GetControlRect(GUILayout.Width(2));
                    using (new EditorGUILayout.VerticalScope()) {
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Prefab", UIStyles.CenteredLabelBold);
                        } using (new EditorGUILayout.HorizontalScope()) {
                            EditorGUILayout.GetControlRect(GUILayout.Width(1));
                            using (var scope = new EditorGUI.ChangeCheckScope()) {
                                prefab = EditorGUILayout.ObjectField(prefab, typeof(GameObject), false) as GameObject;
                                if (scope.changed) {
                                    if (prefab != null) tileName = prefab.name;
                                }
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        } EditorGUILayout.GetControlRect(GUILayout.Height(3));
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
                                    GUI.color = nameValidity == AssetUtils.InvalidNameCondition.None ? Color.white
                                                                                                     : UIColors.DarkRed;
                                    string tooltip = nameValidity switch {
                                        AssetUtils.InvalidNameCondition.None => "Name is valid",
                                        AssetUtils.InvalidNameCondition.Empty => "Name cannot be empty or whitespace",
                                        _ => "Naming convention violated:"
                                           + "\n- First letter must be capitalized or an underscore"
                                           + "\n- Cannot contain invalid characters"
                                    }; tileName = EditorGUILayout.TextField("Name:", tileName);
                                    GUIStyle style = new(EditorStyles.label) { contentOffset = new (0, -2) };
                                    GUILayout.Label(new GUIContent(iconHelp, tooltip),
                                                    style, GUILayout.Width(19), GUILayout.Height(19));
                                    GUI.color = Color.white;
                                } EditorGUILayout.Popup("Complexity:", 0, new string[] { "Single" });
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        } EditorGUIUtility.labelWidth = 0;
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

        private void DisplayMessages() {
            /*GUIUtils.DrawCustomHelpBox(" Missing Editor Node. Won't be added to Palette;",
                                                       EditorUtils.FetchIcon("d_P4_Offline"));*/
            GUIUtils.DrawCustomHelpBox(" Complexity: No collider edits required;", iconGood);
            /*GUIUtils.DrawCustomHelpBox(" Complexity: Additional collider edits required;",
                                       EditorUtils.FetchIcon("d_P4_OutOfSync"));*/
        }

        void AssemblyCleanup() {
            AssemblyReloadEvents.beforeAssemblyReload -= AssemblyCleanup;
            DestroyImmediate(preview);
            Close();
        }

        void OnDisable() {
            OnTileCreation = null;
            DestroyImmediate(preview);
        }
    }
}