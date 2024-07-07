using UnityEngine;

namespace Le3DTilemap {

    public enum GridSettingsPage { Shortcuts = 0, Raycasts = 1,
                                   Size = 2, View = 3, Colors = 4 }
    public enum GridOrientation { XZ = 0, XY = 1, YZ = 2 }
    public enum GridInputMode { None = 0, Move, Turn }

    public class DynamicGridSettings : ScriptableObject {

        [HideInInspector] public SceneViewWindowSettings sceneGUI;
        [HideInInspector] public Rect hintRect;

        public GameObject quadPrefab;
        public Material baseMaterial, ignoreZMaterial;

        public int raycastDistance = 19;
        public double raycastCDMult = 1;

        public int diameter = 60;
        public float thickness = 0.01f;

        public bool followCamera = true;
        public bool ignoreZTest = false;

        public Color baseColor = new Vector4(0.8f, 0.8f, 0.8f, 0.4f);
        public Color hintColor;
    }
}

public class Player : MonoBehaviour {
    [SerializeField] private PlayerData data;
    [SerializeField] private P struc;

    private void Awake() {

    }
}

public class PlayerData : ScriptableObject {
    [SerializeField] private P struc;
    public P Struc;
}

[System.Serializable]
public class P {
    float speed;
    float position;

    public P(P oldP) {
        this.speed = oldP.speed;
    }
}