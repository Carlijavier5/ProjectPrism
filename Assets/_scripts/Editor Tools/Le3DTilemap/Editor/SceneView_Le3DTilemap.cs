using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapTool {

        private enum ToolSettingsPage { Select, MultiSelect, Paint, Fill, Clear }
        private ToolSettingsPage settingsPage;
        private ToolMode toolMode;

        private bool willHide;
        private bool showSettings;
        private float heightDiff;

        private void DrawSceneViewWindowHeader(SceneView sceneView) {
            Handles.BeginGUI();
            Rect rect = GUILayout.Window(4, settings.sceneGUI.rect, DrawSceneViewWindow,
                                         "", EditorStyles.textArea);
            if (!rect.Equals(settings.sceneGUI.rect)) {
                rect = EditorUtils.PreventWindowOverflow(sceneView.position, rect);
                settings.sceneGUI.rect = rect;
                EditorUtility.SetDirty(settings);
            } Handles.EndGUI();
        }

        private void DrawSceneViewWindow(int controlID) {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(208), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    GUILayout.Space(8);
                    Rect rect = GUILayoutUtility.GetRect(30, 14);
                    bool mouseInRect = rect.Contains(Event.current.mousePosition);
                    GUI.color = mouseInRect ? Color.white : new Vector4(1f, 1f, 1f, 0.6f);
                    GUI.Label(rect, iconGrip);
                    GUI.color = Color.white;
                    GUILayout.Space(10);
                    GUILayout.Label("Tilemap Tool", UIStyles.CenteredLabelBold);
                    GUILayout.Space(21);
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
                    } GUILayout.Space(4);
                    if (mouseInRect && Event.current.type == EventType.MouseDown
                        && Event.current.button == 0) {
                        willHide = true;
                    } else if (Event.current.type == EventType.MouseDrag) {
                        willHide = false;
                    } else if (Event.current.type == EventType.MouseUp) {
                        if (mouseInRect && willHide) {
                            bool hide = settings.sceneGUI.hideContents = !settings.sceneGUI.hideContents;
                            if (settings.sceneGUI.hideContents) {
                                heightDiff = settings.sceneGUI.rect.height * 0.755f;
                            } float diffSign = hide ? 1 : -1;
                            settings.sceneGUI.rect = new Rect(settings.sceneGUI.rect) {
                                y = settings.sceneGUI.rect.y + heightDiff * diffSign,
                                height = settings.sceneGUI.rect.height - heightDiff * diffSign,
                            }; EditorUtility.SetDirty(settings);
                        } willHide = false;
                    }
                } if (!settings.sceneGUI.hideContents) DrawContent();
            } GUI.DragWindow();
        }

        private void DrawContent() {
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox)) {
                if (sceneHook == null) {
                    GUIUtils.DrawCustomHelpBox("Scene Hook Not Found", iconWarning);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button("Search Again")) {
                            sceneHook = FindAnyObjectByType<LevelGridHook>();
                        } if (GUILayout.Button("Create Hook")) {
                            GameObject hookGO = new ("Level Grid");
                            sceneHook = hookGO.AddComponent<LevelGridHook>();
                            Undo.RegisterCreatedObjectUndo(hookGO, "Create Scene Hook (Level Grid Hook)");
                            EditorApplication.RepaintHierarchyWindow();
                        }
                    }
                } else {
                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                            if (showSettings) {
                                GUIStyle style = new(GUI.skin.button) {
                                    margin = { left = 0 },
                                    contentOffset = new Vector2(0, -1),
                                    fontSize = 10
                                };
                                if ((int) settingsPage < 2) {
                                    DrawSettingsPageButton(ToolSettingsPage.Select,
                                                           new RectOffset() { right = 0 },
                                                           GUILayout.Width(78));
                                    DrawSettingsPageButton(ToolSettingsPage.MultiSelect,
                                                           new RectOffset() { left = 0 },
                                                           GUILayout.Width(78));
                                    if (GUILayout.Button("▶", style,
                                        GUILayout.Width(24), GUILayout.Height(19))) {
                                        settingsPage = ToolSettingsPage.Paint;
                                    }
                                } else {
                                    style = new(GUI.skin.button) {
                                        margin = { right = 0 },
                                        fontSize = 9
                                    };
                                    if (GUILayout.Button("◀", style,
                                        GUILayout.Width(24), GUILayout.Height(19))) {
                                        settingsPage = ToolSettingsPage.Select;
                                    }
                                    DrawSettingsPageButton(ToolSettingsPage.Paint,
                                                           new RectOffset() { right = 0 },
                                                           GUILayout.Width(52));
                                    DrawSettingsPageButton(ToolSettingsPage.Fill,
                                                           new RectOffset() { right = 0, left = 0 },
                                                           GUILayout.Width(52));
                                    DrawSettingsPageButton(ToolSettingsPage.Clear,
                                                           new RectOffset() { left = 0 },
                                                           GUILayout.Width(52));
                                }
                            } else {
                                using (new EditorGUILayout.VerticalScope()) {
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        GUIContent content = new GUIContent(iconSelect, "Select (W)");
                                        GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                                        Rect rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                                   GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.Select, content, style);

                                        content = new GUIContent(iconPaint, "Paint (P)");
                                        style = new(GUI.skin.button) { margin = { right = 0, left = 0 } };
                                        rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                              GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.Paint, content, style);

                                        content = new GUIContent(iconFill, "Fill (F)");
                                        rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                              GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.Fill, content, style);

                                        content = new GUIContent(iconPick, "Pick (K)");
                                        style = new(GUI.skin.button) { margin = { left = 0 } };
                                        rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                              GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.Pick, content, style);
                                    } using (new EditorGUILayout.HorizontalScope()) {
                                        GUIContent content = new GUIContent(iconMSelect, "Multi-Select (W)");
                                        GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                                        Rect rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                                   GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.MSelect, content, style);

                                        content = new GUIContent(iconErase, "Erase (P)");
                                        style = new(GUI.skin.button) { margin = { right = 0, left = 0 } };
                                        rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                              GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.Erase, content, style);

                                        content = new GUIContent(iconClear, "Clear (F)");
                                        rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                              GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.Clear, content, style);
                                        GUI.enabled = false;
                                        content = new GUIContent(iconTransform, "Pick (K)");
                                        style = new(GUI.skin.button) { margin = { left = 0 } };
                                        rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                              GUILayout.Width(45));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        DrawToolModeButton(rect, ToolMode.Pick, content, style);
                                    }
                                }


                                GUI.enabled = true;
                                GUI.backgroundColor = Color.white;
                            }
                        } GUILayout.FlexibleSpace();
                    }

                    GUIStyle paddedBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 6, 6) };
                    using (new EditorGUILayout.VerticalScope(paddedBox, GUILayout.Height(23))) {
                        GUIStyle lStyle = new(GUI.skin.label) { contentOffset = new Vector2(0, -1) };
                        if (showSettings) {
                            using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                                GUIStyle style = new(GUI.skin.button) { margin = new() };
                                GUIContent content;
                                switch (settingsPage) {
                                    case ToolSettingsPage.Select:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            content = new ("Focus Inspector On Select");
                                            GUILayout.Label(content, GUILayout.Width(150));
                                            GUILayout.FlexibleSpace();
                                            settings.focusInspectorOnSelect
                                                = EditorGUILayout.Toggle(settings.focusInspectorOnSelect,
                                                                         GUILayout.Width(12));
                                            GUILayout.FlexibleSpace();
                                        } using (new EditorGUILayout.HorizontalScope()) {

                                        } break;
                                    case ToolSettingsPage.MultiSelect:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            content = new ("Include Partial Selections");
                                            GUILayout.Label(content, GUILayout.Width(150));
                                            GUILayout.FlexibleSpace();
                                            settings.includePartialSelectionsMSelect
                                                = EditorGUILayout.Toggle(settings.includePartialSelectionsMSelect,
                                                                         GUILayout.Width(12));
                                            GUILayout.FlexibleSpace();
                                        } using (new EditorGUILayout.HorizontalScope()) {

                                        } break;
                                    case ToolSettingsPage.Paint:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            content = new ("Focus Palette On Enter");
                                            GUILayout.Label(content, GUILayout.Width(150));
                                            GUILayout.FlexibleSpace();
                                            settings.focusPaletteOnEnterPaint
                                                = EditorGUILayout.Toggle(settings.focusPaletteOnEnterPaint,
                                                                         GUILayout.Width(12));
                                            GUILayout.FlexibleSpace();
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            content = new ("Rotate Tile to Surface");
                                            GUILayout.Label(content, GUILayout.Width(150));
                                            GUILayout.FlexibleSpace();
                                            settings.rotateTileToSurface
                                                = EditorGUILayout.Toggle(settings.rotateTileToSurface,
                                                                         GUILayout.Width(12));
                                            GUILayout.FlexibleSpace();
                                        } break;
                                    case ToolSettingsPage.Fill:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            content = new("Focus Palette On Enter");
                                            GUILayout.Label(content, GUILayout.Width(150));
                                            GUILayout.FlexibleSpace();
                                            settings.focusPaletteOnEnterFill
                                                = EditorGUILayout.Toggle(settings.focusPaletteOnEnterFill,
                                                                         GUILayout.Width(12));
                                            GUILayout.FlexibleSpace();
                                        } using (new EditorGUILayout.HorizontalScope()) {

                                        } break;
                                    case ToolSettingsPage.Clear:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            content = new("Include Partial Selections");
                                            GUILayout.Label(content, GUILayout.Width(150));
                                            GUILayout.FlexibleSpace();
                                            settings.includePartialSelectionsClear
                                                = EditorGUILayout.Toggle(settings.includePartialSelectionsClear,
                                                                         GUILayout.Width(12));
                                            GUILayout.FlexibleSpace();
                                        } using (new EditorGUILayout.HorizontalScope()) {

                                        } break;
                                } if (changeScope.changed) EditorUtility.SetDirty(settings);
                            }
                        } else {
                            switch (toolMode) {
                                case ToolMode.Select:
                                case ToolMode.MSelect:
                                    using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                                        GUILayout.Label("No Tiles Selected", UIStyles.CenteredLabelBold);
                                    } using (new EditorGUILayout.HorizontalScope()) {
                                        GUI.enabled = false;
                                        GUILayout.FlexibleSpace();
                                        GUIContent content = new (" Move", iconDisplace, "Move (W)");
                                        GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                                        Rect rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                                   GUILayout.Width(87));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        GUI.Button(rect, content, style);

                                        content = new (" Rotate", iconRotate, "Rotate (R)");
                                        style = new(GUI.skin.button) { margin = { left = 0 } };
                                        rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                              GUILayout.Width(87));
                                        rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                        GUI.Button(rect, content, style);
                                        GUILayout.FlexibleSpace();
                                        GUI.enabled = true;
                                    } break;
                                case ToolMode.Paint:
                                case ToolMode.Fill:
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Tile");
                                        Rect rect = EditorGUILayout.GetControlRect(false);
                                        rect = new Rect(rect) { y = rect.y - 1 };
                                        EditorGUI.ObjectField(rect, SelectedTile, typeof(TileData), false);
                                    } if (GUILayout.Button(new GUIContent(" Focus Palette", iconPalette),
                                                       GUILayout.Height(24))) {
                                        Le3DTilemapWindow.Launch(this);
                                    } break;
                                case ToolMode.Erase:
                                    break;
                                case ToolMode.Clear:
                                    break;
                                case ToolMode.Pick:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        protected override void DrawHintContent(int controlID) {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(208), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    string space = "---";
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"X: {(HasHint ? HintTile.x.ToString() : space)}", GUILayout.Width(45));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Y: {(HasHint ? HintTile.y.ToString() : space)}", GUILayout.Width(45));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Z: {(HasHint ? HintTile.z.ToString() : space)}", GUILayout.Width(45));
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawSettingsPageButton(ToolSettingsPage page, RectOffset margin,
                                    params GUILayoutOption[] options) {
            GUI.backgroundColor = settingsPage == page ? UIColors.DefinedBlue : Color.white;
            GUIStyle style = new(GUI.skin.button) { margin = margin };
            if (GUILayout.Button(System.Enum.GetName(typeof(ToolSettingsPage), page),
                                 style, options)) {
                settingsPage = page;
            } GUI.backgroundColor = Color.white;
        }

        private void DrawToolModeButton(Rect rect, ToolMode mode,
                                        GUIContent content, GUIStyle style) {
            GUI.backgroundColor = toolMode == mode ? UIColors.DefinedBlue : Color.white;
            if (GUI.Button(rect, content, style)) {
                SetToolMode(mode);
            }
        }

        private void SetToolMode(ToolMode toolMode) {
            this.toolMode = toolMode;
            switch (toolMode) {
                case ToolMode.Paint:
                case ToolMode.Fill:
                    break;
            }
        }
    }
}