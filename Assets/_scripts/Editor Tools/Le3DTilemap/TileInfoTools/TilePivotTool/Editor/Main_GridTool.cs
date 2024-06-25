using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using CJUtils;

namespace Le3DTilemap {

    public abstract partial class GridTool : EditorTool {

        protected DynamicGridSettings gridSettings;
        protected DynamicGridQuad gridQuad;

        protected Texture2D iconSearch, iconPlus, iconGrip,
                            iconSettings, iconMove, iconTurn;

        public override void OnActivated() {
            SceneView.duringSceneGui += OnSceneGUI;
            if (gridSettings is null) {
                AssetUtils.TryRetrieveAsset(out gridSettings);
            } if (gridSettings is not null) {
                InitializeLocalGrid();
            } depth = 0;
            LoadIcons();
            ResetWindowProperties();
        }

        protected bool HasNullSettings<T>(ref T settings, SceneView sceneView) where T : ScriptableObject {
            bool missingSettings = gridSettings == null || settings == null;
            if (missingSettings) {
                Handles.BeginGUI();
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope()) {
                        GUILayout.FlexibleSpace();
                        if (settings == null) {
                            EditorGUILayout.GetControlRect(GUILayout.Height(1));
                            SceneViewUtils.DrawMissingSettingsPrompt(ref settings, sceneView,
                                                    "Missing Tool Settings",
                                                    "New Tool Settings",
                                                    iconSearch, iconPlus);
                            EditorGUILayout.GetControlRect(GUILayout.Height(1));
                        } if (gridSettings == null) {
                            EditorGUILayout.GetControlRect(GUILayout.Height(1));
                            SceneViewUtils.DrawMissingSettingsPrompt(ref gridSettings, sceneView,
                                                    "Missing Grid Settings",
                                                    "New Grid Settings",
                                                    iconSearch, iconPlus);
                            EditorGUILayout.GetControlRect(GUILayout.Height(1));
                        } GUILayout.FlexibleSpace();
                    } GUILayout.FlexibleSpace();
                } Handles.BeginGUI();
            } return missingSettings;
        }

        private void InitializeLocalGrid() {
            gridQuad = FindAnyObjectByType<DynamicGridQuad>(FindObjectsInactive.Include);
            if (gridQuad == null) {
                GameObject go = PrefabUtility.InstantiatePrefab(gridSettings.quadPrefab) as GameObject;
                if (!go.TryGetComponent(out gridQuad)) DestroyImmediate(go);
            } if (gridQuad == null) {
                Debug.LogWarning("Missing Dynamic Grid Quad");
                return;
            } StageUtility.PlaceGameObjectInCurrentStage(gridQuad.gameObject);
            gridQuad.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInEditor;
            SetGridOrientation(GridOrientation.XZ);
            ToggleQuad(true);
            SetGridDiameter(gridSettings.diameter);
            SetGridThickness(gridSettings.thickness);
            SetIgnoreZTest(gridSettings.ignoreZTest);
            SetGridColor(gridSettings.baseColor);
            UpdateGridDepth();
        }

        protected virtual void OnSceneGUI(SceneView sceneView) { }

        public override void OnWillBeDeactivated() {
            SceneView.duringSceneGui -= OnSceneGUI;
            gridSettings = null;
            if (gridQuad) {
                DestroyImmediate(gridQuad.gameObject);
            }
        }

        protected virtual void LoadIcons() {
            EditorUtils.LoadIcon(ref iconSearch, EditorUtils.ICON_SEARCH);
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
            EditorUtils.LoadIcon(ref iconGrip, EditorUtils.ICON_VGRIP);
            EditorUtils.LoadIcon(ref iconSettings, EditorUtils.ICON_SETTINGS);
            EditorUtils.LoadIcon(ref iconMove, "_GridMove");
            EditorUtils.LoadIcon(ref iconTurn, "_GridTurn");
        }
    }
}