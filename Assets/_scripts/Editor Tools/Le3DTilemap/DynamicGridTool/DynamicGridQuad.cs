using UnityEngine;

namespace Le3DTilemap {
    public class DynamicGridQuad : MonoBehaviour {
        [SerializeField] private MeshRenderer meshRenderer;
        public MeshRenderer Renderer => meshRenderer;
    }
}