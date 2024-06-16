using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapTool {

        private void DrawSceneViewWindowHeader() {
            Handles.BeginGUI();
            Rect rect = GUILayout.Window(2, settings.sceneGUI.rect, DrawSceneViewWindow,
                                         "", EditorStyles.textArea);
            if (!rect.Equals(settings.sceneGUI.rect)) {
                settings.sceneGUI.rect = rect;
                EditorUtility.SetDirty(settings );
            } Handles.EndGUI();
        }

        private void DrawSceneViewWindow(int controlID) {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(170), GUILayout.MinHeight(0))) {
                using (new EditorGUILayout.HorizontalScope(UIStyles.WindowBox)) {
                    GUILayout.Space(8);
                    Rect rect = GUILayoutUtility.GetRect(18, 14);
                    bool mouseInRect = rect.Contains(Event.current.mousePosition);
                    GUI.color = mouseInRect ? Color.white : new Vector4(1f, 1f, 1f, 0.6f);
                    GUI.Label(rect, iconGrip);
                    GUI.color = Color.white;
                    GUILayout.Label("Tilemap Settings", UIStyles.CenteredLabelBold);
                    if (mouseInRect && Event.current.type == EventType.MouseDown) {
                        settings.sceneGUI.hideContents = !settings.sceneGUI.hideContents;
                        EditorUtility.SetDirty(settings);
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
                            Undo.RegisterCreatedObjectUndo(hookGO, "Undo Scene Hook Creation");
                            EditorApplication.RepaintHierarchyWindow();
                        }
                    }
                } else if (sceneHook.GridData == null) {
                    GUIUtils.DrawCustomHelpBox("Assign A Level Grid Asset", iconInfo);
                    LevelGridData nData = EditorGUILayout.ObjectField(sceneHook.GridData, typeof(LevelGridData), false) as LevelGridData;
                    if (!(sceneHook.GridData == nData)) sceneHook.EDITOR_SetGrid(nData);
                    if (GUILayout.Button("Generate Level Grid")) {
                        LevelGridData newAsset = AssetUtils.CreateAsset<LevelGridData>("New Level Grid Data", "Level Grid");
                        if (newAsset) sceneHook.EDITOR_SetGrid(newAsset);
                        GUIUtility.ExitGUI();
                    } 
                } else {
                    if (GUILayout.Button("Reset Settings")) {
                        settings = null;
                        /*
                        Debug.Log($"Single: {SceneUtils.SelectedOutlineSingleColor}");
                        Debug.Log($"Children: {SceneUtils.SelectedOutlineChildrenColor}");
                        SceneUtils.SelectedOutlineSingleColor = new Vector4(0.5f, 0.5f, 0.25f, 1f);
                        SceneUtils.SelectedOutlineChildrenColor = new Vector4(0, 0, 0, 0);*/
                    } gridHeight = EditorGUILayout.IntField(gridHeight);
                    tileObject = EditorGUILayout.ObjectField(tileObject, typeof(GameObject), false) as GameObject;
                }
            }
        }
    }
}