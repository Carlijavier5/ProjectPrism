using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {

    public partial class TileColliderTool {
        private enum ToolMode { Select = 0, Scale, Move, Pivot }
        private ToolMode toolMode;

        private bool showSettings;
        private bool willHide;

        private string[] names;

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
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(200), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    GUILayout.Space(8);
                    Rect rect = GUILayoutUtility.GetRect(30, 14);
                    bool mouseInRect = rect.Contains(Event.current.mousePosition);
                    GUI.color = mouseInRect ? Color.white : new Vector4(1f, 1f, 1f, 0.6f);
                    GUI.Label(rect, iconGrip);
                    GUI.color = Color.white;
                    GUILayout.Space(8);
                    GUILayout.Label("Collider Editor Tool", UIStyles.CenteredLabelBold);
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
                    } GUILayout.Space(4);
                    if (mouseInRect && Event.current.type == EventType.MouseDown
                        && Event.current.button == 0) {
                        willHide = true;
                    } else if (Event.current.type == EventType.MouseDrag) {
                        willHide = false;
                    } else if (Event.current.type == EventType.MouseUp) {
                        if (mouseInRect && willHide) {
                            settings.sceneGUI.hideContents = !settings.sceneGUI.hideContents;
                            settings.sceneGUI.rect = new Rect(settings.sceneGUI.rect) {
                                y = settings.sceneGUI.rect.y
                                + settings.sceneGUI.rect.height * 0.75f
                                * (settings.sceneGUI.hideContents ? 1 : -1),
                            }; EditorUtility.SetDirty(settings);
                        } willHide = false;
                    }
                } if (!settings.sceneGUI.hideContents) DrawContent();
            } GUI.DragWindow();
        }

        private void DrawContent() {
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox)) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                        if (showSettings) {
                            if (GUILayout.Button(new GUIContent(" Leave Settings ", iconSettings))) {
                                showSettings = false;
                            }
                        } else {
                            DrawToolModeButton(ToolMode.Select, new RectOffset() { right = 0 });
                            GUI.enabled = Info.SelectedCollider != null;
                            DrawToolModeButton(ToolMode.Scale, new RectOffset() { right = 0, left = 0 });
                            DrawToolModeButton(ToolMode.Move, new RectOffset() { left = 0 });
                            GUI.enabled = true;
                        }
                    } GUILayout.FlexibleSpace();
                } GUIStyle paddedBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 6, 6) };
                using (new EditorGUILayout.VerticalScope(paddedBox, GUILayout.Height(50))) {
                    if (showSettings) {
                        using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                            EditorGUIUtility.labelWidth = 0;
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Highlight Scope:", GUILayout.Width(97));
                                GUILayout.FlexibleSpace();
                                settings.drawDistributionScope = (DrawDistributionScope) EditorGUILayout.EnumPopup(
                                                                 settings.drawDistributionScope, GUILayout.Width(75));
                            }
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Highlight Mode:", GUILayout.Width(97));
                                GUILayout.FlexibleSpace();
                                settings.drawDistributionMode = (DrawDistributionMode) EditorGUILayout.EnumPopup(
                                                                settings.drawDistributionMode, GUILayout.Width(75));
                            } if (changeScope.changed) EditorUtility.SetDirty(settings);
                        }
                    } else {
                        if (Info.SelectedCollider == null) {
                            GUIUtils.DrawScopeCenteredText("No Collider Selected;");
                        } else {
                            switch (toolMode) {
                                case ToolMode.Select:
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                                            GUILayout.Label("Letter Name:");
                                            int collName = Info.SelectedCollider.name - 65;
                                            GUIStyle style = new(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter };
                                            collName = EditorGUILayout.Popup(collName, names, style, GUILayout.Width(80));
                                            if (changeScope.changed) {
                                                UndoUtils.RecordScopeUndo(target, "Change Letter Tag (Tile Collider)");
                                                Info.SelectedCollider.name = (char) (65 + collName);
                                                EditorUtility.SetDirty(Info);
                                            }
                                        }
                                    } using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Component:");
                                        GUI.enabled = false;
                                        EditorGUILayout.ObjectField(Info.SelectedCollider.collider,
                                                                    typeof(BoxCollider), false, GUILayout.Width(80));
                                        GUI.enabled = true;
                                    } break;
                                case ToolMode.Scale:
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Size:", GUILayout.Width(45));
                                        GUILayout.FlexibleSpace();
                                        EditorGUIUtility.labelWidth = 0;
                                        TileUtils.TileSizeField(Info.SelectedCollider, GUILayout.Width(125));
                                    } using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.Label("Center:", GUILayout.Width(45));
                                        EditorGUIUtility.labelWidth = 0;
                                        GUILayout.FlexibleSpace();
                                        GUI.enabled = false;
                                        EditorGUILayout.Vector3Field("", Info.SelectedCollider.Center,
                                                                        GUILayout.Width(125));
                                        GUI.enabled = true;
                                    } break;
                                case ToolMode.Move:
                                case ToolMode.Pivot:
                                    using (new EditorGUILayout.VerticalScope()) {
                                        using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Select Pivot Tile:", GUILayout.Width(100));
                                            GUILayout.FlexibleSpace();
                                            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(67));
                                            GUI.backgroundColor = toolMode == ToolMode.Pivot ? UIColors.DefinedBlue
                                                                                                : Color.white;
                                            if (GUI.Button(new(rect) { y = rect.y - 1 }, 
                                                            new GUIContent(" Pick", iconPivot))) {
                                                toolMode = toolMode == ToolMode.Pivot ? ToolMode.Move : ToolMode.Pivot;
                                            } GUI.backgroundColor = Color.white;
                                        } using (new EditorGUILayout.HorizontalScope()) {
                                            GUILayout.Label("Pivot:", GUILayout.Width(45));
                                            GUILayout.FlexibleSpace();
                                            Info.SelectedCollider.Pivot = EditorGUILayout.Vector3IntField("",
                                                                            Info.SelectedCollider.Pivot, GUILayout.Width(125));
                                        }
                                    } break;
                            }
                        }
                    }
                }
            }
        }

        private void DrawToolModeButton(ToolMode toolMode, RectOffset margin) {
            GUI.backgroundColor = this.toolMode == toolMode ? UIColors.DefinedBlue : Color.white;
            GUIStyle style = new(GUI.skin.button) { margin = margin };
            if (GUILayout.Button(toolMode switch { ToolMode.Scale => iconScale,
                                                   ToolMode.Move => iconMove,
                                                   _ => iconSelect }, style, GUILayout.Width(60))) {
                this.toolMode = toolMode;
            } GUI.backgroundColor = Color.white;
        }

        private void ResetWindowProperties() {
            names = new string[26];
            for (int i = 0; i < 26; i++) {
                names[i] = ((char) (65 + i)).ToString();
            } showSettings = false;
            toolMode = 0;
        }
    }
}