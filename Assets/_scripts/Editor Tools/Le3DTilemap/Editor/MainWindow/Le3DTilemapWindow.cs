using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {

    public enum PaletteEditMode { Editable, Focused }

    public partial class Le3DTilemapWindow : EditorWindow {

        private Le3DTilemapTool tool;
        private Le3DTilemapWindowPrefs prefs;

        private Texture2D iconSearch, iconPlus, iconGridBox,
                          iconGridPaint, iconGridPicking,
                          iconTilemap, iconPlusMore;

        public static Le3DTilemapWindow Launch(Le3DTilemapTool tool) {
            Le3DTilemapWindow window = GetWindow<Le3DTilemapWindow>("Le3D Tilemap");
            window.tool = tool;
            return window;
        }

        void OnEnable() {
            if (prefs is null) {
                AssetUtils.TryRetrieveAsset(out prefs);
            } LoadIcons();
            if (prefs && prefs.activePalette) {
                UpdateSearchResults(searchString, out shownTiles);
            }
        }

        void OnDisable() {
            Resources.UnloadUnusedAssets();
        }

        void OnGUI() {
            ValidateRepaint();
            if (HasNullSettings()) {
                if (prefs && prefs.activePalette) {
                    UpdateSearchResults(searchString, out shownTiles);
                } return;
            } DrawMainToolbar();
            DrawPaletteEditor();
        }

        private bool HasNullSettings() {
            bool missingPrefs = prefs == null;
            if (missingPrefs) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope()) {
                        GUILayout.FlexibleSpace();
                        SceneViewUtils.DrawMissingSettingsPrompt(ref prefs, "Missing Window Preferences",
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

        private void DrawMainToolbar() {
            GUIContent[] toolbarContent = new GUIContent[] { new(iconGridBox),
                                                             new(iconGridPaint),
                                                             new(iconGridPicking) };
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox, GUILayout.Height(50))) {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                        int selected = 0;
                        GUILayout.Toolbar(selected, toolbarContent, GUILayout.Width(200), GUILayout.Height(24));
                    } GUILayout.FlexibleSpace();
                } GUILayout.FlexibleSpace();
            }
        }

        private void LoadIcons() {
            EditorUtils.LoadIcon(ref iconSearch, EditorUtils.ICON_SEARCH);
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
            EditorUtils.LoadIcon(ref iconGridBox, "Grid.BoxTool");
            EditorUtils.LoadIcon(ref iconGridPaint, "Grid.PaintTool");
            EditorUtils.LoadIcon(ref iconGridPicking, "Grid.PickingTool");
            EditorUtils.LoadIcon(ref iconTilemap, "d_Tilemap.ActiveTargetLayers");
            EditorUtils.LoadIcon(ref iconPlusMore, EditorUtils.ICON_PLUS_MORE);
        }
    }
}