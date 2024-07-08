using UnityEngine;

namespace Le3DTilemap {
    [System.Serializable]
    public class Cell {

        public Vector3Int localPosition;
        public TileInfo info;
        public TileInstanceData Data => info.InstanceData;

        public Cell(Vector3Int localPosition, TileInfo info) {
            this.localPosition = localPosition;
            this.info = info;
        }
    }

    [System.Serializable]
    public class TileInstanceData {

        public TileData data;
        public GameObject gameObject;

        public Vector3Int position;
        public Vector3Int tileRotation;
        public Quaternion meshRotation;

        public TileInstanceData(Vector3Int position, Vector3Int tileRotation,
                                GameObject gameObject, TileData data) {
            this.position = position;
            this.tileRotation = tileRotation;
            this.gameObject = gameObject;
            this.data = data;
        }
    }
}