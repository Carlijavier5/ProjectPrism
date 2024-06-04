using UnityEngine;

[System.Serializable]
public class Array1D<T> {

    [HideInInspector]
    [SerializeField] private T[] cells;
    public T this[int z] {
        get { return cells[z]; }
        set { cells[z] = value; }
    }

    public int Size => cells.Length;

    public Array1D(int size) {
        cells = new T[size];
    }
}

[System.Serializable]
public class Matrix2D<T> {

    [HideInInspector]
    [SerializeField] private Array1D<T>[] lines;
    public T this[int x, int z] {
        get { return lines[x][z]; }
        set { lines[x][z] = value; }
    }

    [HideInInspector]
    [SerializeField] private Vector2Int size;
    public Vector2Int Size => size;

    public Matrix2D(int x, int z) {
        lines = new Array1D<T>[x];
        for (int i = 0; i < x; i++) {
            lines[i] = new Array1D<T>(z);
        }
        size = new Vector2Int(x, z);
    }

    public Matrix2D(Vector2Int size) => new Matrix2D<T>(size.x, size.y);
    public Matrix2D(int size) => new Matrix2D<T>(size, size);
}

[System.Serializable]
public class Matrix3D<T> {

    [HideInInspector]
    [SerializeField] private Matrix2D<T>[] planes;
    public T this[int x, int y, int z] {
        get { return planes[y][x, z]; }
        set { planes[y][x, z] = value; }
    }
    public T this[Vector3Int pos] {
        get { return planes[pos.y][pos.x, pos.z]; }
        set { planes[pos.y][pos.x, pos.z] = value; }
    }

    [HideInInspector]
    [SerializeField] private Vector3Int size;
    public Vector3Int Size => size;

    public Matrix3D(Vector3Int size) {
        planes = new Matrix2D<T>[size.y];
        for (int i = 0; i < size.y; i++) {
            planes[i] = new Matrix2D<T>(size.x, size.z);
        }
        this.size = size;
    }

    public Matrix3D(int size) => new Matrix3D<T>(Vector3Int.one * size);
}