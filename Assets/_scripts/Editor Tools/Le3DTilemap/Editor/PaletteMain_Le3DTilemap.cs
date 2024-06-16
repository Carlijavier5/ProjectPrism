using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapWindow {

        private string searchString = "";
        private List<TileData> shownTiles;

        private readonly string[] paletteDropdown = new string[] { "Create New..." };
        private readonly string[] tileAddDropdown = new string[] { "Add Tile...",
                                                                   "New Tile... " };

        private bool mouseInScope;
        private enum DragAndDropState { None, Invalid, TileData, GameObject }
        private DragAndDropState dndState = DragAndDropState.None;

        private PaletteEditMode activeEditMode;
        private bool CanDrag => activeEditMode == PaletteEditMode.Editable;

        private TileData SelectedTile { get; set; }
        private TileData potentialDrag;
        private bool isDrag;

        private Vector2 paletteScroll;

        public void DrawPaletteEditor() {
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox, GUILayout.ExpandHeight(true))) {
                EditorGUILayout.GetControlRect(GUILayout.Height(2));
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(iconTilemap);
                    using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                        prefs.activePalette = EditorGUILayout.ObjectField(prefs.activePalette, typeof(TilePalette3D),
                                                                          false) as TilePalette3D;
                        if (changeScope.changed) EditorUtility.SetDirty(prefs);
                    } Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(20));
                    if (EditorGUI.DropdownButton(rect, GUIContent.none, FocusType.Keyboard)) {
                        EditorUtils.RequestDropdownCallback(rect, paletteDropdown, CreatePaletteCallback);
                    } GUILayout.FlexibleSpace();
                } EditorGUILayout.GetControlRect(GUILayout.Height(5));
                GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                ProcessDragAndDrop(out dndState);
                HandleDropZone(dndState);
                if (mouseInScope) {
                    DragAndDrop.visualMode = dndState switch {
                        DragAndDropState.Invalid => DragAndDropVisualMode.Rejected,
                        DragAndDropState.GameObject => DragAndDropVisualMode.Copy,
                        DragAndDropState.TileData => DragAndDropVisualMode.Move,
                        _ => DragAndDrop.visualMode,
                    };
                } GUI.enabled = dndState == DragAndDropState.TileData
                             || dndState == DragAndDropState.GameObject;
                using (var boxScope = new EditorGUILayout.VerticalScope(EditorStyles.numberField,
                                                                          GUILayout.ExpandHeight(true))) {
                    GUI.enabled = true;
                    if (prefs.activePalette is not null) {
                        DrawPaletteToolbar();
                        if (prefs.activePalette.Count == 0) {
                            GUIUtils.DrawScopeCenteredText("Tile Palette is Empty");
                        } else {
                            if (!string.IsNullOrEmpty(searchString)) {
                                GUILayout.Label($"Showing Results for \"{searchString}\"", UIStyles.TextBoxLabel);
                            } using (var scope = new EditorGUILayout.ScrollViewScope(paletteScroll)) {
                                paletteScroll = scope.scrollPosition;
                                DrawTileCards(shownTiles);
                            }
                        }
                    } else GUIUtils.DrawScopeCenteredText("No Tile Palette Selected");
                    if (Event.current.type == EventType.Repaint) {
                        mouseInScope = boxScope.rect.Contains(Event.current.mousePosition);
                    }
                }
            }
        }

        private void ProcessDragAndDrop(out DragAndDropState stateRes) {
            if (DragAndDrop.objectReferences.Length == 1) {
                object draggedObject = DragAndDrop.objectReferences[0];
                stateRes = draggedObject is TileData ? DragAndDropState.TileData
                                                     : draggedObject is GameObject ? DragAndDropState.GameObject
                                                                                   : DragAndDropState.Invalid;
            } else stateRes = DragAndDrop.objectReferences.Length == 0 ? DragAndDropState.None
                                                                       : DragAndDropState.Invalid;
        }

        private void HandleDropZone(DragAndDropState state) {
            if (mouseInScope) {
                object[] result = EditorUtils.DropZone();
                if (result == null || result.Length != 1) return;
                switch (state) {
                    case DragAndDropState.TileData:
                        OPCallback(result[0], true);
                        break;
                    case DragAndDropState.GameObject:
                        GameObject prefab = result[0] as GameObject;
                        TileCreationWindow window = TileCreationWindow.ShowAuxiliary(prefab);
                        window.OnTileCreation += Window_OnTileCreation;
                        break;
                }
            }
        }

        private void DrawTileCards(List<TileData> tiles) {
            if (tiles.Count == 0) {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope()) {
                    EditorGUILayout.Separator(); EditorGUILayout.Separator();
                    GUILayout.Label("Tile Palette is Empty", UIStyles.CenteredLabelBold);
                } GUILayout.FlexibleSpace();
            } else {
                int amountPerRow = Mathf.CeilToInt((position.xMax - position.xMin - 10)
                                                   / (prefs.cardSize * prefs.cardSizeMultiplier + 7)) - 1;
                using (new EditorGUILayout.VerticalScope()) {
                    for (int i = 0; i < Mathf.CeilToInt(((float) tiles.Count) / amountPerRow); i++) {
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Width(prefs.cardSize * prefs.cardSizeMultiplier))) {
                            for (int j = i * amountPerRow; j < Mathf.Min((i + 1) * amountPerRow, tiles.Count); j++) {
                                if (j != i * amountPerRow) EditorGUILayout.GetControlRect(GUILayout.Width(1));
                                DrawTileCard(tiles[j], j);
                            }
                        }
                    }
                }
            }
        }

        private void DrawTileCard(TileData data, int index) {
            bool isSelected = SelectedTile == data;
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(false))) {
                DrawDragAndDropPreview(data, index, isSelected);
                if (isSelected) GUI.color = UIColors.MidBlue;
                GUIContent content = new(data.name, data.name);
                GUILayout.Label(content, UIStyles.TextBoxLabel, GUILayout.Width(prefs.cardSize),
                                GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUI.color = Color.white;
            }
        }

        private void DrawDragAndDropPreview(TileData data, int index, bool isSelected) {
            using (new EditorGUILayout.VerticalScope()) {
                Rect buttonRect = GUILayoutUtility.GetRect(prefs.cardSize, prefs.cardSize,
                                                           GUILayout.ExpandWidth(false));
                if (mouseInScope && buttonRect.Contains(Event.current.mousePosition)) {
                    bool mouseUp = Event.current.type == EventType.MouseUp;
                    if (DragAndDrop.objectReferences.Length == 1) {
                        TileData draggedTile = DragAndDrop.objectReferences[0] as TileData;
                        if (draggedTile && prefs.activePalette.Tiles[index] != draggedTile) {
                            bool wasPresent = prefs.activePalette.Remove(draggedTile);
                            prefs.activePalette.Insert(index, draggedTile);
                            if (!wasPresent) GUIUtility.ExitGUI();
                        }
                    } else {
                        bool mouseDown = Event.current.type == EventType.MouseDown;
                        bool mouseDrag = Event.current.type == EventType.MouseDrag;
                        bool leftClick = Event.current.button == 0;
                        bool rightClick = Event.current.button == 1;
                        if (mouseDown) {
                            if (leftClick) {
                                potentialDrag = data;
                            } else if (rightClick) {
                                EditorUtils.RequestDropdownCallback(new Rect(Event.current.mousePosition, Vector2.zero),
                                                                    new string[] { "Remove" }, new TileData[] { data },
                                                                    RemoveTileCallback);
                            }
                        } else if (potentialDrag) {
                            if (CanDrag && mouseDrag) {
                                DragAndDrop.PrepareStartDrag();
                                DragAndDrop.StartDrag("Dragging");
                                DragAndDrop.objectReferences = new Object[] { data };
                                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                                ConfirmDrag();
                                potentialDrag = null;
                            } else if (mouseUp) {
                                ToggleSelectedTile(data);
                                potentialDrag = null;
                            }
                        }
                    } /// Breathing room for short dragged clicks;
                    if (Event.current.type == EventType.DragExited && !isDrag) {
                        ToggleSelectedTile(prefs.activePalette.Tiles[index]);
                    }
                } GUI.color = isSelected ? UIColors.DefinedBlue : new Vector4(1.25f, 1.25f, 1.25f, 1);
                GUI.Label(buttonRect, "", isSelected ? GUI.skin.button : EditorStyles.textArea);
                GUI.color = Color.white;
                /// Resizing Logic:
                /// - Preview rect is a percent of the total size of the old rect;
                /// - Half of the subtracted width/height is added to center rect;
                GUIStyle previewStyle = new(UIStyles.WindowBox) { padding = new RectOffset(2, 2, 2, 2),
                                                                  contentOffset = Vector2.zero };
                GUI.Label(new (buttonRect) { x = buttonRect.x + buttonRect.width * 0.0875f,
                                             y = buttonRect.y + buttonRect.height * 0.0875f,
                                             width = buttonRect.width * 0.825f,
                                             height = buttonRect.height * 0.825f }, data.Preview, previewStyle);
            }
        }

        private void ToggleSelectedTile(TileData data) {
            SelectedTile = SelectedTile == data ? null :data;
        }

        private void RemoveTileCallback(object res) {
            TileData data;
            if (data = res as TileData) {
                prefs.activePalette.Remove(data);
            }
        }

        private async void ConfirmDrag() {
            isDrag = false;
            await Task.Delay(100);
            isDrag = true;
        }
    }
}
