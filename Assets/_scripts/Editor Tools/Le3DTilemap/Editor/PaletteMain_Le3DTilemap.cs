using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapWindow {

        private string searchString = "";
        private readonly List<TileData> dragSelectionGroup = new();
        private bool mouseOverButton;

        public void DrawPaletteEditor() {
            using (new EditorGUILayout.VerticalScope(UIStyles.WindowBox, GUILayout.ExpandHeight(true))) {
                EditorGUILayout.GetControlRect(GUILayout.Height(2));
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(EditorUtils.FetchIcon("d_Tilemap.ActiveTargetLayers"));
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
                using (new EditorGUILayout.VerticalScope(GUI.skin.box, GUILayout.ExpandHeight(true))) {
                    if (prefs.activePalette is not null) {
                        DrawPaletteToolbar();
                        if (prefs.activePalette.Count == 0) {
                            GUIUtils.DrawScopeCenteredText("Tile Palette is Empty");
                        } else {
                            using (new EditorGUILayout.ScrollViewScope(Vector2.zero)) {
                                DrawTileCards(prefs.activePalette.Tiles);
                            }
                        }
                    } else GUIUtils.DrawScopeCenteredText("No Tile Palette Selected");
                }
            }
        }

        private void DrawTileCards(List<TileData> tiles) {
            Debug.Log(dragSelectionGroup.Count);
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
                                DrawTileCard(tiles[j]);
                            }
                        }
                    } DeselectionCheck();
                }
            }
        }

        /// <summary>
        /// Draws a Prefab Card containing buttons;
        /// </summary>
        private void DrawTileCard(TileData data) {
            bool objectInSelection = dragSelectionGroup.Contains(data);
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(false))) {
                DrawDragAndDropPreview(data, objectInSelection);
                //GUIStyle labelStyle = new GUIStyle(UIStyles.CenteredLabel) { clipping = TextClipping.Clip };
                if (objectInSelection) GUI.color = UIColors.Cyan;
                GUIContent content = new(data.name, data.name);
                GUILayout.Label(content, UIStyles.TextBoxLabel, GUILayout.Width(prefs.cardSize),
                                GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// Creates a Drag & Drop button for a given prefab;
        /// </summary>
        /// <param name="objectInSelection"> Whether the prefab is in the current Drag & Drop Selection Group; </param>
        private void DrawDragAndDropPreview(TileData data, bool objectInSelection) {
            using (new EditorGUILayout.VerticalScope()) {
                Rect buttonRect = GUILayoutUtility.GetRect(prefs.cardSize, prefs.cardSize,
                                                           GUILayout.ExpandWidth(false));
                if (buttonRect.Contains(Event.current.mousePosition)) {
                    mouseOverButton = true;
                    bool mouseDown = Event.current.type == EventType.MouseDown;
                    bool mouseDrag = Event.current.type == EventType.MouseDrag;
                    bool leftClick = Event.current.button == 0;
                    bool rightClick = Event.current.button == 1;
                    if (Event.current.shift) {
                        if (objectInSelection) {
                            if (mouseDown || (mouseDrag && rightClick)) dragSelectionGroup.Remove(data);
                        } else if (mouseDown || (mouseDrag && leftClick)) dragSelectionGroup.Add(data);
                    } else if (mouseDown && leftClick) {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.StartDrag("Dragging");
                        DragAndDrop.objectReferences = dragSelectionGroup.Count > 1
                                                       ? dragSelectionGroup.ToArray() : new Object[] { data };
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                } if (objectInSelection) GUI.color = UIColors.DefinedBlue;
                GUI.Label(buttonRect, "", GUI.skin.button);
                GUI.color = Color.white;
                /// Resizing Logic:
                /// - Preview rect is a percent of the total size of the old rect;
                /// - Half of the subtracted width/height is added to center rect;
                GUI.Label(new (buttonRect) { x = buttonRect.x + buttonRect.width * 0.075f,
                                             y = buttonRect.y + buttonRect.height * 0.075f,
                                             width = buttonRect.width * 0.85f,
                                             height = buttonRect.height * 0.85f }, data.Preview);
            }
        }

        /// <summary>
        /// Whether a Drag & Drop Selection Group wipe may happen at the end of the frame;
        /// </summary>
        private void DeselectionCheck() {
            if (!mouseOverButton && Event.current.type == EventType.MouseDown && !Event.current.shift
                && Event.current.button == 0 && dragSelectionGroup.Count > 0) dragSelectionGroup.Clear();
            mouseOverButton = false;
        }
    }
}
