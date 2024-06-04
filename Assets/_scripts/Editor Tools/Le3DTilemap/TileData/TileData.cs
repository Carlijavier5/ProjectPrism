using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public class TileData : ScriptableObject {

    [SerializeField] private GameObject prefab;
    public GameObject Prefab { 
        get => prefab;
        set { prefab = value;
              GetPreviewAsync(); }
    }

    [HideInInspector]
    [SerializeField] private Texture2D preview;
    public Texture2D Preview {
        get {
            if (preview == null) GetPreviewAsync();
            return preview;
        }
    }

    public async void GetPreviewAsync() {
        while (preview == null) {
            if (prefab == null) return;
            preview = AssetPreview.GetAssetPreview(prefab);
            await Task.Yield();
        }
    }
}

[CustomEditor(typeof(TileData))]
public class TileDataEditor : Editor {

    private TileData TileData => target as TileData;

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
        return TileData.Preview == null ? base.RenderStaticPreview(assetPath, subAssets, width, height)
                                        : TileData.Preview;
    }
}