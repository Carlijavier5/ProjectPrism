using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {
    [CustomEditor(typeof(TileInfo))]
    public class TileInfoEditor : Editor {

        private TileInfo Info => target as TileInfo;
        private Vector2 scrollPos;

        private bool mouseInScope;
        private int potentialSelection;
        private int potentialDrag;
        private int actualDrag;

        private bool selectionIsInvalid;

        private Texture2D iconPlus, iconLess, iconGrip,
                          iconDone, iconWarn, iconFail,
                          iconPivot;

        public override bool RequiresConstantRepaint() => true;

        void OnEnable() {
            Info.HideTransformAndColliders();
            Info.EventDispose();
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
            EditorUtils.LoadIcon(ref iconLess, EditorUtils.ICON_LESS);
            EditorUtils.LoadIcon(ref iconGrip, EditorUtils.ICON_HGRIP);
            EditorUtils.LoadIcon(ref iconDone, EditorUtils.ICON_CHECK_BLUE);
            EditorUtils.LoadIcon(ref iconWarn, EditorUtils.ICON_CHECK_YELLOW);
            EditorUtils.LoadIcon(ref iconFail, EditorUtils.ICON_CHECK_RED);
            EditorUtils.LoadIcon(ref iconPivot, "d_ToolHandlePivot");
            AssemblyReloadEvents.afterAssemblyReload += AssemblyDisposeCallback;
        }

        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI() {
            EditorApplication.delayCall += () => {
                selectionIsInvalid = Selection.count != 1
                                     || AssetDatabase.Contains(Selection.activeObject);
                if (!selectionIsInvalid) {
                    ToolManager.SetActiveTool<TileColliderTool>();
                }
            }; return base.CreateInspectorGUI();
        }

        void OnDisable() => AssemblyDisposeCallback();

        void AssemblyDisposeCallback() {
            Info.EventDispose();
            AssemblyReloadEvents.afterAssemblyReload -= AssemblyDisposeCallback;
        }

        public override void OnInspectorGUI() {
            GUIStyle style = new(EditorStyles.helpBox) { margin = new RectOffset(0, 0, 2, 0),
                                                         padding = new RectOffset(8, 8, 8,
                                                         Info.Colliders.Count == 0 ? 8 : 6) };
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                GUILayout.Label("Tile Pivot", UIStyles.CenteredLabelBold);
            } using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.Label("Position", GUILayout.Width(50));
                Info.TilePivot = EditorGUILayout.Vector3IntField("", Info.TilePivot,
                                                                 GUILayout.Width(150));
                GUILayout.FlexibleSpace();
                GUILayout.Label("|");
                GUILayout.FlexibleSpace();
                bool hasPivotTool = ToolManager.activeToolType == typeof(TilePivotTool);
                GUI.backgroundColor = hasPivotTool ? UIColors.DefinedBlue : Color.white;
                if (GUILayout.Button(iconPivot, GUILayout.Width(50), GUILayout.Height(18))) {
                    if (hasPivotTool) ToolManager.SetActiveTool<TileColliderTool>();
                    else ToolManager.SetActiveTool<TilePivotTool>();
                } GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(0);
            } EditorGUILayout.GetControlRect(GUILayout.Height(5));
            GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
            EditorGUILayout.GetControlRect(GUILayout.Height(5));
            using (var scope = new EditorGUILayout.VerticalScope(style)) {
                if (Info.Colliders.Count == 0) {
                    GUILayout.Label("Tiles need at least one collider!", EditorStyles.boldLabel);
                } else DrawColliderCards();
            } using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                    if (GUILayout.Button(iconPlus, EditorStyles.toolbarButton)) {
                        Info.Add();
                        Info.HideTransformAndColliders();
                    } EditorGUILayout.Space(0);
                    GUI.enabled = Info.Colliders.Count > 0;
                    if (GUILayout.Button(iconLess, EditorStyles.toolbarButton)) {
                        Info.Remove();
                    } GUI.enabled = true;
                } EditorGUILayout.Space(10);
            } EditorGUILayout.GetControlRect(GUILayout.Height(5));
            GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
            EditorGUILayout.GetControlRect(GUILayout.Height(5));
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                GUILayout.Label("Tilespace Info", UIStyles.CenteredLabelBold);
            } using (new EditorGUILayout.HorizontalScope()) {
                GUIStyle tsStyle = new(GUI.skin.box) { normal = { textColor = GUI.skin.label
                                                                  .normal.textColor },
                                                       margin = new() };
                GUILayout.FlexibleSpace();
                GUILayout.Label("Colliders:");
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{Info.Colliders.Count}", tsStyle,
                                GUILayout.Width(40));
                GUILayout.FlexibleSpace();
                GUILayout.Label("|");
                GUILayout.FlexibleSpace();
                GUILayout.Label("Size:");
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{Info.Tilespace.Count}", tsStyle,
                                GUILayout.Width(80));
                GUILayout.FlexibleSpace();
            } using (new EditorGUILayout.HorizontalScope()) {
                
            } EditorGUILayout.GetControlRect(GUILayout.Height(5));
            GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
            EditorGUILayout.GetControlRect(GUILayout.Height(5));
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                GUILayout.Label("Messages", UIStyles.CenteredLabelBold);
            } GUIStyle paddedBox = new(GUI.skin.box) { padding = new RectOffset(4, 4, 4, 4) };
            using (new EditorGUILayout.VerticalScope(paddedBox)) {
                GUIStyle hBox = new(UIStyles.HelpBoxLabel) {
                    padding = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(4, 4, 4, 4)
                };
                GUIUtils.DrawCustomHelpBox(" Minimum tile components met. Tilespace is valid!", iconDone, hBox);
                GUIUtils.DrawCustomHelpBox(" Tilespace is not hashed. Will hash automatically;", iconWarn, hBox);
            }
        }

        void OnSceneGUI() {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        private void DrawColliderCards() {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                GUILayout.Label("Tile Colliders", UIStyles.CenteredLabelBold);
            } using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPos)) {
                scrollPos = scrollScope.scrollPosition;
                if (mouseInScope && DragAndDrop.objectReferences.Length == 1) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                } using (var vScope = new EditorGUILayout.VerticalScope()) {
                    for (int i = 0; i < Info.Colliders.Count; i++) {
                        DrawColliderCard(Info.Colliders[i], i);
                    } if (Event.current.type == EventType.Repaint) {
                        mouseInScope = vScope.rect.Contains(Event.current.mousePosition);
                    } else if (Event.current.type == EventType.DragExited) {
                        actualDrag = -1;
                    } 
                }
            }
        }

        private void DrawColliderCard(TileCollider collider, int index) {
            float height = EditorGUIUtility.singleLineHeight * 2.8f;
            bool isSelected = Info.SelectedIndex == index;
            GUI.color = isSelected ? UIColors.DefinedBlue : Color.white;
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                GUI.color = isSelected ? Color.white : new Vector4(1, 1, 1, 0.1f);
                GUIStyle innerBox = new(GUI.skin.box) { margin = new RectOffset(1, 1, 1, 1) };
                using (new EditorGUILayout.HorizontalScope(innerBox)) {
                    GUI.color = Color.white;
                    DrawDragAndDropGrip(height, index, collider);
                    GUI.color = isSelected ? UIColors.DefinedBlue
                                           : new Vector4(1.25f, 1.25f, 1.25f, 1);
                    GUIStyle paddedField = new(EditorStyles.numberField) 
                                           { padding = GUI.skin.button.padding };
                    using (new EditorGUILayout.VerticalScope(isSelected ? GUI.skin.button
                                                                        : paddedField,
                                                             GUILayout.Width(40),
                                                             GUILayout.Height(height))) {
                        GUI.color = Color.white;
                        EditorGUILayout.GetControlRect(GUILayout.Height(1), GUILayout.Width(0));
                        using (new EditorGUILayout.HorizontalScope()) {
                           DrawSelectionButton(collider, index);
                        } EditorGUILayout.GetControlRect(GUILayout.Height(1), GUILayout.Width(0));
                    } using (new EditorGUILayout.VerticalScope(new GUIStyle(EditorStyles.helpBox) 
                                                               { padding = new RectOffset(8, 8, 0, 0) },
                                                               GUILayout.Height(height))) {
                        GUILayout.FlexibleSpace();
                        EditorGUIUtility.labelWidth = 1;
                        using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.Label("Pivot", GUILayout.Width(40));
                            collider.Pivot = EditorGUILayout.Vector3IntField("", collider.Pivot);
                        } using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.Label("Size", GUILayout.Width(40));
                            TileUtils.TileSizeField(collider);
                        } EditorGUIUtility.labelWidth = 0;
                        GUILayout.FlexibleSpace();
                    }
                }
            }
        }

        private void DrawSelectionButton(TileCollider collider, int index) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(40),
                                                       GUILayout.ExpandHeight(true));
            GUIStyle style = new(UIStyles.WindowBox) { padding = new RectOffset(10, 10, 10, 10),
                                                       contentOffset = Vector2.zero,
                                                       richText = true,
                                                       fontSize = 16 };
            GUI.enabled = !selectionIsInvalid;
            GUI.Label(rect, $"<i><b>{collider.name}</b></i>", style);
            GUI.enabled = true;
            rect = new Rect(rect) { x = rect.x - 6, width = rect.width + 12,
                                    y = rect.y - 6, height = rect.height + 12 };
            bool mouseOnGrip = rect.Contains(Event.current.mousePosition);
            if (mouseInScope && mouseOnGrip) {
                bool mouseDown = Event.current.type == EventType.MouseDown;
                bool mouseUp = Event.current.type == EventType.MouseUp;
                bool leftClick = Event.current.button == 0;
                if (mouseDown && leftClick) {
                    potentialSelection = index;
                } else if (mouseUp) {
                    if (potentialSelection >= 0) {
                        ToggleSelectedIndex(potentialSelection);
                    } potentialSelection = -1;
                }
            }
        }

        private void ToggleSelectedIndex(int index) {
            if (selectionIsInvalid) return;
            Info.ToggleSelectedIndex(index);
            ToolManager.SetActiveTool<TileColliderTool>();
        }

        private void DrawDragAndDropGrip(float height, int index, TileCollider collider) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(15),
                                                       GUILayout.Height(height));
            bool mouseOnGrip = rect.Contains(Event.current.mousePosition);
            if (mouseInScope && mouseOnGrip) {
                if (DragAndDrop.objectReferences.Length == 1) {
                    if (Event.current.type == EventType.Repaint) {
                        if (actualDrag >= 0 && actualDrag <= Info.Colliders.Count
                            && index != actualDrag) {
                            TileCollider dragCollider = Info.Colliders[actualDrag];
                            Info.SoftRemoveAt(actualDrag);
                            Info.Insert(index, dragCollider);
                            if (actualDrag == Info.SelectedIndex) {
                                ToggleSelectedIndex(index);
                            } else if (index == Info.SelectedIndex) {
                                ToggleSelectedIndex(actualDrag);
                            } actualDrag = index;
                        }
                    }
                } else {
                    bool mouseDown = Event.current.type == EventType.MouseDown;
                    bool leftClick = Event.current.button == 0;
                    bool rightClick = Event.current.button == 1;
                    if (mouseDown) {
                        if (leftClick) {
                            potentialDrag = index;
                        } else if (rightClick) {
                            EditorUtils.RequestDropdownCallback(new Rect(Event.current.mousePosition, Vector2.zero),
                                                                new string[] { "Remove" }, 
                                                                new DragWrapper[] { new (collider) },
                                                                RemoveColliderCallback);
                        }
                    } else if (potentialDrag >= 0) {
                        if (Event.current.type == EventType.MouseDrag) {
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.StartDrag("Dragging");
                            actualDrag = potentialDrag;
                            DragAndDrop.objectReferences = new Object[] { null };
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                            potentialDrag = -1;
                        } else if (Event.current.type == EventType.MouseUp) {
                            actualDrag = -1;
                            potentialDrag = -1;
                        }
                    }
                }
            } GUI.color = new Vector4(1, 1, 1, mouseOnGrip ? 1 : 0.25f);
            GUI.Label(rect, iconGrip, GUI.skin.label);
            GUI.color = Color.white;
        }

        private void RemoveColliderCallback(object res) {
            DragWrapper wrapper = res as DragWrapper;
            if (wrapper is not null) {

                int undoGroup = UndoUtils.StartUndoGroup("Remove Tile Collider (Tile Info)");
                UndoUtils.RecordScopeUndo(Info, "Remove Tile Collider (Tile Info)");

                int index = Info.Remove(wrapper.collider);
                if (index >= 0) {
                    if (index < Info.SelectedIndex) {
                        ToggleSelectedIndex(Info.SelectedIndex - 1);
                    } else if (index == Info.SelectedIndex) {
                        ToggleSelectedIndex(Info.SelectedIndex);
                    }
                } Undo.CollapseUndoOperations(undoGroup);
            }
        }

        private class DragWrapper : Object {
            public TileCollider collider;
            public DragWrapper(TileCollider collider) {
                this.collider = collider;
            }
        }
    }
}