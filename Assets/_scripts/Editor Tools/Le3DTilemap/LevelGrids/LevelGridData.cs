using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Le3DTilemap {
        public class LevelGridData : ScriptableObject {

        [SerializeField] private Tilemap3D<TileData> tilemap;
        public Tilemap3D<TileData> Tilemap => tilemap;

        [SerializeField] private int baseScale = 1;

        public void GenerateGrid(int size) => GenerateGrid(Vector3Int.one * size);
        public void GenerateGrid(Vector3Int size) {
            tilemap = new Tilemap3D<TileData>(size);
            EditorUtility.SetDirty(this);
        }

        public Cell<TileData> InAdjacent(Vector3 center, Vector3 normal) {
            center /= baseScale;
            return tilemap.UnscaledInAdjacent(center.Round(), normal);
        }

        public Vector3Int WorldToCell(Vector3 position, int height) {
            Vector3Int cellCenter = new(Mathf.RoundToInt(position.x), height,
                                        Mathf.RoundToInt(position.z));
            return cellCenter;
        }

        void Reset() => baseScale = 1;
    }
}