using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    public abstract partial class GridTool {

        protected bool allowDirectGridMode;
        private bool showGridSettings;
        private bool willHideGridWindow;
        private float gridHeightDiff;

        protected GridOrientation gridOrientation;
        private GridSettingsPage settingsPage;

        private const string GRID_ORIENTATION = "_Orientation";
        private const string GRID_FALLOFF_DIST = "_TDistance";
        private const string GRID_GIRTH = "_LineThickness";
        private const string GRID_COLOR = "_GridColour";

        protected void DrawGridWindow(SceneView sceneView, bool hasHint) {
            Rect firstWindowRect = DrawGridSceneViewHeader(sceneView);
            DrawHintWindow(firstWindowRect, hasHint);
        }

        private Rect DrawGridSceneViewHeader(SceneView sceneView) {
            Handles.BeginGUI();
            Rect rect = GUILayout.Window(2, gridSettings.sceneGUI.rect, DrawGridSceneViewWindow,
                                         "", EditorStyles.textArea);
            if (!rect.Equals(gridSettings.sceneGUI.rect)) {
                rect = EditorUtils.PreventWindowOverflow(sceneView.position, rect);
                gridSettings.sceneGUI.rect = rect;
                EditorUtility.SetDirty(gridSettings);
            } Handles.EndGUI();
            return rect;
        }

        private void DrawHintWindow(Rect firstWindow, bool hasHint) {
            if (!hasHint) return;
            Handles.BeginGUI();
            Rect rect = new(firstWindow) {
                y = firstWindow.y - 40,
                height = 30,
            }; GUILayout.Window(3, rect, DrawHintContent,
                                "", EditorStyles.textArea);
            Handles.EndGUI();
        }

        private void DrawGridSceneViewWindow(int controlID) {
            if (!gridSettings) return;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(208), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    GUILayout.Space(8);
                    Rect rect = GUILayoutUtility.GetRect(30, 14);
                    bool mouseInRect = rect.Contains(Event.current.mousePosition);
                    GUI.color = mouseInRect ? Color.white : new Vector4(1f, 1f, 1f, 0.6f);
                    GUI.Label(rect, iconGrip);
                    GUI.color = Color.white;
                    if (!gridSettings.sceneGUI.hideContents) {
                        GUILayout.Label("Grid Settings", UIStyles.CenteredLabelBold);
                        GUILayout.Space(20);
                        Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(14), GUILayout.Height(14));
                        buttonRect = new Rect(buttonRect) { y = buttonRect.y - 1 };
                        if (showGridSettings) {
                            GUI.color = UIColors.DefinedBlue;
                            GUI.Label(new(buttonRect) {
                                x = buttonRect.x - 2,
                                y = buttonRect.y - 2,
                                width = buttonRect.width + 6,
                                height = buttonRect.height + 6
                            }, "", GUI.skin.button);
                            GUI.color = Color.white;
                        } if (GUI.Button(buttonRect, iconSettings, EditorStyles.iconButton)) {
                            showGridSettings = !showGridSettings;
                            if (showGridSettings) settingsPage = 0;
                        } GUILayout.Space(4);
                    } else {
                        GUIContent content = new GUIContent(" Move", iconMove, "Move (W)");
                        GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                        Rect bRect = EditorGUILayout.GetControlRect(false, 14, style);
                        bRect = new(bRect) { y = bRect.y - 3, height = bRect.height + 6 };
                        DrawGridModeButton(bRect, GridInputMode.Move, content, style);
                        content = new GUIContent(" Turn", iconTurn, " Turn (R)");
                        style = new(GUI.skin.button) { margin = { left = 0 } };
                        bRect = EditorGUILayout.GetControlRect(false, 14, style);
                        bRect = new(bRect) { y = bRect.y - 3, height = bRect.height + 6 };
                        DrawGridModeButton(bRect, GridInputMode.Turn, content, style);
                        GUI.enabled = true;
                        GUI.backgroundColor = Color.white;
                    }
                    if (mouseInRect && Event.current.type == EventType.MouseDown
                        && Event.current.button == 0) {
                        willHideGridWindow = true;
                    } else if (Event.current.type == EventType.MouseDrag) {
                        willHideGridWindow = false;
                    } else if (Event.current.type == EventType.MouseUp) {
                        if (mouseInRect && willHideGridWindow) {
                            bool hide = gridSettings.sceneGUI.hideContents = !gridSettings.sceneGUI.hideContents;
                            if (gridSettings.sceneGUI.hideContents) {
                                gridHeightDiff = gridSettings.sceneGUI.rect.height * 0.715f;
                            } float diffSign = hide ? 1 : -1;
                            gridSettings.sceneGUI.rect = new Rect(gridSettings.sceneGUI.rect) {
                                y = gridSettings.sceneGUI.rect.y + gridHeightDiff * diffSign,
                                height = gridSettings.sceneGUI.rect.height - gridHeightDiff * diffSign,
                            }; EditorUtility.SetDirty(gridSettings);
                        } willHideGridWindow = false;
                    }
                } if (!gridSettings.sceneGUI.hideContents) DrawContent();
            } GUI.DragWindow();
        }

        protected virtual void DrawHintContent(int controlID) { }

        private void DrawContent() {
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox)) {
                if (gridQuad == null) {
                    GUIUtils.DrawScopeCenteredText("Missing Dynamic Quad;\n"
                                                   + "Check Grid Settings!");
                    if (GUILayout.Button("Reload")) InitializeLocalGrid();
                } else {
                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                            if (showGridSettings) {
                                GUIStyle style = new(GUI.skin.button) { margin = { left = 0 },
                                                                        contentOffset = new Vector2(0, -1),
                                                                        fontSize = 10 };
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
                                    style = new(GUI.skin.button) { margin = { right = 0 },
                                                                   fontSize = 9 };
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
                                GUIContent content = new GUIContent(" Move", iconMove, "Move (W)");
                                GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                                Rect rect = EditorGUILayout.GetControlRect(false, 19, style, 
                                                                           GUILayout.Width(90));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawGridModeButton(rect, GridInputMode.Move, content, style);
                                content = new GUIContent(" Turn", iconTurn, " Turn (R)");
                                style = new(GUI.skin.button) { margin = { left = 0 } };
                                rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                      GUILayout.Width(90));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawGridModeButton(rect, GridInputMode.Turn, content, style);
                                GUI.enabled = true;
                                GUI.backgroundColor = Color.white;
                            }
                        } GUILayout.FlexibleSpace();
                    } GUIStyle paddedBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 6, 6) };
                    using (new EditorGUILayout.VerticalScope(paddedBox, GUILayout.Height(50))) {
                        GUIStyle lStyle = new(GUI.skin.label) { contentOffset = new Vector2(0, -1) };
                        if (showGridSettings) {
                            using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                                switch (settingsPage) {
                                    case GridSettingsPage.Shortcuts:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.FlexibleSpace();
                                            GUIStyle nStyle = new(GUI.skin.label) { margin = { left = 0, right = 0 },
                                                                                    fontStyle = FontStyle.Italic,
                                                                                    contentOffset = new Vector2(0, -1) };
                                            Rect rect = EditorGUILayout.GetControlRect(false, 17, nStyle, 
                                                                                       GUILayout.Width(20));
                                            rect = new(rect) { y = rect.y - 4, height = rect.height + 6 };
                                            GUI.Label(rect, iconSettings);
                                            GUILayout.Label("Keybindings Editor", nStyle);
                                            GUILayout.FlexibleSpace();
                                        } if (GUILayout.Button("Launch")) {

                                        } break;
                                    case GridSettingsPage.Raycasts:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Input Distance:");
                                            EditorGUIUtility.labelWidth = 0;
                                            GUILayout.FlexibleSpace();
                                            gridSettings.raycastDistance = EditorGUILayout.IntField(gridSettings
                                                                           .raycastDistance, GUILayout.Width(75));
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("CD Multiplier:");
                                            EditorGUIUtility.labelWidth = 0;
                                            GUILayout.FlexibleSpace();
                                            gridSettings.raycastCDMult = EditorGUILayout.DoubleField(gridSettings
                                                                         .raycastCDMult, GUILayout.Width(75));
                                        } break;
                                    case GridSettingsPage.Size:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Diameter:");
                                            EditorGUIUtility.labelWidth = 0;
                                            GUILayout.FlexibleSpace();
                                            int size = EditorGUILayout.IntField(gridSettings.diameter,
                                                                                GUILayout.Width(90));
                                            if (gridSettings.diameter != size) SetGridDiameter(size);
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Thickness:");
                                            EditorGUIUtility.labelWidth = 0;
                                            GUILayout.FlexibleSpace();
                                            float girth = EditorGUILayout.FloatField(gridSettings.thickness,
                                                                                     GUILayout.Width(90));
                                            if (gridSettings.thickness != girth) SetGridThickness(girth);
                                        } break;
                                    case GridSettingsPage.View:
                                        GUIStyle style = new(GUI.skin.button) { margin = new() };
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Ignore zTest:", lStyle);
                                            GUILayout.FlexibleSpace();
                                            GUIUtils.OnOffButton(gridSettings.ignoreZTest, out bool ignoreZTest,
                                                                 style, GUILayout.Width(60));
                                            if (ignoreZTest != gridSettings.ignoreZTest) SetIgnoreZTest(ignoreZTest);
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Follow Camera:", lStyle);
                                            GUILayout.FlexibleSpace();
                                            GUIUtils.OnOffButton(gridSettings.followCamera, out gridSettings.followCamera,
                                                                 style, GUILayout.Width(60));
                                        } break;
                                    case GridSettingsPage.Colors:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Base:", GUILayout.Width(35));
                                            GUILayout.FlexibleSpace();
                                            Color baseColor = EditorGUILayout.ColorField(gridSettings.baseColor,
                                                                                            GUILayout.Width(125));
                                            if (gridSettings.baseColor != baseColor) SetGridColor(baseColor);
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Hint:", GUILayout.Width(35));
                                            GUILayout.FlexibleSpace();
                                            gridSettings.hintColor = EditorGUILayout.ColorField(gridSettings.hintColor,
                                                                                            GUILayout.Width(125));
                                        } break;
                                } if (changeScope.changed) EditorUtility.SetDirty(gridSettings);
                            }
                        } else {
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Orientation:", lStyle);
                                GUILayout.FlexibleSpace();
                                GUIStyle style = new(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter,
                                                                           margin = new() };
                                GridOrientation newOrientation = (GridOrientation) EditorGUILayout.EnumPopup(
                                                                    gridOrientation, style, GUILayout.Width(80));
                                if (newOrientation != gridOrientation) {
                                    SetGridOrientation(newOrientation);
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Toggle Grid:", lStyle);
                                GUILayout.FlexibleSpace();
                                GUIStyle style = new(GUI.skin.button) { margin = new() };
                                GUIUtils.OnOffButton(gridQuad.gameObject.activeSelf,
                                                     out bool enabled, style, GUILayout.Width(80));
                                if (gridQuad.gameObject.activeSelf != enabled) ToggleQuad(enabled);
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

        private void DrawGridModeButton(Rect rect, GridInputMode mode, 
                                        GUIContent content, GUIStyle style) {
            GUI.backgroundColor = overrideInput == mode ? UIColors.DefinedGreen
                                : defaultInput == mode ? UIColors.DefinedBlue
                                                       : Color.white;
            GUI.enabled = overrideInput == 0 && allowDirectGridMode
                       || overrideInput == mode;
            if (GUI.Button(rect, content, style)) {
                defaultInput = defaultInput == mode ? GridInputMode.None
                                                    : mode;
                overrideInput = GridInputMode.None;
            }
        }

        private void SetGridOrientation(GridOrientation orientation) {
            MaterialPropertyBlock mpb = new();
            gridQuad.Renderer.GetPropertyBlock(mpb);
            mpb.SetInt(GRID_ORIENTATION, (int) orientation);
            gridQuad.Renderer.SetPropertyBlock(mpb);

            gridQuad.transform.eulerAngles = orientation switch {
                GridOrientation.XZ => new Vector3(90, 0, 0),
                GridOrientation.XY => Vector3.zero,
                _ => new Vector3(0, 90, 0),
            }; this.gridOrientation = orientation;
        }

        private void ToggleQuad(bool toggle) {
            gridQuad.gameObject.SetActive(toggle);
        }

        private void SetGridDiameter(int size) {
            gridQuad.transform.localScale = Vector3Int.one * size;
            MaterialPropertyBlock mpb = new();
            gridQuad.Renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(GRID_FALLOFF_DIST, size / 3);
            gridQuad.Renderer.SetPropertyBlock(mpb);
            gridSettings.diameter = size;
        }

        private void SetGridThickness(float girth) {
            MaterialPropertyBlock mpb = new();
            gridQuad.Renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(GRID_GIRTH, girth);
            gridQuad.Renderer.SetPropertyBlock(mpb);
            gridSettings.thickness = girth;
        }

        private void SetIgnoreZTest(bool ignoreZTest) {
            gridQuad.Renderer.sharedMaterial = ignoreZTest ? gridSettings.ignoreZMaterial
                                                           : gridSettings.baseMaterial;
            gridSettings.ignoreZTest = ignoreZTest;
        }

        private void SetGridColor(Color color) {
            MaterialPropertyBlock mpb = new();
            gridQuad.Renderer.GetPropertyBlock(mpb);
            mpb.SetColor(GRID_COLOR, color);
            gridQuad.Renderer.SetPropertyBlock(mpb);
            gridSettings.baseColor = color;
        }

        private void ResetGridWindowProperties() {
            showGridSettings = false;
            settingsPage = 0;
        }
    }
}