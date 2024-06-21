using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Le3DTilemap {
    public partial class TileColliderTool {

        private const float OFFSET = 0.5f;

        private void DrawTileDistribution() {
            switch (settings.drawDistributionScope) {
                case DrawDistributionScope.Selection:
                    if (Info.SelectedCollider != null) {
                        DrawTileDistribution(Info.transform
                                             .TransformPoint(Info.SelectedCollider.Center),
                                             Info.SelectedCollider.Size);
                    } break;
                case DrawDistributionScope.Colliders:
                    foreach (TileCollider collider in Info.Colliders) {
                        DrawTileDistribution(Info.transform
                                             .TransformPoint(collider.Center), 
                                             collider.Size);
                    } break;
                case DrawDistributionScope.Tilespace:
                    DrawTileDistribution(Info.Tilespace);
                    break;
            }
        }

        private void DrawTileDistribution(IEnumerable<Vector3Int> tilespace) {
            System.Span<Vector3> span = new System.Span<Vector3>(tilespace.Select(
                                                                 (vec) => (Vector3) vec)
                                                                 .ToArray());
            Info.transform.TransformPoints(span);
            if (Event.current.type == EventType.Repaint) {
                Handles.color = Color.white;
                foreach (Vector3 position in span) {
                    Handles.DrawWireCube(position.Round(), Vector3Int.one);
                }
            }
        }

        private void DrawTileDistribution(Vector3 center, Vector3Int size) {
            if (Event.current.type == EventType.Repaint) {
                Handles.color = Color.white;
                switch (settings.drawDistributionMode) {
                    case DrawDistributionMode.Full:
                        for (int x = Mathf.RoundToInt(center.x - size.x / 2f + OFFSET);
                             x <= Mathf.RoundToInt(center.x + size.x / 2f - OFFSET); x++) {
                            for (int y = Mathf.RoundToInt(center.y - size.y / 2f + OFFSET);
                                 y <= Mathf.RoundToInt(center.y + size.y / 2f - OFFSET); y++) {
                                for (int z = Mathf.RoundToInt(center.z - size.z / 2f + OFFSET);
                                     z <= Mathf.RoundToInt(center.z + size.z / 2f - OFFSET); z++) {
                                    Handles.DrawWireCube(new Vector3(x, y, z), Vector3Int.one);
                                }
                            }
                        } break; 
                    case DrawDistributionMode.Bounds:
                        if (size == Vector3Int.one) {
                            Handles.DrawWireCube(center, size);
                            return;
                        }

                        if (size.x > 1) {
                            Vector3 sizeYZ = new Vector3(1, size.y, size.z);
                            for (int x = Mathf.RoundToInt(center.x - size.x / 2f + OFFSET);
                                 x <= Mathf.RoundToInt(center.x + size.x / 2f - OFFSET); x++) {
                                Vector3 centerX = new Vector3(x, center.y, center.z);
                                Handles.DrawWireCube(centerX, sizeYZ);
                            }
                        }

                        if (size.y > 1) {
                            Vector3 sizeXZ = new Vector3(size.x, 1, size.z);
                            for (int y = Mathf.RoundToInt(center.y - size.y / 2f + OFFSET);
                                 y <= Mathf.RoundToInt(center.y + size.y / 2f - OFFSET); y++) {
                                Vector3 centerY = new Vector3(center.x, y, center.z);
                                Handles.DrawWireCube(centerY, sizeXZ);
                            }
                        }

                        if (size.z > 1) {
                            Vector3 sizeXY = new Vector3(size.x, size.y, 1);
                            for (int z = Mathf.RoundToInt(center.z - size.z / 2f + OFFSET);
                                 z <= Mathf.RoundToInt(center.z + size.z / 2f - OFFSET); z++) {
                                Vector3 centerZ = new Vector3(center.x, center.y, z);
                                Handles.DrawWireCube(centerZ, sizeXY);
                            }
                        } break;
                }
            }
        }
    }
}