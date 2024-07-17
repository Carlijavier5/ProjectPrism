using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {
    public enum ToolMode { Select, MSelect, Paint, Erase, Fill, Clear, Pick }

    [EditorTool("Le3DTilemap")]
    public partial class Le3DTilemapTool : GridTool {

        public static int InstanceID { get; private set; }

        private Le3DTilemapSettings settings;

        private LevelGridHook sceneHook;

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("d_Tile Icon"));
        } public override GUIContent toolbarIcon => ToolIcon;

        private Texture2D iconWarning, iconSelect, iconMSelect, 
                          iconPaint, iconErase, iconFill,
                          iconClear, iconPick, iconTransform,
                          iconDisplace, iconRotate, iconPalette;

        private TileData selectedTile;
        public TileData SelectedTile {
            get => selectedTile;
            set {
                if (value != null) {
                    if (toolMode != ToolMode.Paint
                    &&  toolMode != ToolMode.Fill) {
                        SetToolMode(ToolMode.Paint);
                    } int size = value.Info.Colliders.Count;
                    highlightCenters = new Vector3[size];
                } selectedTile = value;
            } 
        }

        [Shortcut("Le3DTilemap Tool", KeyCode.Tab)]
        public static void Activate() => ToolManager.SetActiveTool<Le3DTilemapTool>();

        public override void OnActivated() {
            base.OnActivated();
            InstanceID = GetInstanceID();
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } allowDirectGridMode = false;
            LoadIcons();

            LoadPhysicsScene(out physicsSpace);
            Le3DTilemapWindow.Launch(this);
            sceneHook = FindAnyObjectByType<LevelGridHook>();
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) { return; }
            if (HasNullSettings(ref settings, sceneView)) return;
            DrawGridWindow(sceneView, true);
            DrawSceneViewWindowHeader(sceneView);
            HighlightModeContent(sceneView);
        }

        protected override void OnSceneGUI(SceneView sceneView) {
            base.OnSceneGUI(sceneView);
            if (InvalidSceneGUI(settings, sceneView)) return;
            if (MouseOnGUI(settings.sceneGUI.rect)) {
                ClearHint();
                return;
            } DrawCustomCursor(sceneView);
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            DoGridInput(sceneView);
            DoToolInput(sceneView);
        }

        /*
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
        }*/

        public override void OnWillBeDeactivated() {
            base.OnWillBeDeactivated();
            settings = null;
            Resources.UnloadUnusedAssets();
        }

        protected override void LoadIcons() {
            base.LoadIcons();
            EditorUtils.LoadIcon(ref iconWarning, EditorUtils.ICON_WARNING);
            EditorUtils.LoadIcon(ref iconSelect, "_Select");
            EditorUtils.LoadIcon(ref iconMSelect, "_MultiSelect");
            EditorUtils.LoadIcon(ref iconPaint, "_Paint");
            EditorUtils.LoadIcon(ref iconErase, "_Erase");
            EditorUtils.LoadIcon(ref iconFill, "_Fill");
            EditorUtils.LoadIcon(ref iconClear, "_Clear");
            EditorUtils.LoadIcon(ref iconPick, "_Pick");
            EditorUtils.LoadIcon(ref iconTransform, "d_RectTransform Icon");
            EditorUtils.LoadIcon(ref iconDisplace, "d_UnityEditor.Graphs.AnimatorControllerTool");
            EditorUtils.LoadIcon(ref iconRotate, "d_preAudioLoopOff");
            EditorUtils.LoadIcon(ref iconPalette, "_TilePalette");
        }
    }
}