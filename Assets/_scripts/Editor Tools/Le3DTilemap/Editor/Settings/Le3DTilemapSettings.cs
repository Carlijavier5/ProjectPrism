using UnityEngine;

namespace Le3DTilemap {

    public class Le3DTilemapSettings : ScriptableObject {
        [System.Serializable] public struct SceneViewWindowSettings {
            public bool hideContents;
            public Rect rect;
        } [HideInInspector] public SceneViewWindowSettings sceneGUI;
        public Color defaultOutline, validOutline, invalidOutline;
    }
}
