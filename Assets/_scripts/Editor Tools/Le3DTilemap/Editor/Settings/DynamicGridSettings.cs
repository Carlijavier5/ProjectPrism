using UnityEngine;

namespace Le3DTilemap {

    public enum GridSettingsPage { Size, View, Colors }
    public enum GridOrientation { XZ = 0, XY = 1, YZ = 2 }
    public enum GridInputMode { None = 0, Move, Turn }

    public class DynamicGridSettings : ScriptableObject {

        [HideInInspector] public SceneViewWindowSettings sceneGUI;

        public GameObject quadPrefab;
        public Material baseMaterial, ignoreZMaterial;

        public int diameter = 60;
        public float thickness = 0.01f;

        public bool followCamera = true;
        public bool ignoreZTest = false;

        public Color baseColor = new Vector4(0.8f, 0.8f, 0.8f, 0.4f);
        public Color hintColor;
    }
}