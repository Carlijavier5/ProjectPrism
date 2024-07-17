using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    public enum PaletteEditMode { Editable, Focused }

    public partial class Le3DTilemapWindow : EditorWindow {

        private Le3DTilemapTool tool;

        private Le3DTilemapWindowSettings settings;
        private bool showSettings;

        private Texture2D iconSearch, iconPlus, iconTilemap,
                          iconPlusMore, iconSettings;

        public static Le3DTilemapWindow Launch(Le3DTilemapTool tool) {
            Le3DTilemapWindow window = GetWindow<Le3DTilemapWindow>("Le3D Tilemap");
            window.tool = tool;
            return window;
        }

        void OnEnable() {
            ToolManager.activeToolChanged += FindToolInstance;
            tool = null ;
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } LoadIcons();
            if (settings && settings.activePalette) {
                UpdateSearchResults(searchString, out shownTiles);
            }
        }

        void OnDisable() {
            ToolManager.activeToolChanged -= FindToolInstance;
            Resources.UnloadUnusedAssets();
        }

        void OnGUI() {
            ValidateRepaint();
            if (HasNullSettings()) {
                if (settings && settings.activePalette) {
                    UpdateSearchResults(searchString, out shownTiles);
                } return;
            } DrawWindowTitle();
            DrawPaletteEditor();
        }

        private bool HasNullSettings() {
            bool missingPrefs = settings == null;
            if (missingPrefs) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope()) {
                        GUILayout.FlexibleSpace();
                        SceneViewUtils.DrawMissingSettingsPrompt(ref settings, "Missing Window Preferences",
                                                             "New Window Preferences Asset",
                                                             iconSearch, iconPlus);
                        GUILayout.FlexibleSpace();
                    } GUILayout.FlexibleSpace();
                }
            } return missingPrefs;
        }

        private void ValidateRepaint() {
            EventType eType = Event.current.type;
            if (mouseInScope && (eType == EventType.MouseMove
                || eType == EventType.MouseDown
                || eType == EventType.MouseUp
                || eType == EventType.MouseDrag)) Repaint();
        }

        private void DrawWindowTitle() {
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox, GUILayout.Height(50))) {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox, GUILayout.Width(128))) {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Tile Palette Editor", UIStyles.CenteredLabelBold, GUILayout.Height(16));
                        GUILayout.FlexibleSpace();
                    } using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox, GUILayout.Width(26), GUILayout.Height(26))) {
                        GUILayout.FlexibleSpace();
                        GUI.color = UIColors.DefinedBlue;
                        Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(14), GUILayout.Height(14));
                        if (showSettings) {
                            GUI.Label(new(buttonRect) {
                                x = buttonRect.x - 2,
                                y = buttonRect.y - 2,
                                width = buttonRect.width + 6,
                                height = buttonRect.height + 6
                            }, "", GUI.skin.button);
                        } GUI.color = Color.white;
                        if (GUI.Button(buttonRect, iconSettings, EditorStyles.iconButton)) {
                            showSettings = !showSettings;
                        } GUILayout.FlexibleSpace();
                    } GUILayout.FlexibleSpace();
                } GUILayout.FlexibleSpace();
            }
        }

        private void LoadIcons() {
            EditorUtils.LoadIcon(ref iconSearch, EditorUtils.ICON_SEARCH);
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
            EditorUtils.LoadIcon(ref iconTilemap, "d_Tilemap.ActiveTargetLayers");
            EditorUtils.LoadIcon(ref iconPlusMore, EditorUtils.ICON_PLUS_MORE);
            EditorUtils.LoadIcon(ref iconSettings, EditorUtils.ICON_SETTINGS);
        }
    }
}