using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    [EditorTool("Tile Rotation Tool", typeof(TileInfo))]
    public partial class TileRotationTool : GridTool {

        private TileRotationToolSettings settings;

        private TileInfo Info => target as TileInfo;

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("_TileRotationTool"));
        } public override GUIContent toolbarIcon => ToolIcon;

        public override void OnActivated() {
            base.OnActivated();
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } allowDirectGridMode = false;
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) return;
            if (HasNullSettings(ref settings, sceneView)) return;
            DrawGridWindow(sceneView, true);
            DrawSceneViewWindowHeader(sceneView);
            HighlightPivots();
        }

        protected override void OnSceneGUI(SceneView sceneView) {
            if (InvalidSceneGUI(settings, sceneView, GetType())
                || gridSettings.sceneGUI.rect
                .Contains(Event.current.mousePosition)
                || settings.sceneGUI.rect
                .Contains(Event.current.mousePosition)) {
                return;
            } DoInputOverrides();
            DoScrollInput(sceneView);
            DoToolInput();
        }

        public override void OnWillBeDeactivated() {
            base.OnWillBeDeactivated();
            settings = null;
            Resources.UnloadUnusedAssets();
        }
    }
}