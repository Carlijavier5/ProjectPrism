using UnityEngine;
using UnityEditor;

namespace Le3DTilemap {
    public class Le3DTilemapWindowSettings : ScriptableObject {

        public TilePalette3D activePalette;
        public PaletteEditMode editMode;

        public int cardSize = 60;
        public int cardSizeMultiplier = 1;

        public bool launchToolOnSelect;
    }

    [CustomEditor(typeof(Le3DTilemapWindowSettings))]
    public class Le3DTilemapWindowPrefsEditor : Editor {

        public override void OnInspectorGUI() {
            EditorGUILayout.HelpBox("This Asset contains Dynamic Data used in an Editor Window;",
                                    MessageType.Info);
        }
    }
}