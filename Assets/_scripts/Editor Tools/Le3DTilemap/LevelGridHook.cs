using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Le3DTilemap {
    public class LevelGridHook : MonoBehaviour {
        
        [HideInInspector]
        [SerializeField] private DataRootMap tileMap = new();
        [HideInInspector]
        [SerializeField] private WorldspaceCellMap worldspaceMap = new();

        [SerializeField] private TileData data;


        public void PlaceTile(Vector3Int position, Vector3Int rotation, TileData data) {
            SerializableHashSet<Vector3Int> tilespace = data.Info.Tilespace;
            foreach (Vector3Int localPosition in tilespace) {
                worldspaceMap[localPosition + position] = new(localPosition, null);
            }
        }

        /* public void PlaceTile(Vector3Int position) {
            distributionMap[data] = new();
            distributionMap[data].data.Add(new(null, null));
            levelMap[position] = new(Vector3Int.zero, null, null);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void RemoveTile(Vector3Int position) {
            levelMap.Remove(position);
            distributionMap.Remove(data);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void PrintTiles() {
            Debug.Log($"Entries: {levelMap.Count}\n" +
                      $"Tiles: {(distributionMap.Count > 0 ? distributionMap[data].data.Count : "null")}");
        }*/
    }
}