using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Le3DTilemap {
    public class TilePalette3D : ScriptableObject {

        [SerializeField] private List<TileData> tiles = new();
        public List<TileData> Tiles { get => tiles ??= new(); }
        public int Count => tiles.Count;

        /// <summary> Add tile to palette; </summary>
        /// <returns> True if tile was NOT in the list; </returns>
        public bool Add(TileData tileData) {
            if (!Tiles.Contains(tileData)) {
                tiles.Add(tileData);
                return true;
            } return false;
        }

        /// <summary> Insert tile to palette; </summary>
        /// <returns> True if tile was NOT in the list; </returns>
        public bool Insert(int index, TileData tileData) {
            if (!Tiles.Contains(tileData)) {
                tiles.Insert(index, tileData);
                return true;
            } return false;
        }

        public bool Remove(TileData tileData) => Tiles.Remove(tileData);
    }
}