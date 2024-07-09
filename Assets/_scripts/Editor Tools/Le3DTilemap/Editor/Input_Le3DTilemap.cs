using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Le3DTilemap {
    public partial class Le3DTilemapTool {

        private PhysicsScene physicsSpace;

        private double raycastCD;
        private double RaycastCD {
            get => EditorApplication.timeSinceStartup > raycastCD ? -1 : 1;
            set { raycastCD = EditorApplication.timeSinceStartup + value; }
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

        private void DoToolInput(SceneView sceneView) {
            switch (toolMode) {
                case ToolMode.Select:
                    DoRaycastInput(DoSingleSelectionSignal);
                    break;
            }
        }

        private void DoRaycastInput(System.Action<EventType> RaycastSignalFunc) {
            if (Event.current.button > 0) return;
            switch (Event.current.type) {
                case EventType.MouseMove:
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
            if (physicsSpace.Raycast(ray.origin, ray.direction, out RaycastHit hit, 1000f, 1 << 6)
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


    }
}