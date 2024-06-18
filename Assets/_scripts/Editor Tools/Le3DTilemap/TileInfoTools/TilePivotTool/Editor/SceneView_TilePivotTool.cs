using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class TilePivotTool {

        private const string GRID_ORIENTATION = "_Orientation";
        private const string GRID_FALLOFF_DIST = "_TDistance";
        private const string GRID_GIRTH = "_LineThickness";
        private const string GRID_COLOR = "_GridColour";

        private bool showSettings;
        private bool willHide;

        private GridSettingsPage settingsPage;
        private GridOrientation orientation;

        private Rect DrawSceneViewWindowHeader() {
            Handles.BeginGUI();
            Rect rect = GUILayout.Window(2, settings.sceneGUI.rect, DrawSceneViewWindow,
                                         "", EditorStyles.textArea);
            if (!rect.Equals(settings.sceneGUI.rect)) {
                settings.sceneGUI.rect = rect;
                EditorUtility.SetDirty(settings);
            } Handles.EndGUI();
            return rect;
        }

        private void DrawHintWindow(Rect firstWindow) {
            if (!hasHint) return;
            Handles.BeginGUI();
            Rect rect = new(firstWindow) { y = firstWindow.y - 40,
                                           height = 30 };
            GUILayout.Window(3, rect, DrawHintContent, 
                             "", EditorStyles.textArea);
            Handles.EndGUI();
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
                    GUILayout.Label("Tile Pivot Tool", UIStyles.CenteredLabelBold);
                    GUILayout.Space(16);
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

        private void DrawHintContent(int controlID) {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(208), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"X: {hintTile.x}", GUILayout.Width(45));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Y: {hintTile.y}", GUILayout.Width(45));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Z: {hintTile.z}", GUILayout.Width(45));
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawSettingsPageButton(GridSettingsPage page, RectOffset margin) {
            GUI.backgroundColor = settingsPage == page ? UIColors.DefinedBlue : Color.white;
            GUIStyle style = new(GUI.skin.button) { margin = margin };
            if (GUILayout.Button(System.Enum.GetName(typeof(GridSettingsPage), page),
                                 style, GUILayout.Width(60))) {
                settingsPage = page;
            } GUI.backgroundColor = Color.white;
        }

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
                            if (showSettings) {
                                DrawSettingsPageButton(GridSettingsPage.View, new RectOffset() { right = 0 });
                                DrawSettingsPageButton(GridSettingsPage.Options, new RectOffset() { right = 0, left = 0 });
                                DrawSettingsPageButton(GridSettingsPage.Colors, new RectOffset() { left = 0 });
                                GUI.enabled = true;
                            } else {
                                GUIContent content = new GUIContent(" Move", iconMove, "Move (W)");
                                GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                                GUI.backgroundColor = overrideInput == GridInputMode.Move ? UIColors.DefinedGreen
                                                    : defaultInput == GridInputMode.Move ? UIColors.DefinedBlue
                                                                                         : Color.white;
                                GUI.enabled = overrideInput == 0 || overrideInput == GridInputMode.Move;
                                if (GUILayout.Button(content, style, GUILayout.Width(90))) {
                                    defaultInput = defaultInput == GridInputMode.Move ? GridInputMode.None
                                                                                      : GridInputMode.Move;
                                    overrideInput = GridInputMode.None;
                                } GUI.backgroundColor = overrideInput == GridInputMode.Turn ? UIColors.DefinedGreen
                                                      : defaultInput == GridInputMode.Turn ? UIColors.DefinedBlue
                                                                                           : Color.white;
                                content = new GUIContent(" Turn", iconTurn, " Turn (R)");
                                style = new(GUI.skin.button) { margin = { left = 0 } };
                                GUI.enabled = overrideInput == 0 || overrideInput == GridInputMode.Turn;
                                if (GUILayout.Button(content, style, GUILayout.Width(90))) {
                                    defaultInput = defaultInput == GridInputMode.Turn ? GridInputMode.None
                                                                                      : GridInputMode.Turn;
                                    overrideInput = GridInputMode.None;
                                } GUI.enabled = true;
                                GUI.backgroundColor = Color.white;
                            }
                        } GUILayout.FlexibleSpace();
                    } GUIStyle paddedBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 6, 6) };
                    using (new EditorGUILayout.VerticalScope(paddedBox, GUILayout.Height(50))) {
                        if (showSettings) {
                            using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                                switch (settingsPage) {
                                    case GridSettingsPage.View:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Diameter:");
                                            EditorGUIUtility.labelWidth = 0;
                                            GUILayout.FlexibleSpace();
                                            int size = EditorGUILayout.IntField(settings.diameter,
                                                                                GUILayout.Width(90));
                                            if (settings.diameter != size) SetGridDiameter(size);
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Thickness:");
                                            EditorGUIUtility.labelWidth = 0;
                                            GUILayout.FlexibleSpace();
                                            float girth = EditorGUILayout.FloatField(settings.thickness,
                                                                                     GUILayout.Width(90));
                                            if (settings.thickness != girth) SetGridThickness(girth);
                                        } break;
                                    case GridSettingsPage.Options:
                                        GUIStyle style = new(GUI.skin.button) { margin = new() };
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Ignore zTest:");
                                            GUILayout.FlexibleSpace();
                                            GUIUtils.OnOffButton(settings.ignoreZTest, out bool ignoreZTest,
                                                                 style, GUILayout.Width(60));
                                            if (ignoreZTest != settings.ignoreZTest) SetIgnoreZTest(ignoreZTest);
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Follow Camera:");
                                            GUILayout.FlexibleSpace();
                                            GUIUtils.OnOffButton(settings.followCamera, out settings.followCamera,
                                                                 style, GUILayout.Width(60));
                                        } break;
                                    case GridSettingsPage.Colors:
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Base:", GUILayout.Width(35));
                                            GUILayout.FlexibleSpace();
                                            Color baseColor = EditorGUILayout.ColorField(settings.baseColor,
                                                                                            GUILayout.Width(125));
                                            if (settings.baseColor != baseColor) SetGridColor(baseColor);
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Hint:", GUILayout.Width(35));
                                            GUILayout.FlexibleSpace();
                                            settings.hintColor = EditorGUILayout.ColorField(settings.hintColor,
                                                                                            GUILayout.Width(125));
                                        } break;
                                } if (changeScope.changed) EditorUtility.SetDirty(settings);
                            }
                        } else {
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Orientation:");
                                GUILayout.FlexibleSpace();
                                GUIStyle style = new(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter };
                                GridOrientation newOrientation = (GridOrientation) EditorGUILayout.EnumPopup(
                                                                    orientation, style, GUILayout.Width(80));
                                if (newOrientation != orientation) {
                                    SetGridOrientation(newOrientation);
                                }
                            } using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Toggle Grid:");
                                GUILayout.FlexibleSpace();
                                GUIUtils.OnOffButton(gridQuad.gameObject.activeSelf,
                                                     out bool enabled, GUILayout.Width(80));
                                if (gridQuad.gameObject.activeSelf != enabled) ToggleQuad(enabled);
                            }
                        }
                    }
                }
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
            }; this.orientation = orientation;
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
            settings.diameter = size;
        }

        private void SetGridThickness(float girth) {
            MaterialPropertyBlock mpb = new();
            gridQuad.Renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(GRID_GIRTH, girth);
            gridQuad.Renderer.SetPropertyBlock(mpb);
            settings.thickness = girth;
        }

        private void SetIgnoreZTest(bool ignoreZTest) {
            gridQuad.Renderer.sharedMaterial = ignoreZTest ? settings.ignoreZMaterial
                                                           : settings.baseMaterial;
            settings.ignoreZTest = ignoreZTest;
        }

        private void SetGridColor(Color color) {
            MaterialPropertyBlock mpb = new();
            gridQuad.Renderer.GetPropertyBlock(mpb);
            mpb.SetColor(GRID_COLOR, color);
            gridQuad.Renderer.SetPropertyBlock(mpb);
            settings.baseColor = color;
        }

        private void ResetWindowProperties() {
            showSettings = false;
            settingsPage = 0;
        }
    }
}