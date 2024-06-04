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
                if (EditorGUILayout.DropdownButton(new(EditorUtils.FetchIcon("d_Toolbar Plus More")),
                                                   FocusType.Keyboard, EditorStyles.toolbarButton)) {
                    EditorUtils.RequestDropdownCallback(rect, tileAddDropdown, AddTileCallback);
                } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                    searchString = EditorGUILayout.TextField(searchString,
                                                             EditorStyles.toolbarSearchField,
                                                             GUILayout.MinWidth(150));
                    if (changeScope.changed) { } // Update Search Results;
                } EditorGUILayout.GetControlRect(GUILayout.MinWidth(5));
                GUILayout.Label("Mode:");
                using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                    prefs.editMode = (PaletteEditMode) EditorGUILayout.EnumPopup(prefs.editMode, EditorStyles.toolbarDropDown,
                                                                             GUILayout.MinWidth(100), GUILayout.MaxWidth(150));
                    if (changeScope.changed) EditorUtility.SetDirty(prefs);
                }
            } if (awaitOPCallback) {
                TileData newTile = EditorUtils.CatchOPEvent<TileData>();
                if (Event.current.commandName == EditorUtils.OBJECT_PICKER_CLOSED) {
                    if (newTile is not null) prefs.activePalette.Tiles.Add(newTile);
                    awaitOPCallback = false;
                }
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
                    EditorUtils.ShowObjectPicker<TileData>(null);
                    awaitOPCallback = true;
                    break;
                case 1:
                    TileCreationWindow window = TileCreationWindow.ShowAuxiliary(null);
                    window.OnTileCreation += Window_OnTileCreation;
                    break;
            }
        }

        private void Window_OnTileCreation(TileData tileData) {
            if (tileData) prefs.activePalette.Insert(0, tileData);
        }
    }
}
