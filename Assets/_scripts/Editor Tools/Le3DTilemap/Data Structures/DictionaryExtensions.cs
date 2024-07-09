using System.Collections.Generic;
using UnityEngine;

namespace Le3DTilemap {
    [System.Serializable]
    public class RootMapStorage : SerializableDictionary.Storage<SerializableHashSet<TileInfo>> {
        public RootMapStorage() => data = new();
    }

    [System.Serializable]
    public class DataRootMap : SerializableDictionary<TileData, RootMapStorage> { }

    [System.Serializable]
    public class WorldspaceCellMap : SerializableDictionary<Vector3Int, Cell> { }
}