using UnityEngine;

namespace Le3DTilemap {

    public enum DrawDistributionScope { None, Selection, Colliders, Tilespace }
    public enum DrawDistributionMode { Bounds, Full };

    public class TileColliderToolSettings : ScriptableObject {
        [HideInInspector] public SceneViewWindowSettings sceneGUI;
        public DrawDistributionScope drawDistributionScope;
        public DrawDistributionMode drawDistributionMode;
    }
}