using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    [EditorTool("Tile Collider Tool", typeof(TileInfo))]
    public partial class TileColliderTool : GridTool {

        private TileColliderToolSettings settings;
        private TileInfo Info => target as TileInfo;

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("_TileColliderInfo"));
        } public override GUIContent toolbarIcon => ToolIcon;

        private int activeID;

        private Texture2D iconPivot, iconSelect, iconScale;

        public override void OnActivated() {
            base.OnActivated();
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } allowDirectGridMode = false;

            Undo.undoRedoPerformed += UpdateHandles;
            LoadPhysicsScene(out physicsSpace);

            Info.OnSelectionChanged += Info_OnSelectionChanged;
            ResetWindowProperties();

            activeHandles = null;
            Info_OnSelectionChanged();

            ResetSelection();
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
            if (HasNullSettings(ref settings, sceneView)) return;
            DrawSubtoolContent();
            DrawGridWindow(sceneView, false);
            DrawSceneViewWindowHeader(sceneView);
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

        protected override void OnSceneGUI(SceneView sceneView) {
            if (InvalidSceneGUI(settings, sceneView)) return;
            if (gridSettings.sceneGUI.rect
                .Contains(Event.current.mousePosition)
                || settings.sceneGUI.rect
                .Contains(Event.current.mousePosition)) {
                hintCollider = null;
                return;
            } DoInputOverrides();
            DoScrollInput(sceneView);
            switch (toolMode) {
                case ToolMode.Select:
                    DoSelectionInput();
                    break;
                case ToolMode.Pivot:
                    DoLocalPivotSelector();
                    break;
            }
        }

        public override void OnWillBeDeactivated() {
            base.OnWillBeDeactivated();
            if (Info) {
                Info.ToggleSelectedIndex(Info.SelectedIndex);
                Info.OnSelectionChanged -= Info_OnSelectionChanged;
            } Undo.undoRedoPerformed -= UpdateHandles;
            settings = null;
            Resources.UnloadUnusedAssets();
        }

        protected override void LoadIcons() {
            base.LoadIcons();
            EditorUtils.LoadIcon(ref iconSelect, "d_Grid.Default");
            EditorUtils.LoadIcon(ref iconScale, "d_AvatarPivot");
            EditorUtils.LoadIcon(ref iconPivot, "d_ToolHandleLocal");
        }
    }
}