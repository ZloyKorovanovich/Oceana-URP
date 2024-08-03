/*
Copyright 2024 Mitrofan Juryev

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using UnityEngine;

namespace Oceana
{
    [Serializable]
    public class OceanScroll
    {
        [Header("Maps")]

        [SerializeField]
        private Texture2D m_Height_0;
        [SerializeField]
        private Texture2D m_Height_1;

        [SerializeField]
        private Texture2D m_Normal_0;
        [SerializeField]
        private Texture2D m_Normal_1;

        [Header("ST")]

        [SerializeField]
        private Vector2 m_Speed_0;
        [SerializeField]
        private Vector2 m_Speed_1;

        public RenderTexture Scroll { get; private set; }

        public void Init(int resolution)
        {
            Release();
            Scroll = new RenderTexture(resolution, resolution, 32, RenderTextureFormat.ARGB64)
            {
                useMipMap = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Repeat,
                enableRandomWrite = true
            };

            Scroll.Create();
        }

        public void Release()
        {
            if (Scroll)
                Scroll.Release();
        }

        public void SetTextures(ComputeShader shader, int kernel)
        {
            shader.SetTexture(kernel, "HeightMap_0", m_Height_0);
            shader.SetTexture(kernel, "HeightMap_1", m_Height_1);

            shader.SetTexture(kernel, "NormalMap_0", m_Normal_0);
            shader.SetTexture(kernel, "NormalMap_1", m_Normal_1);

            shader.SetTexture(kernel, "Scroll", Scroll);
        }

        public void SetParametres(ComputeShader shader, float time)
        {
            var packedSpeed = new Vector4(m_Speed_0.x, m_Speed_0.y, m_Speed_1.x, m_Speed_1.y);
            shader.SetVector("Maps_ST", packedSpeed * time);
        }
    }
}