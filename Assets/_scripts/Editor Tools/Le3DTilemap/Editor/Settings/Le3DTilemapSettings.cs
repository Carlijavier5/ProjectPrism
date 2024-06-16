using UnityEngine;

namespace Le3DTilemap {

    public class Le3DTilemapSettings : ScriptableObject {
        [HideInInspector] public SceneViewWindowSettings sceneGUI;
        public Color defaultOutline, validOutline, invalidOutline;
        public Color gridColor;
    }
}