using UnityEngine;

namespace Le3DTilemap {

    public enum GridSettingsPage { View, Options, Colors }
    public enum GridOrientation { XZ = 0, XY = 1, YZ = 2 }

    public class DynamicGridSettings : ScriptableObject {

        [HideInInspector] public SceneViewWindowSettings sceneGUI;

        public GameObject quadPrefab;
        public int size;

        public bool followCamera;
        public bool ignoreZTest;

        public Color baseColor;
        public Color hintColor;
    }
}