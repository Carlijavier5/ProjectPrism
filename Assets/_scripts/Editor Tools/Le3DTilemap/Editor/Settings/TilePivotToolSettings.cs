using UnityEngine;

namespace Le3DTilemap {
    public class TilePivotToolSettings : ScriptableObject {
        [HideInInspector] public SceneViewWindowSettings sceneGUI;
        public bool movesColliders = true, movesMesh = true;
    }
}
