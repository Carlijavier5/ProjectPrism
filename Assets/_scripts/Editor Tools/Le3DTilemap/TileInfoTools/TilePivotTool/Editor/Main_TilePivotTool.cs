using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using CJUtils;

namespace Le3DTilemap {

    [EditorTool("Tile Pivot Tool", typeof(TileInfo))]
    public partial class TilePivotTool : GridTool {

        private TileInfo Info => target as TileInfo;

        private GUIContent toolIcon;
        private GUIContent ToolIcon {
            get => toolIcon ??= new(EditorUtils.FetchIcon("d_ToolHandlePivot"));
        } public override GUIContent toolbarIcon => ToolIcon;

        public override void OnActivated() {
            base.OnActivated();
            ResetSelection();
        }

        public override void OnToolGUI(EditorWindow window) {
            if (window is not SceneView sceneView) return;
            if (gridSettings == null) {
                SceneViewUtils.DrawMissingSettingsPrompt(ref gridSettings, sceneView,
                                        "Missing Dynamic Grid Settings",
                                        "New Dynamic Grid Settings",
                                        iconSearch, iconPlus);
                return;
            } DrawGridWindow(true);
            HighlightHintTile();
        }

        protected override void OnSceneGUI(SceneView sceneView) {
            bool mouseOnGUI = gridSettings.sceneGUI.rect
                              .Contains(Event.current.mousePosition);
            if (ToolManager.activeToolType != GetType()
                || !sceneView.hasFocus || gridSettings == null
                || gridQuad == null || mouseOnGUI) return;
            DoInputOverrides();
            DoSelectionInput();
            DoScrollInput(sceneView);
        }

        public override void OnWillBeDeactivated() {
            base.OnWillBeDeactivated();
            Resources.UnloadUnusedAssets();
        }
    }
}