using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    [EditorTool("Tile Pivot Tool", typeof(TileInfo))]
    public partial class TilePivotTool : EditorTool {

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("d_ToolHandlePivot"));
        } public override GUIContent toolbarIcon => ToolIcon;

        private DynamicGridSettings settings;
        private DynamicGridQuad gridQuad;
        private Plane plane;
        private int depth;
        private Texture2D iconSearch, iconPlus, iconGrip,
                          iconSettings;

        public override void OnActivated() {
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } if (settings is not null) {
                InitializeLocalGrid();
            } depth = 0;
            LoadIcons();
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) return;
            if (settings == null) {
                SceneViewUtils.DrawMissingSettingsPrompt(ref settings,
                                        "Missing Dynamic Grid Settings",
                                        "New Dynamic Grid Settings",
                                        iconSearch, iconPlus);
                return;
            } DrawSceneViewWindowHeader();
        }

        private void InitializeLocalGrid() {
            gridQuad = FindAnyObjectByType<DynamicGridQuad>(FindObjectsInactive.Include);
            if (gridQuad == null) {
                gridQuad = Instantiate(settings.gridPrefab);
            } gridQuad.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInEditor;
        }

        public override void OnWillBeDeactivated() {
            settings = null;
            DestroyImmediate(gridQuad);
            Resources.UnloadUnusedAssets();
        }

        private void LoadIcons() {
            EditorUtils.LoadIcon(ref iconSearch, EditorUtils.ICON_SEARCH);
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
            EditorUtils.LoadIcon(ref iconGrip, EditorUtils.ICON_HGRIP);
            EditorUtils.LoadIcon(ref iconSettings, EditorUtils.ICON_SETTINGS);
        }
    }
}