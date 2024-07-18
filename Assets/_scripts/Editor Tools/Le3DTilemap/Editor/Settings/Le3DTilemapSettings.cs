using UnityEngine;

namespace Le3DTilemap {

    public class Le3DTilemapSettings : ScriptableObject {
        [HideInInspector] public SceneViewWindowSettings sceneGUI;
        public Color defaultOutline, validOutline, invalidOutline;
        public Color gridColor;

        public bool focusInspectorOnSelect = true;

        public bool includePartialSelectionsMSelect = true;

        public bool focusPaletteOnEnterPaint = true;
        public bool rotateTileToSurface = false;

        public bool focusPaletteOnEnterFill = true;

        public bool includePartialSelectionsClear = true;
    }
}