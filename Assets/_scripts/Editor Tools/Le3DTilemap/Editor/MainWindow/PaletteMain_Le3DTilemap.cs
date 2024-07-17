using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapWindow {

        private string searchString = "";
        private List<TileData> shownTiles;

        private readonly string[] paletteDropdown = new string[] { "Create New..." };
        private readonly string[] tileAddDropdown = new string[] { "Add Tile...",
                                                                   "New Tile... " };
        private enum MouseMenuOption { OpenData, OpenPrefab, Remove }

        private bool mouseInScope;
        private enum DragAndDropState { None, Invalid, TileData, Prefab }
        private DragAndDropState dndState = DragAndDropState.None;

        private PaletteEditMode activeEditMode;
        private bool CanDrag => activeEditMode == PaletteEditMode.Editable;

        public TileData SelectedTile {
            get => tool == null ? null : tool.SelectedTile;
            set => tool.SelectedTile = value;
        }

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
                        settings.activePalette = EditorGUILayout.ObjectField(settings.activePalette, typeof(TilePalette3D),
                                                                          false) as TilePalette3D;
                        if (changeScope.changed) {
                            if (settings.activePalette != null) UpdateSearchResults(searchString, out shownTiles);
                            EditorUtility.SetDirty(settings);
                        }
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
                        DragAndDropState.Prefab => DragAndDropVisualMode.Copy,
                        DragAndDropState.TileData => DragAndDropVisualMode.Move,
                        _ => DragAndDrop.visualMode,
                    };
                } GUI.enabled = dndState == DragAndDropState.TileData
                             || dndState == DragAndDropState.Prefab;
                using (var boxScope = new EditorGUILayout.VerticalScope(EditorStyles.numberField,
                                                                          GUILayout.ExpandHeight(true))) {
                    GUI.enabled = true;
                    if (settings.activePalette is not null) {
                        DrawPaletteToolbar();
                        if (settings.activePalette.Count == 0) {
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
                                                     : draggedObject is GameObject 
                && PrefabUtility.GetPrefabAssetType(draggedObject as GameObject) > 0 ? DragAndDropState.Prefab
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
                    case DragAndDropState.Prefab:
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
                    string message = string.IsNullOrEmpty(searchString) ? "Tile Palette is Empty"
                                                                        : $"No matching results "
                                                                        + $"for \"{searchString}\"";
                    GUILayout.Label(message, UIStyles.CenteredLabelBold);
                } GUILayout.FlexibleSpace();
            } else {
                int amountPerRow = Mathf.CeilToInt((position.xMax - position.xMin - 10)
                                                   / (settings.cardSize * settings.cardSizeMultiplier + 7)) - 1;
                using (new EditorGUILayout.VerticalScope()) {
                    for (int i = 0; i < Mathf.CeilToInt(((float) tiles.Count) / amountPerRow); i++) {
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Width(settings.cardSize * settings.cardSizeMultiplier))) {
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
            bool isSelected = data && SelectedTile == data;
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(false))) {
                DrawDragAndDropPreview(data, index, isSelected);
                GUI.color = (!data || !data.IsValid) ? UIColors.DarkRed
                          : isSelected ? UIColors.MidBlue
                                       : Color.white;
                string bottomTag = data ? data.name : "Missing";
                GUIContent content = new(bottomTag, bottomTag);
                GUILayout.Label(content, UIStyles.TextBoxLabel, GUILayout.Width(settings.cardSize),
                                GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUI.color = Color.white;
            }
        }

        private void DrawDragAndDropPreview(TileData data, int index, bool isSelected) {
            using (new EditorGUILayout.VerticalScope()) {
                Rect buttonRect = GUILayoutUtility.GetRect(settings.cardSize, settings.cardSize,
                                                           GUILayout.ExpandWidth(false));
                if (mouseInScope && buttonRect.Contains(Event.current.mousePosition)) {
                    bool mouseUp = Event.current.type == EventType.MouseUp;
                    if (DragAndDrop.objectReferences.Length == 1) {
                        TileData draggedTile = DragAndDrop.objectReferences[0] as TileData;
                        if (draggedTile && settings.activePalette.Tiles[index] != draggedTile) {
                            bool wasPresent = settings.activePalette.Remove(draggedTile);
                            settings.activePalette.Insert(index, draggedTile);
                            if (!wasPresent) GUIUtility.ExitGUI();
                        }
                    } else {
                        bool mouseDown = Event.current.type == EventType.MouseDown;
                        bool mouseDrag = Event.current.type == EventType.MouseDrag;
                        bool leftClick = Event.current.button == 0;
                        bool rightClick = Event.current.button == 1;
                        if (mouseDown) {
                            if (leftClick && data && data.IsValid) {
                                potentialDrag = data;
                            } else if (rightClick) {
                                string[] optionsList = new[] { "Open/Data", "Open/Prefab", "Remove" };
                                (MouseMenuOption, TileData)[] content 
                                    = new (MouseMenuOption, TileData)[] {
                                    new(MouseMenuOption.OpenData, data),
                                    new(MouseMenuOption.OpenPrefab, data),
                                    new(MouseMenuOption.Remove, data),
                                }; bool[] disabled = new bool[] { !data, !data || !data.Prefab, false };
                                EditorUtils.RequestDropdownCallback(new Rect(Event.current.mousePosition,
                                                                    Vector2.zero), optionsList, content,
                                                                    disabled, RemoveTileCallback);
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
                        ToggleSelectedTile(settings.activePalette.Tiles[index]);
                    }
                } GUI.color = (!data || !data.IsValid) ? UIColors.DarkRed
                            : isSelected ? UIColors.DefinedBlue 
                                         : new Vector4(1.25f, 1.25f, 1.25f, 1);
                GUI.Label(buttonRect, "", isSelected ? GUI.skin.button : EditorStyles.textArea);
                GUI.color = Color.white;
                /// Resizing Logic:
                /// - Preview rect is a percent of the total size of the old rect;
                /// - Half of the subtracted width/height is added to center rect;
                GUIStyle previewStyle = new(UIStyles.WindowBox) { padding = new RectOffset(2, 2, 2, 2),
                                                                  contentOffset = Vector2.zero };
                buttonRect = new(buttonRect) {
                    x = buttonRect.x + buttonRect.width * 0.0875f,
                    y = buttonRect.y + buttonRect.height * 0.0875f,
                    width = buttonRect.width * 0.825f,
                    height = buttonRect.height * 0.825f
                };
                if (data && data.IsValid) {
                    GUI.Label(buttonRect, data.Preview, previewStyle);
                } else {
                    GUI.color = UIColors.Red;
                    previewStyle.alignment = TextAnchor.MiddleCenter;
                    previewStyle.fontSize = 11;
                    string invalidContext = data ? "Invalid\nData" : "Missing\nData";
                    GUI.Label(buttonRect, invalidContext, previewStyle);
                    GUI.color = Color.white;
                }
            }
        }

        private void ToggleSelectedTile(TileData data) {
            if (settings.launchToolOnSelect) ToolManager.SetActiveTool<Le3DTilemapTool>();
            if (tool) {
                SelectedTile = SelectedTile != null && SelectedTile == data ? null : data;
            } else {
                FindToolInstance();
                SelectTileAsync(data);
            }
        }

        private void RemoveTileCallback(object res) {
            if (res is (MouseMenuOption, TileData)) {
                (MouseMenuOption, TileData) o = ((MouseMenuOption, TileData)) res;
                switch (o.Item1) {
                    case MouseMenuOption.OpenData:
                        EditorUtils.InspectorFocusAsset(o.Item2, true);
                        break;
                    case MouseMenuOption.OpenPrefab:
                        AssetDatabase.OpenAsset(o.Item2.Prefab);
                        break;
                    case MouseMenuOption.Remove:
                        settings.activePalette.Remove(o.Item2);
                        break;
                }
            }
        }

        private void FindToolInstance() {
            if (ToolManager.activeToolType == typeof(Le3DTilemapTool)) {
                tool = Resources.InstanceIDIsValid(Le3DTilemapTool.InstanceID)
                     ? Resources.InstanceIDToObject(Le3DTilemapTool.InstanceID) as Le3DTilemapTool
                     : null;
            }
        }

        private async void ConfirmDrag() {
            isDrag = false;
            await Task.Delay(100);
            isDrag = true;
        }

        private async void SelectTileAsync(TileData data) {
            bool lifetimeExpired = false;
            using (var timer = new System.Threading.Timer((obj) => { lifetimeExpired = true; }, 
                                                          null, 1000, 1000)) {
                while (!tool && !lifetimeExpired) {
                    await Task.Yield();
                    if (tool) SelectedTile = data;
                }
            }
        }
    }
}