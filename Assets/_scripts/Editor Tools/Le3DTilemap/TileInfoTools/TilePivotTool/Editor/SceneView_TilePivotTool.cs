using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class TilePivotTool {

        protected override void DrawHintContent(int controlID) {
            if (hasHint) {
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
        }
    }
}