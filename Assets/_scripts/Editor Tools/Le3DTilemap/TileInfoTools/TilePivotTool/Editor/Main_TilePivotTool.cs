using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using CJUtils;

namespace Le3DTilemap {

    [EditorTool("Tile Pivot Tool", typeof(TileInfo))]
    public partial class TilePivotTool : EditorTool {

        private TileInfo Info => target as TileInfo;

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("d_ToolHandlePivot"));
        } public override GUIContent toolbarIcon => ToolIcon;

        private DynamicGridSettings settings;
        private DynamicGridQuad gridQuad;
        private Texture2D iconSearch, iconPlus, iconGrip,
                          iconSettings, iconMove, iconTurn;

        public override void OnActivated() {
            SceneView.duringSceneGui += OnSceneGUI;
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } if (settings is not null) {
                InitializeLocalGrid();
            } ResetWindowProperties();
            ResetSelection();
            depth = 0;
            LoadIcons();
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) return;
            if (settings == null) {
                SceneViewUtils.DrawMissingSettingsPrompt(ref settings, sceneView,
                                        "Missing Dynamic Grid Settings",
                                        "New Dynamic Grid Settings",
                                        iconSearch, iconPlus);
                return;
            } HighlightHintTile();
            Rect firstWindowRect = DrawSceneViewWindowHeader();
            DrawHintWindow(firstWindowRect);
        }

        private void OnSceneGUI(SceneView sceneView) {
            bool mouseOnGUI = settings.sceneGUI.rect
                              .Contains(Event.current.mousePosition);
            if (ToolManager.activeToolType != GetType()
                || !sceneView.hasFocus || settings == null
                || gridQuad == null || mouseOnGUI) return;
            DoInputOverrides();
            DoSelectionInput();
            DoScrollInput(sceneView);
            UpdateGridPos(sceneView);
        }

        private void InitializeLocalGrid() {
            gridQuad = FindAnyObjectByType<DynamicGridQuad>(FindObjectsInactive.Include);
            if (gridQuad == null) {
                GameObject go = PrefabUtility.InstantiatePrefab(settings.quadPrefab) as GameObject;
                if (!go.TryGetComponent(out gridQuad)) DestroyImmediate(go);
            } if (gridQuad == null) {
                Debug.LogWarning("Missing Dynamic Grid Quad");
                return;
            } StageUtility.PlaceGameObjectInCurrentStage(gridQuad.gameObject);
            gridQuad.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInEditor;
            SetGridOrientation(GridOrientation.XZ);
            ToggleQuad(true);
            SetGridDiameter(settings.diameter);
            SetGridThickness(settings.thickness);
            SetIgnoreZTest(settings.ignoreZTest);
            SetGridColor(settings.baseColor);
            UpdateGridDepth();
        }

        public override void OnWillBeDeactivated() {
            SceneView.duringSceneGui -= OnSceneGUI;
            settings = null;
            DestroyImmediate(gridQuad.gameObject);
            Resources.UnloadUnusedAssets();
        }

        private void LoadIcons() {
            EditorUtils.LoadIcon(ref iconSearch, EditorUtils.ICON_SEARCH);
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
            EditorUtils.LoadIcon(ref iconGrip, EditorUtils.ICON_VGRIP);
            EditorUtils.LoadIcon(ref iconSettings, EditorUtils.ICON_SETTINGS);
            EditorUtils.LoadIcon(ref iconMove, "_GridMove");
            EditorUtils.LoadIcon(ref iconTurn, "_GridTurn");
        }
    }
}