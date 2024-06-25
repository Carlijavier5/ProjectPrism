using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    [EditorTool("Tile Collider Tool", typeof(TileInfo))]
    public partial class TileColliderTool : EditorTool {

        private TileColliderToolSettings settings;
        private TileInfo Info => target as TileInfo;

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("_TileColliderInfo"));
        } public override GUIContent toolbarIcon => ToolIcon;

        private int activeID;

        private Texture2D iconSearch, iconPlus, iconGrip,
                          iconSelect, iconScale, iconMove,
                          iconSettings, iconPivot;

        public override void OnActivated() {
            SceneView.duringSceneGui += OnSceneGUI;
            Undo.undoRedoPerformed += UpdateHandles;
            LoadPhysicsScene(out physicsSpace);
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } Info.OnSelectionChanged += Info_OnSelectionChanged;
            names = new string[26];
            for (int i = 0; i < 26; i++) {
                names[i] = ((char) (65 + i)).ToString();
            } showSettings = false;
            activeHandles = null;
            toolMode = 0;
            Info_OnSelectionChanged();
            ResetSelection();
            LoadIcons();
        }

        private void Info_OnSelectionChanged() {
            if (Info.SelectedCollider == null) {
                activeHandles = null;
                toolMode = ToolMode.Select;
            } else if (activeHandles == null
                       || activeHandles.collider != Info.SelectedCollider) {
                activeHandles = new (Info.transform, Info.SelectedCollider);
            } ResetPivot();
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) { return; }
            if (settings == null) {
                SceneViewUtils.DrawMissingSettingsPrompt(ref settings, sceneView,
                                                         "Missing Tool Settings",
                                                         "New Settings",
                                                         iconSearch, iconPlus);
                return;
            } GridUtils.DrawGrid(GridAxis.XZ, sceneView, 0);
            DrawSubtoolContent();
            DrawSceneViewWindowHeader();
        }

        private void DrawSubtoolContent() {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            switch (toolMode) {
                case ToolMode.Select:
                    HighlightHintCollider();
                    break;
                case ToolMode.Move:
                case ToolMode.Scale:
                    if (activeHandles != null) {
                        activeHandles.DoHandles(ref activeID, toolMode);
                    } break;
                case ToolMode.Pivot:
                    HighlightPivotTarget();
                    break;
            } DrawTileDistribution();
            HighlightSelectedCollider();
        }

        void OnSceneGUI(SceneView sceneView) {
            if (ToolManager.activeToolType != GetType()
                || !sceneView.hasFocus || settings == null
                || settings.sceneGUI.rect
                   .Contains(Event.current.mousePosition)) return;
            switch (toolMode) {
                case ToolMode.Select:
                    DoSelectionInput();
                    break;
                case ToolMode.Pivot:
                    DoLocalPivotSelector();
                    break;
            }
        }

        private void ReadToolShortcuts() {
            
        }

        public override void OnWillBeDeactivated() {
            if (Info) {
                Info.ToggleSelectedIndex(Info.SelectedIndex);
                Info.OnSelectionChanged -= Info_OnSelectionChanged;
            } SceneView.duringSceneGui -= OnSceneGUI;
            Undo.undoRedoPerformed -= UpdateHandles;
            Resources.UnloadUnusedAssets();
        }

        private void LoadIcons() {
            EditorUtils.LoadIcon(ref iconSearch, EditorUtils.ICON_SEARCH);
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
            EditorUtils.LoadIcon(ref iconGrip, EditorUtils.ICON_VGRIP);
            EditorUtils.LoadIcon(ref iconSelect, "d_Grid.Default");
            EditorUtils.LoadIcon(ref iconScale, "d_AvatarPivot");
            EditorUtils.LoadIcon(ref iconMove, "d_ToolHandleCenter");
            EditorUtils.LoadIcon(ref iconSettings, EditorUtils.ICON_SETTINGS);
            EditorUtils.LoadIcon(ref iconPivot, "d_ToolHandleLocal");
        }
    }
}