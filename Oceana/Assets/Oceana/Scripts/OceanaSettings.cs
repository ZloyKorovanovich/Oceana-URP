using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Oceana {
    [CreateAssetMenu(fileName = "OceanaSettings", menuName = "Oceana/Settings")]
    public class OceanaSettings : ScriptableObject {
        #region SCROLL
        public enum MapResolution {
            Res_512x512 = 512,
            Res_1024x1024 = 1024,
            Res_2048x2048 = 2048,
            Res_4096x4096 = 4096
        }

        public ComputeShader ScrollShader;
        public Texture2DArray ScrollArray;
        public Vector4[] ScrollArrayST;
        public MapResolution ScrollResolution = MapResolution.Res_1024x1024;
        public GraphicsFormat ScrollFormat = GraphicsFormat.R16G16B16A16_SFloat;

        private void ClampSTArray() {
            if (ScrollArrayST.Length != ScrollArray.depth) {
                Vector4[] tmp = new Vector4[ScrollArray.depth];
                int minLen = Mathf.Min(ScrollArray.depth, ScrollArrayST.Length);

                for (int i = 0; i < minLen; i++) {
                    tmp[i] = ScrollArrayST[i];
                }
                for (int j = minLen; j < ScrollArray.depth; j++) {
                    tmp[j] = new Vector4(1, 1, 0, 0);
                }
            }
        }
        #endregion

        #region SURFACE
        public float SeaLevel = 0;
        public float DisplaceHeight = 1;
        public Vector4 ScrollST = new Vector4(1, 1, 0, 0);
        public Material SurfaceMaterial;
        public Mesh SurfaceMesh;
        #endregion

        #region UNDERWATER
        public Material UnderwaterMaterial;
        #endregion

        public Action OnUpdate;
        private void OnValidate() {
            ClampSTArray();
            OnUpdate?.Invoke();
        }
    }
}