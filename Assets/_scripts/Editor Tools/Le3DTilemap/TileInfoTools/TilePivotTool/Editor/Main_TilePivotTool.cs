using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    [EditorTool("Tile Pivot Tool", typeof(TileInfo))]
    public partial class TilePivotTool : GridTool {

        private TilePivotToolSettings settings;
        private TileInfo Info => target as TileInfo;

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("d_ToolHandlePivot"));
        } public override GUIContent toolbarIcon => ToolIcon;

        public override void OnActivated() {
            base.OnActivated();
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } ResetSelection();
            allowDirectGridMode = true;
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) return;
            if (HasNullSettings(ref settings, sceneView)) return;
            DrawGridWindow(sceneView, true);
            DrawSceneViewWindowHeader(sceneView);
            HighlightHintTile();
        }

        protected override void OnSceneGUI(SceneView sceneView) {
            if (InvalidSceneGUI(settings, sceneView)) return;
            if (MouseOnGUI(settings.sceneGUI.rect)) {
                hasHint = false;
                return;
            } DoGridInput(sceneView);
            DoSelectionInput();
        }

        public override void OnWillBeDeactivated() {
            base.OnWillBeDeactivated();
            settings = null;
            Resources.UnloadUnusedAssets();
        }
    }
}