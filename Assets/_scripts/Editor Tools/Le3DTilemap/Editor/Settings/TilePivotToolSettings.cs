using UnityEngine;

namespace Le3DTilemap {
    public class TilePivotToolSettings : ScriptableObject {
        [HideInInspector] public SceneViewWindowSettings sceneGUI;
        public int raycastDistance = 20;
    }
}
