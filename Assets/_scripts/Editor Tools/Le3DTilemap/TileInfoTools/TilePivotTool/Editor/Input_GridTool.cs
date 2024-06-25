using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using CJUtils;

namespace Le3DTilemap {

    public abstract partial class GridTool {

        private const float OFFSET = 0.5f;

        protected GridInputMode defaultInput;
        protected GridInputMode overrideInput;

        protected Plane plane;
        protected int depth;

        protected double wheelCD;
        protected double WheelCD {
            get => EditorApplication.timeSinceStartup > wheelCD ? -1 : 1;
            set { wheelCD = EditorApplication.timeSinceStartup + value; }
        } protected bool pendingCast;

        protected void DoScrollInput(SceneView sceneView) {
            if (Event.current.type == EventType.ScrollWheel
                && ((int) defaultInput > 0 || (int) overrideInput > 0)) {
                if (WheelCD < 0) {
                    DoOffsetScroll(sceneView, Event.current.delta.y);
                } Event.current.Use();
            } UpdateGridPos(sceneView);
        }

        private void DoOffsetScroll(SceneView sceneView, float delta) {
            GridInputMode activeMode = overrideInput > 0 ? overrideInput
                                                         : defaultInput;
            int intDelta = (int) Mathf.Sign(delta);
            switch (activeMode) {
                case GridInputMode.Move:
                    if (this.orientation switch { 
                            GridOrientation.XZ
                            => sceneView.camera.transform.position.y,
                            GridOrientation.XY
                            => sceneView.camera.transform.position.z,
                            _
                            => sceneView.camera.transform.position.x,
                        } < depth) {
                        intDelta *= -1;
                    } depth += intDelta;
                    pendingCast = true;
                    UpdateGridDepth();
                    WheelCD = 0.075;
                    break;
                case GridInputMode.Turn:
                    int orientation = ((int) this.orientation + intDelta) % 3;
                    orientation = (orientation < 0) ? 3 + orientation : orientation;
                    SetGridOrientation((GridOrientation) orientation);
                    WheelCD = 0.25;
                    break;
            }
        }

        protected void UpdateGridDepth() {
            Vector3 normal;
            Vector3 nonNormalAxis;
            switch (orientation) {
                case GridOrientation.XZ:
                    normal = Vector3.up;
                    nonNormalAxis = new Vector3(gridQuad.Position.x,
                                                0, gridQuad.Position.z);
                    break;
                case GridOrientation.XY:
                    normal = Vector3.forward;
                    nonNormalAxis = new Vector3(gridQuad.Position.x,
                                                gridQuad.Position.y, 0);
                    break;
                default:
                    normal = Vector3.right;
                    nonNormalAxis = new Vector3(0, gridQuad.Position.y,
                                                gridQuad.Position.z);
                    break;
            } gridQuad.Position = nonNormalAxis + normal * (depth - OFFSET);
            plane = new Plane(normal, nonNormalAxis + normal * depth);
        }

        private void UpdateGridPos(SceneView sceneView) {
            if (Event.current.type == EventType.Repaint) {
                Vector3Int center = (gridSettings.followCamera ? sceneView.camera.transform.position
                                                           : Vector3.zero).Round();
                float depth = this.depth - OFFSET;
                switch (orientation) {
                    case GridOrientation.XZ:
                        gridQuad.Position = new Vector3(center.x, depth, center.z);
                        break;
                    case GridOrientation.XY:
                        gridQuad.Position = new Vector3(center.x, center.y, depth);
                        break;
                    case GridOrientation.YZ:
                        gridQuad.Position = new Vector3(depth, center.y, center.z);
                        break;
                }
            }
        }

        protected void DoInputOverrides() {
            if (Event.current.type == EventType.KeyDown) {
                switch (Event.current.keyCode) {
                    case KeyCode.LeftControl:
                        overrideInput = GridInputMode.Move;
                        Event.current.Use();
                        break;
                    case KeyCode.R:
                        overrideInput = GridInputMode.Turn;
                        Event.current.Use();
                        break;
                }
            } else if (Event.current.type == EventType.KeyUp) {
                overrideInput = GridInputMode.None;
            }
        }
    }
}