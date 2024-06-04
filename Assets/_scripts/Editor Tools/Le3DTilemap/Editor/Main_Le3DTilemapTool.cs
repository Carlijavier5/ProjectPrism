using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {
    public enum ToolMode { Move, Place, Pick }

    [EditorTool("Tilemap")]
    public partial class Le3DTilemapTool : EditorTool {

        public static event System.Action OnToolActivated;

        [SerializeField] private Le3DTilemapSettings settings;
        public Le3DTilemapSettings Settings => settings;

        private Le3DTilemapWindow window;
        private LevelGridHook sceneHook;

        public override GUIContent toolbarIcon => EditorGUIUtility.IconContent("d_Tile Icon");
        private int gridHeight;
        private GameObject tileObject;

        [Shortcut("Le3DTilemap Tool", KeyCode.Tab)]
        public static void Activate() => ToolManager.SetActiveTool<Le3DTilemapTool>();

        public override void OnActivated() {
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } window = Le3DTilemapWindow.Launch(this);
            sceneHook = FindAnyObjectByType<LevelGridHook>();
            OnToolActivated?.Invoke();
            OnToolActivated = null;
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) { return; }
            if (settings == null) {
                DrawMissingSettingsPrompt(sceneView);
                return;
            } if (sceneHook == null) {
                DrawSceneViewWindowHeader();
                return;
            } DrawGrid(gridHeight, 1000);
            DrawSceneViewWindowHeader();
        }

        public void InputHandling(SceneView sceneView) {
            sceneView.sceneViewState.alwaysRefresh = Event.current.type == EventType.MouseMove
                                         || Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftShift) GUIUtility.hotControl = 1;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            plane.Raycast(ray, out float enter);
            Vector3 hitPoint = ray.GetPoint(enter);
            ///Handles.DrawSolidDisc(hitPoint, Vector3.up, 0.25f);
            if (Event.current.button == 0 && (Event.current.type == EventType.MouseDown
                || Event.current.type == EventType.MouseDrag)) {
                if (tileObject != null && !Physics.Raycast(ray, out RaycastHit hit, 1000f)) {
                    ///Instantiate(tileObject, GridUtils.WorldToCell(hitPoint), Quaternion.identity);
                }
            } else { }///Handles.DrawWireCube(GridUtils.WorldToCell(hitPoint), Vector3.one * 0.8f);
        }

        public override void OnWillBeDeactivated() {
            
        }

        private void DrawGrid(int y, int distance) {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
            float offset = 0.5f;
            for (int x = -distance; x < distance; x++) {
                Handles.DrawLine(new(x - offset, y, -distance), new(x - offset, y, distance));
            } for (int z = -distance; z < distance; z++) {
                Handles.DrawLine(new(-distance, y, z - offset), new(distance, y, z - offset));
            } Handles.zTest = UnityEngine.Rendering.CompareFunction.GreaterEqual;
        }
    }
}