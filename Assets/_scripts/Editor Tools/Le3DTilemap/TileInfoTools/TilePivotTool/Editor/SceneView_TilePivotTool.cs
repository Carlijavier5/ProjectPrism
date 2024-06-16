using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class TilePivotTool {

        private const string GRID_ORIENTATION = "_Orientation";

        private bool showSettings;
        private bool willHide;
        private GridSettingsPage settingsPage;
        private GridOrientation orientation;

        private void DrawSceneViewWindowHeader() {
            Handles.BeginGUI();
            Rect rect = GUILayout.Window(2, settings.sceneGUI.rect, DrawSceneViewWindow,
                                         "", EditorStyles.textArea);
            if (!rect.Equals(settings.sceneGUI.rect)) {
                settings.sceneGUI.rect = rect;
                EditorUtility.SetDirty(settings);
            } Handles.EndGUI();
        }

        private void DrawSceneViewWindow(int controlID) {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(200), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    GUILayout.Space(8);
                    Rect rect = GUILayoutUtility.GetRect(30, 14);
                    bool mouseInRect = rect.Contains(Event.current.mousePosition);
                    GUI.color = mouseInRect ? Color.white : new Vector4(1f, 1f, 1f, 0.6f);
                    GUI.Label(rect, iconGrip);
                    GUI.color = Color.white;
                    GUILayout.Space(8);
                    GUILayout.Label("Tile Pivot Tool", UIStyles.CenteredLabelBold);
                    GUILayout.Space(8);
                    GUI.color = UIColors.DefinedBlue;
                    Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(14), GUILayout.Height(14));
                    buttonRect = new Rect(buttonRect) { y = buttonRect.y - 1 };
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
                        if (showSettings) settingsPage = 0;
                    } GUILayout.Space(4);
                    if (mouseInRect && Event.current.type == EventType.MouseDown
                        && Event.current.button == 0) {
                        willHide = true;
                    } else if (Event.current.type == EventType.MouseDrag) {
                        willHide = false;
                    } else if (Event.current.type == EventType.MouseUp) {
                        if (mouseInRect && willHide) {
                            settings.sceneGUI.hideContents = !settings.sceneGUI.hideContents;
                            EditorUtility.SetDirty(settings);
                        } willHide = false;
                    }
                } if (!settings.sceneGUI.hideContents) DrawContent();
            } GUI.DragWindow();
        }

        private void DrawToolModeButton(GridSettingsPage page, RectOffset margin) {
            GUI.backgroundColor = settingsPage == page ? UIColors.DefinedBlue : Color.white;
            GUIStyle style = new(GUI.skin.button) { margin = margin };
            if (GUILayout.Button(System.Enum.GetName(typeof(GridSettingsPage), page),
                                 style, GUILayout.Width(60))) {
                settingsPage = page;
            } GUI.backgroundColor = Color.white;
        }

        private void DrawContent() {
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox)) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                        if (showSettings) {
                            DrawToolModeButton(GridSettingsPage.View, new RectOffset() { right = 0 });
                            DrawToolModeButton(GridSettingsPage.Options, new RectOffset() { right = 0, left = 0 });
                            DrawToolModeButton(GridSettingsPage.Colors, new RectOffset() { left = 0 });
                            GUI.enabled = true;
                        } else {
                            GUILayout.Button("Move Button");
                            GUILayout.Button("Orientation");
                        }
                    } GUILayout.FlexibleSpace();
                } GUIStyle paddedBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 6, 6) };
                using (new EditorGUILayout.VerticalScope(paddedBox, GUILayout.Height(50))) {
                    if (showSettings) {
                        using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                            switch (settingsPage) {
                                case GridSettingsPage.View:
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Size:");
                                        EditorGUIUtility.labelWidth = 0;
                                        GUILayout.FlexibleSpace();
                                        GUI.enabled = false;
                                        settings.size = EditorGUILayout.IntField(settings.size,
                                                                                 GUILayout.Width(80));
                                    } using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Component:");
                                        GUI.enabled = false;
                                        EditorGUILayout.ObjectField(gridQuad, typeof(DynamicGridQuad),
                                                                    false, GUILayout.Width(80));
                                        GUI.enabled = true;
                                    } break;
                                case GridSettingsPage.Options:
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Ignore zTest:");
                                        GUILayout.FlexibleSpace();
                                        settings.ignoreZTest = GUILayout.Toggle(settings.ignoreZTest, "");
                                    } using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Follow Camera:");
                                        GUILayout.FlexibleSpace();
                                        settings.followCamera = GUILayout.Toggle(settings.followCamera, "");
                                    } break;
                                case GridSettingsPage.Colors:
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Base:");
                                        GUILayout.FlexibleSpace();
                                        settings.baseColor = EditorGUILayout.ColorField(settings.baseColor);
                                    } using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Hint:");
                                        GUILayout.FlexibleSpace();
                                        settings.baseColor = EditorGUILayout.ColorField(settings.baseColor);
                                    } break;
                            } if (changeScope.changed) EditorUtility.SetDirty(settings);
                        }
                    } else {
                        using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.Label("Orientation:");
                            GUILayout.FlexibleSpace();
                            GridOrientation newOrientation = (GridOrientation) EditorGUILayout.EnumPopup(
                                                                orientation, GUILayout.Width(80));
                            if (newOrientation != orientation) {
                                SetGridOrienration(newOrientation);
                                orientation = newOrientation;
                            }
                        } using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.Label("Toggle Grid:");
                            GUILayout.FlexibleSpace();
                            GUILayout.Button("On", GUILayout.Width(80));
                        }
                    }
                }
            }
        }

        private void SetGridOrienration(GridOrientation orientation) {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetInt(GRID_ORIENTATION, (int) orientation);
            gridQuad.Renderer.SetPropertyBlock(mpb);

            gridQuad.transform.eulerAngles = orientation switch {
                GridOrientation.XZ => new Vector3(90, 0, 0),
                GridOrientation.XY => Vector3.zero,
                _ => new Vector3(0, 90, 0),
            };
        }
    }
}