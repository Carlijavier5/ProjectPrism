using UnityEngine;

namespace Le3DTilemap {
    public class TileRotationToolSettings : ScriptableObject {
        [HideInInspector] public SceneViewWindowSettings sceneGUI;
        public bool rotatesColliders = true, rotatesMesh = true;
    }
}