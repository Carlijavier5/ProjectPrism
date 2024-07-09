using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapTool {

        private GridSettingsPage settingsPage;
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
                                heightDiff = settings.sceneGUI.rect.height * 0.57f;
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
                                    DrawSettingsPageButton(GridSettingsPage.Shortcuts,
                                                           new RectOffset() { right = 0 },
                                                           GUILayout.Width(78));
                                    DrawSettingsPageButton(GridSettingsPage.Raycasts,
                                                           new RectOffset() { left = 0 },
                                                           GUILayout.Width(78));
                                    if (GUILayout.Button("▶", style,
                                        GUILayout.Width(24), GUILayout.Height(19))) {
                                        settingsPage = GridSettingsPage.Size;
                                    }
                                } else {
                                    style = new(GUI.skin.button) {
                                        margin = { right = 0 },
                                        fontSize = 9
                                    };
                                    if (GUILayout.Button("◀", style,
                                        GUILayout.Width(24), GUILayout.Height(19))) {
                                        settingsPage = GridSettingsPage.Shortcuts;
                                    }
                                    DrawSettingsPageButton(GridSettingsPage.Size,
                                                           new RectOffset() { right = 0 },
                                                           GUILayout.Width(52));
                                    DrawSettingsPageButton(GridSettingsPage.View,
                                                           new RectOffset() { right = 0, left = 0 },
                                                           GUILayout.Width(52));
                                    DrawSettingsPageButton(GridSettingsPage.Colors,
                                                           new RectOffset() { left = 0 },
                                                           GUILayout.Width(52));
                                }
                            } else {
                                GUIContent content = new GUIContent(iconSelect, "Select (W)");
                                GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                                Rect rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                           GUILayout.Width(45));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawToolModeButton(rect, ToolMode.Select, content, style);

                                content = new GUIContent(iconPaint, "Paint (R)");
                                style = new(GUI.skin.button) { margin = { right = 0, left = 0 } };
                                rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                      GUILayout.Width(45));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawToolModeButton(rect, ToolMode.Paint, content, style);

                                content = new GUIContent(iconFill, "Fill (R)");
                                rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                      GUILayout.Width(45));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawToolModeButton(rect, ToolMode.Fill, content, style);

                                content = new GUIContent(iconPick, "Pick (R)");
                                style = new(GUI.skin.button) { margin = { left = 0 } };
                                rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                      GUILayout.Width(45));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawToolModeButton(rect, ToolMode.Pick, content, style);

                                GUI.enabled = true;
                                GUI.backgroundColor = Color.white;
                            }
                        } GUILayout.FlexibleSpace();
                    }

                    GUIStyle paddedBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 6, 6) };
                    using (new EditorGUILayout.VerticalScope(paddedBox, GUILayout.Height(50))) {
                        GUIStyle lStyle = new(GUI.skin.label) { contentOffset = new Vector2(0, -1) };
                        if (showSettings) {
                            using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                                switch (settingsPage) {
                                    case GridSettingsPage.Raycasts:
                                        break;
                                    case GridSettingsPage.Colors:
                                        break;
                                } if (changeScope.changed) EditorUtility.SetDirty(settings);
                            }
                        } else {
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Label", lStyle);
                                GUILayout.FlexibleSpace();
                            }
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Label", lStyle);
                                GUILayout.FlexibleSpace();
                            }
                        }
                    }
                }
            }
        }

        private void DrawSettingsPageButton(GridSettingsPage page, RectOffset margin,
                                    params GUILayoutOption[] options) {
            GUI.backgroundColor = settingsPage == page ? UIColors.DefinedBlue : Color.white;
            GUIStyle style = new(GUI.skin.button) { margin = margin };
            if (GUILayout.Button(System.Enum.GetName(typeof(GridSettingsPage), page),
                                 style, options)) {
                settingsPage = page;
            } GUI.backgroundColor = Color.white;
        }

        private void DrawToolModeButton(Rect rect, ToolMode mode,
                                        GUIContent content, GUIStyle style) {
            GUI.backgroundColor = toolMode == mode ? UIColors.DefinedBlue
                                                   : Color.white;
            if (GUI.Button(rect, content, style)) {
                SetToolMode(toolMode);
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