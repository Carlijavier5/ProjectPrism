using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapWindow {

        private void DrawPaletteToolbar() {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                Rect rect = GUILayoutUtility.GetRect(0, 20);
                if (EditorGUILayout.DropdownButton(new(iconPlusMore), FocusType.Keyboard,
                                                   EditorStyles.toolbarButton)) {
                    EditorUtils.RequestDropdownCallback(rect, tileAddDropdown, AddTileCallback);
                } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                    searchString = EditorGUILayout.TextField(searchString,
                                                             EditorStyles.toolbarSearchField,
                                                             GUILayout.MinWidth(150));
                    if (changeScope.changed) {
                        shownTiles = null;
                        UpdateSearchResults(searchString, out shownTiles);
                    }
                } EditorGUILayout.GetControlRect(GUILayout.MinWidth(5));
                GUILayout.Label("Mode:");
                using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                    GUIStyle style = new (EditorStyles.toolbarDropDown) { contentOffset = new Vector2(2, 0) };
                    GUI.enabled = string.IsNullOrEmpty(searchString);
                    PaletteEditMode editMode  = (PaletteEditMode) EditorGUILayout.EnumPopup(activeEditMode, style,
                                                                        GUILayout.MinWidth(75), GUILayout.MaxWidth(120));
                    if (GUI.enabled) prefs.editMode = editMode;
                    GUI.enabled = true;
                    if (changeScope.changed) EditorUtility.SetDirty(prefs);
                }
            }
        }

        private void UpdateSearchResults(string searchString, out List<TileData> tiles) {
            if (string.IsNullOrWhiteSpace(searchString)) {
                tiles = prefs.activePalette.Tiles;
                activeEditMode = prefs.editMode;
            } else {
                tiles = prefs.activePalette.Tiles.FindAll((tile) => tile.name.Contains(searchString,
                                                                    System.StringComparison.OrdinalIgnoreCase));
                activeEditMode = PaletteEditMode.Focused;
            }
        }

        private void CreatePaletteCallback(object res) {
            prefs.activePalette = AssetUtils.CreateAsset<TilePalette3D>("New Tile Palette",
                                                                        "New Palette");
        }

        private void AddTileCallback(object res) {
            int selection = (int) res;
            switch (res) {
                case 0:
                    EditorUtils.ShowAdvancedObjectPicker<TileData>(OPCallback, ".asset");
                    break;
                case 1:
                    TileCreationWindow window = TileCreationWindow.ShowAuxiliary(null);
                    window.OnTileCreation += Window_OnTileCreation;
                    break;
            }
        }

        private void OPCallback(object res, bool state) {
            TileData newTile = res as TileData;
            if (newTile != null) {
                prefs.activePalette.Add(newTile);
                UpdateSearchResults(searchString, out shownTiles);
            }
        }

        private void Window_OnTileCreation(TileData tileData) {
            if (tileData) {
                prefs.activePalette.Add(tileData);
                UpdateSearchResults(searchString, out shownTiles);
            }
        }
    }
}
