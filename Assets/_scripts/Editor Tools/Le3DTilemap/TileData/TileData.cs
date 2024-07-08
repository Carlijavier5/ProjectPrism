using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Le3DTilemap {
    public class TileData : ScriptableObject {

        [SerializeField] private GameObject prefab;
        public GameObject Prefab {
            get => prefab;
            set {
                if (prefab != value) {
                    prefab = value;
                    hashVersion++;
                } info = prefab.GetComponentInChildren<TileInfo>();
                GetPreviewAsync();
            }
        }

        [SerializeField] private TileInfo info;
        public TileInfo Info => info;

        private int hashVersion;
        public int TileHashVersion => hashVersion;
        public int PrefabHashVersion => info == null ? -1 : info.HashVersion;

        public bool IsValid => prefab != null
                            && info != null
                            && info.IsValid;

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
}