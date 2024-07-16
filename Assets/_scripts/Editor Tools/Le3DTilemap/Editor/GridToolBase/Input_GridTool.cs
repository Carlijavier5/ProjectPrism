using UnityEngine;
using UnityEditor;

namespace Le3DTilemap {
    public abstract partial class GridTool {

        protected const float OFFSET = 0.5f;

        protected GridInputMode defaultInput;
        protected GridInputMode overrideInput;

        protected Plane plane;
        protected int depth;

        private Vector3Int hintTile;
        protected Vector3Int HintTile {
            get => hintTile;
            set { hintTile = value;
                  HasHint = true; }
        }
        protected bool HasHint { get; private set; }

        protected double wheelCD;
        protected double WheelCD {
            get => EditorApplication.timeSinceStartup > wheelCD ? -1 : 1;
            set { wheelCD = EditorApplication.timeSinceStartup + value; }
        } protected bool pendingCast;

        protected bool viewBehindQuad;

        protected void DoGridInput(SceneView sceneView) {
            DoInputOverrides();
            DoScrollInput(sceneView);
        }

        private void DoInputOverrides() {
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

        private void DoScrollInput(SceneView sceneView) {
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
                    if (CameraBehindQuad(sceneView)) {
                        intDelta *= -1;
                    } depth += intDelta;
                    pendingCast = true;
                    UpdateGridDepth();
                    WheelCD = 0.075;
                    break;
                case GridInputMode.Turn:
                    int orientation = ((int) gridOrientation + intDelta) % 3;
                    orientation = (orientation < 0) ? 3 + orientation : orientation;
                    SetGridOrientation((GridOrientation) orientation);
                    WheelCD = 0.25;
                    break;
            }
        }

        protected void UpdateGridDepth() {
            Vector3 normal;
            Vector3 nonNormalAxis;
            switch (gridOrientation) {
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
            } float offset = OFFSET * (viewBehindQuad ? 1 : -1);
            gridQuad.Position = nonNormalAxis + normal * (depth + offset);
            plane = new Plane(normal, nonNormalAxis + normal * depth);
        }

        private void UpdateGridPos(SceneView sceneView) {
            if (Event.current.type == EventType.Repaint) {
                Vector3Int center = (gridSettings.followCamera ? sceneView.camera.transform.position
                                                           : Vector3.zero).Round();
                float offset = OFFSET * (this.viewBehindQuad ? 1 : -1);
                float depth = this.depth + offset;
                switch (gridOrientation) {
                    case GridOrientation.XZ:
                        gridQuad.Position = new Vector3(center.x, depth, center.z);
                        break;
                    case GridOrientation.XY:
                        gridQuad.Position = new Vector3(center.x, center.y, depth);
                        break;
                    case GridOrientation.YZ:
                        gridQuad.Position = new Vector3(depth, center.y, center.z);
                        break;
                } bool viewBehindQuad = CameraBehindQuad(sceneView);
                if (viewBehindQuad != this.viewBehindQuad) {
                    this.viewBehindQuad = viewBehindQuad;
                    UpdateGridDepth();
                }
            } 
        }

        protected void ClearHint() => HasHint = false;

        protected bool CameraBehindQuad(SceneView sceneView) => gridOrientation switch {
            GridOrientation.XZ
            => sceneView.camera.transform.position.y,
            GridOrientation.XY
            => sceneView.camera.transform.position.z,
            _
            => sceneView.camera.transform.position.x,
        } < depth;

        protected void ResetGridInput() {
            defaultInput = 0;
            overrideInput = 0;
        }
    }
}