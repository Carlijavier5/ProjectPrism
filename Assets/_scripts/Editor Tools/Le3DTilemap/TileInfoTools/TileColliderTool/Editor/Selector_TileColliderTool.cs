using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using CJUtils;

namespace Le3DTilemap {
    public partial class TileColliderTool {

        private PhysicsScene physicsSpace;

        private BoxCollider hintCollider;
        private int selectionIndex;

        private double raycastCD;
        private double RaycastCD {
            get => EditorApplication.timeSinceStartup > raycastCD ? -1 : 1;
            set { raycastCD = EditorApplication.timeSinceStartup + value; }
        }

        private void HighlightSelectedCollider() {
            if (Info.SelectedCollider != null) {
                using (new Handles.DrawingScope(new Vector4(0.392f, 0.533f, 0.917f, 1f))) {
                    Vector3 center = Info.transform.TransformPoint(Info.SelectedCollider.Center);
                    Handles.DrawWireCube(center, Info.SelectedCollider.Size);
                }
            }
        }

        private void HighlightHintCollider() {
            if (hintCollider) {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Color surfaceColor = selectionIndex >= 0 ? new Vector4(1, 1, 0, 0.25f)
                                                         : new Vector4(0, 0, 1, 0.15f);
                Color outlineColor = selectionIndex >= 0 ? new Vector4(0, 1, 0, 0.5f)
                                                         : new Vector4(1, 1, 0, 0.5f);
                Vector3 center = Info.transform.TransformPoint(hintCollider.center);
                HandleUtils.DrawOctohedralVolume(center, hintCollider.size,
                                                 surfaceColor, outlineColor);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            }
        }

        public void LoadPhysicsScene(out PhysicsScene physicsSpace) {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null) {
                physicsSpace = prefabStage.scene.GetPhysicsScene();
            } else {
                Scene scene = SceneManager.GetActiveScene();
                physicsSpace = scene.GetPhysicsScene();
            }
        }

        private void ResetSelection() {
            hintCollider = null;
            selectionIndex = -1;
        }

        private void DoSelectionInput() {
            if (Event.current.button > 0) return;
            switch (Event.current.type) {
                case EventType.MouseMove:
                    if (RaycastCD > 0) {
                        pendingCast = true;
                    } else {
                        RaycastCD = gridSettings.raycastCDMult * 0.005f;
                        DoSelectionSignal(EventType.MouseMove);
                    } break;
                case EventType.MouseUp:
                case EventType.MouseDown:
                    DoSelectionSignal(Event.current.type);
                    break;
                case EventType.Repaint:
                    if (RaycastCD < 0 && pendingCast) {
                        DoSelectionSignal(EventType.MouseMove);
                    } break;
            }
        }

        private void DoSelectionSignal(EventType eventType) {
            pendingCast = false;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (physicsSpace.Raycast(ray.origin, ray.direction, out RaycastHit hit, 1000f, 1 << 6)
                && hit.collider.TryGetComponent(out TileInfo info)) {
                TileCollider collider = info.FindColliderInfo(hit.collider as BoxCollider,
                                                              out int index);
                if (index < 0) {
                    if (eventType == EventType.MouseDown) {
                        Debug.LogWarning("Collider does not belong to Tile");
                    } return;
                } switch (eventType) {
                    case EventType.MouseMove:
                        hintCollider = collider.collider;
                        break;
                    case EventType.MouseDown:
                        selectionIndex = index;
                        break;
                    case EventType.MouseUp:
                        if (selectionIndex >= 0
                            && selectionIndex == index) {
                            if (Info.SelectedIndex != selectionIndex) {
                                toolMode = ToolMode.Scale;
                            } Info.ToggleSelectedIndex(selectionIndex);
                        } ResetSelection();
                        RaycastCD = 0.5f;
                        break;
                }
            } else ResetSelection();
        }
    }
}