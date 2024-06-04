using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapTool {

        private void DrawMissingSettingsPrompt(SceneView sceneView) {
            Handles.BeginGUI();
            sceneView.sceneViewState.alwaysRefresh = true;
            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(1))) {
                        GUIUtils.WindowBoxLabel("Missing Tool Settings");
                        using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Search Again", EditorUtils.FetchIcon("Search Icon")),
                                                 GUILayout.Width(150), GUILayout.Height(20))) {
                                AssetUtils.TryRetrieveAsset(out settings);
                            } GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Create Settings", EditorUtils.FetchIcon("Toolbar Plus")),
                                                 GUILayout.Width(150), GUILayout.Height(20))) {
                                settings = AssetUtils.CreateAsset<Le3DTilemapSettings>("New Le3DTilemap Settings", "Le3DTilemapSettings");
                                GUIUtility.ExitGUI();
                            } GUILayout.FlexibleSpace();
                        }
                    } GUILayout.FlexibleSpace();
                } GUILayout.FlexibleSpace();
            } Handles.EndGUI();
        }
    }
}