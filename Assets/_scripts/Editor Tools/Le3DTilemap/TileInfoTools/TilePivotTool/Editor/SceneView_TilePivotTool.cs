using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class TilePivotTool {

        private bool willHide;
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
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(200), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    GUILayout.Space(8);
                    Rect rect = GUILayoutUtility.GetRect(30, 14);
                    bool mouseInRect = rect.Contains(Event.current.mousePosition);
                    GUI.color = mouseInRect ? Color.white : new Vector4(1f, 1f, 1f, 0.6f);
                    GUI.Label(rect, iconGrip);
                    GUI.color = Color.white;
                    GUILayout.Space(20);
                    GUILayout.Label("Pivot Editor Tool", UIStyles.CenteredLabelBold);
                    GUILayout.Space(31);
                    if (mouseInRect && Event.current.type == EventType.MouseDown
                        && Event.current.button == 0) {
                        willHide = true;
                    } else if (Event.current.type == EventType.MouseDrag) {
                        willHide = false;
                    } else if (Event.current.type == EventType.MouseUp) {
                        if (mouseInRect && willHide) {
                            bool hide = settings.sceneGUI.hideContents = !settings.sceneGUI.hideContents;
                            if (settings.sceneGUI.hideContents) {
                                heightDiff = settings.sceneGUI.rect.height * 0.63f;
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
                GUIStyle paddedBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 6, 6) };
                using (new EditorGUILayout.VerticalScope(paddedBox, GUILayout.Height(50))) {
                    using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                        GUIStyle lStyle = new(GUI.skin.label) { contentOffset = new Vector2(0, -1) };
                        GUIStyle style = new(GUI.skin.button) { margin = new() };
                        using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.Label("Move Colliders:", lStyle);
                            GUILayout.FlexibleSpace();
                            GUIUtils.OnOffButton(settings.movesColliders, out settings.movesColliders,
                                                 style, GUILayout.Width(65));
                        } using (new EditorGUILayout.HorizontalScope()) {
                            GUI.enabled = Info.MeshRoot != null;
                            GUILayout.Label("Move Mesh:", lStyle);
                            GUILayout.FlexibleSpace();
                            if (Info.MeshRoot != null) {
                                GUIUtils.OnOffButton(settings.movesMesh, out settings.movesMesh,
                                                     style, GUILayout.Width(65));
                            } else {
                                Rect rect = EditorGUILayout.GetControlRect(false, 19,
                                                                           UIStyles.TextBoxLabel,
                                                                           GUILayout.Width(65));
                                GUI.Label(rect, "Unassigned", UIStyles.TextBoxLabel);
                            } GUI.enabled = true;
                        } if (changeScope.changed) EditorUtility.SetDirty(settings);
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
    }
}