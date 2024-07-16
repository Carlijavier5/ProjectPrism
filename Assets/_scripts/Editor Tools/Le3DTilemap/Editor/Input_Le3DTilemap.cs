using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using CJUtils;

namespace Le3DTilemap {
    public partial class Le3DTilemapTool {

        private PhysicsScene physicsSpace;

        private double raycastCD;
        private double RaycastCD {
            get => EditorApplication.timeSinceStartup > raycastCD ? -1 : 1;
            set { raycastCD = EditorApplication.timeSinceStartup + value; }
        }

        private void HighlightModeContent(SceneView sceneView) {
             switch (toolMode) {
                case ToolMode.Select:
                    break;
                case ToolMode.MSelect:
                    DrawAreaSelection();
                    break;
                case ToolMode.Paint:

                    break;
            }
        }

        private void LoadPhysicsScene(out PhysicsScene physicsSpace) {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null) {
                physicsSpace = prefabStage.scene.GetPhysicsScene();
            } else {
                Scene scene = SceneManager.GetActiveScene();
                physicsSpace = scene.GetPhysicsScene();
            }
        }

        private void DoToolInput(SceneView sceneView) {
            switch (toolMode) {
                case ToolMode.Select:
                    DoRaycastInput(DoSingleSelectionSignal);
                    break;
                case ToolMode.MSelect:
                    DoRaycastInput(DoAreaSelectionSignal);
                    break;
                case ToolMode.Paint:
                    DoRaycastInput(DoPaintSignal);
                    break;
            }
        }

        private void DoRaycastInput(System.Action<EventType> RaycastSignalFunc) {
            if (Event.current.button > 0) return;
            switch (Event.current.type) {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    if (RaycastCD > 0) {
                        pendingCast = true;
                    } else {
                        RaycastCD = gridSettings.raycastCDMult * 0.005f;
                        RaycastSignalFunc(EventType.MouseMove);
                    } break;
                case EventType.MouseUp:
                case EventType.MouseDown:
                    RaycastSignalFunc(Event.current.type);
                    break;
                case EventType.Repaint:
                    if (RaycastCD < 0 && pendingCast) {
                        RaycastSignalFunc(EventType.MouseMove);
                    } break;
            }
        }
    }

    public partial class Le3DTilemapTool {

        private TileInfo selectionInfoHint;
        private TileInfo selectionInfoPotential;
        private TileInfo selectedInfoInstance;

        private void DoSingleSelectionSignal(EventType eventType) {
            pendingCast = false;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (physicsSpace.Raycast(ray.origin, ray.direction, 
                out RaycastHit hit, gridSettings.raycastDistance, 1 << 6)
                && hit.collider.TryGetComponent(out TileInfo info) && info.IsInstance) {
                switch (eventType) {
                    case EventType.MouseMove:
                        selectionInfoHint = info;
                        break;
                    case EventType.MouseDown:
                        selectionInfoPotential = info;
                        break;
                    case EventType.MouseUp:
                        if (selectionInfoPotential == info
                            && selectionInfoPotential) {
                            selectedInfoInstance = selectionInfoPotential;
                        } ResetSelectionHint();
                        RaycastCD = gridSettings.raycastCDMult * 0.025f;
                        break;
                }
            } else ResetSelectionHint();
        }

        private void ResetSelectionHint() {
            selectionInfoHint = null;
            selectionInfoPotential = null;
        }
    }

    public partial class Le3DTilemapTool {

        private enum AreaDrawState { None, StartAwait, MarkCorners, EndAwait, Drag }
        private AreaDrawState areaDrawState;
        
        private Vector3Int areaStart;
        private Vector3Int areaEnd;

        private void DrawAreaSelection() {
            if (Event.current.type == EventType.Repaint) {
                if (HasHint) {
                    using (new Handles.DrawingScope(UIColors.Blue)) {
                        Handles.DrawWireCube(HintTile, Vector3.one * 0.75f);
                    }
                }
                switch (areaDrawState) {
                    case AreaDrawState.Drag:
                    case AreaDrawState.EndAwait:
                        Vector3 center1 = (areaStart - Vector3.one * OFFSET + areaEnd + Vector3.one * OFFSET) / 2;
                        Vector3 size1 = ((areaEnd - areaStart).Abs() + Vector3.one);
                        HandleUtils.DrawOctohedralVolume(center1, size1, Color.blue, Color.blue);
                        break;
                    case AreaDrawState.MarkCorners:
                        if (HasHint) {
                            Vector3 center2 = (areaStart - Vector3.one * OFFSET + areaEnd + Vector3.one * OFFSET) / 2;
                            HandleUtils.DrawOctohedralVolume(center2, (areaEnd - areaStart).Abs() + Vector3.one, Color.blue, Color.blue);
                        } break;
                }
            }
        }

        /*
        private void HighlightHintTile() {
            if (Event.current.type == EventType.Repaint) {
                if (HasHint) {
                    if (hasPicked) {
                        HandleUtils.DrawOctohedralVolume(pickedTile, Vector3.one,
                                                         new Vector4(1, 0, 0, 0.25f),
                                                         Color.white);
                    } else {
                        using (new Handles.DrawingScope(UIColors.Blue)) {
                            Handles.DrawWireCube(HintTile, Vector3.one * 0.75f);
                        }
                    }
                }
                HandleUtils.DrawDottedOctohedron(Info.transform.position,
                                                 Vector3.one, Color.white, 5f);
                using (new Handles.DrawingScope(Color.red)) {
                    Handles.SphereHandleCap(0, Info.transform.position,
                                            Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }
        }*/

        private void DoAreaSelectionSignal(EventType eventType) {
            pendingCast = false;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (plane.Raycast(ray, out float enter)
                && enter <= gridSettings.raycastDistance) {
                Vector3Int hintTile = ray.GetPoint(enter).Round();
                switch (eventType) {
                    case EventType.MouseMove:
                        AreaSelectMove(hintTile);
                        break;
                    case EventType.MouseDown:
                        AreaSelectDown(hintTile);
                        break;
                    case EventType.MouseUp:
                        AreaSelectUp(hintTile);
                        RaycastCD = gridSettings.raycastCDMult * 0.025f;
                        break;
                }
            } else ClearHint();
        }

        private void AreaSelectMove(Vector3Int hintTile) {
            switch (areaDrawState) {
                case AreaDrawState.None:
                    HintTile = hintTile;
                    break;
                case AreaDrawState.StartAwait:
                case AreaDrawState.EndAwait:
                    if (HintTile != hintTile) {
                        areaDrawState = AreaDrawState.Drag;
                        areaEnd = hintTile;
                        ClearHint();
                    } break;
                case AreaDrawState.Drag:
                    areaEnd = hintTile;
                    break;
            }
        }

        private void AreaSelectDown(Vector3Int hintTile) {
            switch (areaDrawState) {
                case AreaDrawState.None:
                    areaStart = hintTile;
                    areaDrawState = AreaDrawState.StartAwait;
                    break;
                case AreaDrawState.MarkCorners:
                    areaEnd = hintTile;
                    areaDrawState = AreaDrawState.EndAwait;
                    break;
            }
        }

        private void AreaSelectUp(Vector3Int hintTile) {
            switch (areaDrawState) {
                case AreaDrawState.StartAwait:
                    areaDrawState = AreaDrawState.MarkCorners;
                    break;
                case AreaDrawState.EndAwait:
                case AreaDrawState.Drag:
                    if (hintTile == areaEnd) {
                        AreaSelectOutput();
                    } else { /* Reset */ }
                    areaDrawState = AreaDrawState.None;
                    break;
            }
        }

        private void AreaSelectOutput() {
            Debug.Log($"Start: {areaStart} | End: {areaEnd}");
        }
    }

    public partial class Le3DTilemapTool {

        private Vector3[] highlightCenters;

        private void DrawPaintHint() {
            if (HasHint) {
                
            }
        }

        private void DoPaintSignal(EventType eventType) {
            pendingCast = false;
            bool standardHit = false;

            if (!GridKey) {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (physicsSpace.Raycast(ray.origin, ray.direction,
                    out RaycastHit hit, gridSettings.raycastDistance, 1 << 6)) {
                    standardHit = true;
                    Vector3Int hintTile = (hit.point + hit.normal * OFFSET).Round();
                    switch (eventType) {
                        case EventType.MouseMove:
                            PaintMove(hintTile, false);
                            break;
                        case EventType.MouseDown:
                            PaintDown(hintTile);
                            break;
                        case EventType.MouseUp:
                            RaycastCD = gridSettings.raycastCDMult * 0.025f;
                            break;
                    }
                }
            } 

            if (!standardHit) {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (plane.Raycast(ray, out float enter)
                    && enter <= gridSettings.raycastDistance) {
                    Vector3Int hintTile = ray.GetPoint(enter).Round();
                    switch (eventType) {
                        case EventType.MouseMove:
                            PaintMove(hintTile, true);
                            break;
                        case EventType.MouseDown:
                            PaintDown(hintTile);
                            break;
                        case EventType.MouseUp:
                            RaycastCD = gridSettings.raycastCDMult * 0.025f;
                            break;
                    }
                } else ClearHint();
            }
        }

        private void PaintMove(Vector3Int hintTile, bool allowDrag) {
            if (HintTile != hintTile) {
                if (allowDrag && Event.current.type == EventType.MouseDrag) {
                    /// Paint on new tile;
                } else {
                    /// Recompute highlight;
                }
            } SetPaintHint(hintTile);
        }

        private void SetPaintHint(Vector3Int hintTile) {
            highlightCenters.Offset(hintTile - HintTile);
            HintTile = hintTile;
        }

        private void PaintDown(Vector3Int paintDown) {
            /// Paint on grid;
            ClearHint();
        }
    }
}