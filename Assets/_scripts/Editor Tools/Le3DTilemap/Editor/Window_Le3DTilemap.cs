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

        private readonly string[] paletteDropdown = new string[] { "Create New..." };
        private readonly string[] tileAddDropdown = new string[] { "Add Tile...",
                                                                   "New Tile... "};
        private bool awaitOPCallback;

        public static Le3DTilemapWindow Launch(Le3DTilemapTool tool) {
            Le3DTilemapWindow window = GetWindow<Le3DTilemapWindow>("Le3D Tilemap");
            window.tool = tool;
            return window;
        }

        void OnEnable() {
            if (prefs is null) {
                AssetUtils.TryRetrieveAsset(out prefs);
            }
        }

        void Update() => Repaint();

        void OnGUI() {
            if (prefs is null) {
                DrawMissingPrefsPrompt();
                return;
            } DrawMainToolbar();
            DrawPaletteEditor();
        }

        private void DrawMissingPrefsPrompt() {
            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(1))) {
                        GUIUtils.WindowBoxLabel("Missing Window Preferences");
                        using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Search Again", EditorUtils.FetchIcon("Search Icon")),
                                                 GUILayout.Width(150), GUILayout.Height(20))) {
                                AssetUtils.TryRetrieveAsset(out prefs);
                            } GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Create Asset", EditorUtils.FetchIcon("Toolbar Plus")),
                                                 GUILayout.Width(150), GUILayout.Height(20))) {
                                prefs = AssetUtils.CreateAsset<Le3DTilemapWindowPrefs>("New Window Preferences Asset",
                                                                                       "Le3DTilemapWindowPrefs");
                                GUIUtility.ExitGUI();
                            } GUILayout.FlexibleSpace();
                        }
                    } GUILayout.FlexibleSpace();
                } GUILayout.FlexibleSpace();
            }
        }

        private void DrawMainToolbar() {
            GUIContent[] toolbarContent = new GUIContent[] { new(EditorUtils.FetchIcon("Grid.BoxTool")),
                                                             new(EditorUtils.FetchIcon("Grid.PaintTool")),
                                                             new(EditorUtils.FetchIcon("Grid.PickingTool")), };
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
    }
}