using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public static class SceneViewUtils {

        public static void DrawMissingSettingsPrompt<T>(ref T settings, string title, string createPrompt,
                                                        Texture2D iconSearch, Texture2D iconPlus)
                                                        where T : ScriptableObject {
            Handles.BeginGUI();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(1))) {
                GUIUtils.WindowBoxLabel(title);
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Search Again", iconSearch),
                                            GUILayout.Width(150), GUILayout.Height(20))) {
                        AssetUtils.TryRetrieveAsset(out settings);
                    } GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Create Settings", iconPlus),
                                            GUILayout.Width(150), GUILayout.Height(20))) {
                        settings = AssetUtils.CreateAsset<T>(createPrompt, 
                                                                typeof(T).Name);
                        GUIUtility.ExitGUI();
                    } GUILayout.FlexibleSpace();
                }
            }
        }

        public static void DrawMissingSettingsPrompt<T>(ref T settings, SceneView sceneView,
                                                        string title, string createPrompt,
                                                        Texture2D iconSearch, Texture2D iconPlus)
                                                        where T : ScriptableObject {
            sceneView.sceneViewState.alwaysRefresh = true;
            DrawMissingSettingsPrompt(ref settings, title, createPrompt, iconSearch, iconPlus);
        }
    }
}