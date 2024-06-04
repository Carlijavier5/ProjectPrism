using UnityEngine;

[System.Serializable]
public class Cell {

    [SerializeField] private TileData tile;
    public TileData Tile => tile;

    [SerializeField] private Vector3 position;
    public Vector3 Position => position;

    [SerializeField] private Quaternion rotation;
    public Quaternion Rotation => rotation;

    [SerializeField] private LayerMask layer;
    public LayerMask Layer => layer;

    public Cell(Vector3 position) => this.position = position;

    public void Fill(TileData tile, Quaternion rotation, LayerMask layer) {
        this.tile = tile;
        this.rotation = rotation;
        this.layer = layer;
    }
    public void Clear() {
        tile = null;
        rotation = Quaternion.identity;
        layer = 0;
    }

    public void SetLayer(int layer) => this.layer = layer;
    public bool HasLayer(int layer) => (this.layer & layer) != 0;

    public static void Swap(Cell cell1, Cell cell2) {
        TileData tempTile = cell1.tile;
        cell1.tile = cell2.tile;
        cell2.tile = tempTile;

        Quaternion tempRotation = cell1.rotation;
        cell1.rotation = cell2.rotation;
        cell2.rotation = tempRotation;

        LayerMask tempLayer = cell1.layer;
        cell1.layer = cell2.layer;
        cell2.layer = tempLayer;
    }
}