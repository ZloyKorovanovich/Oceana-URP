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
    [CreateAssetMenu(fileName = "new OceanSettings", menuName = "Oceana/OceanSettings")]
    public class OceanGenerator : ScriptableObject
    {
        private const int COMPUTE_GROUP_X = 32;
        private const int COMPUTE_GROUP_Y = 32;
        private const int COMPUTE_GROUP_Z = 1;

        [SerializeField]
        private ComputeShader m_Compute;

        [SerializeField]
        private ScrollResolution m_Resolution = ScrollResolution.res_1024x1024;

        [SerializeField]
        private OceanScroll m_Scroll_0 = new();
        [SerializeField]
        private OceanScroll m_Scroll_1 = new();
        [SerializeField]
        private OceanScroll m_Scroll_2 = new();
        [SerializeField]
        private OceanScroll m_Scroll_3 = new();

        private Action m_OnChanged;

        public event Action OnChanged
        {
            add => m_OnChanged += value;
            remove => m_OnChanged -= value;
        }

        public enum ScrollResolution : int
        {
            res_512x512 = 512,
            res_1024x1024 = 1024,
            res_2048x2048 = 2048
        }

        private void OnValidate()
        {
            m_OnChanged?.Invoke();
        }

        private void OnDestroy()
        {
            ReleaseScrolls();
        }

        public void SetScrolls(Material material)
        {
            material.SetTexture("_Scroll_0", m_Scroll_0.Scroll);
            material.SetTexture("_Scroll_1", m_Scroll_1.Scroll);
            material.SetTexture("_Scroll_2", m_Scroll_2.Scroll);
            material.SetTexture("_Scroll_3", m_Scroll_3.Scroll);
        }

        public void SetScrolls(ComputeShader shader, int kernel)
        {
            shader.SetTexture(kernel, "Scroll_0", m_Scroll_0.Scroll);
            shader.SetTexture(kernel, "Scroll_1", m_Scroll_1.Scroll);
            shader.SetTexture(kernel, "Scroll_2", m_Scroll_2.Scroll);
            shader.SetTexture(kernel, "Scroll_3", m_Scroll_3.Scroll);
        }

        public void SetParametres(Material material)
        {
            var heightPack = new Vector4(m_Scroll_0.Height, m_Scroll_1.Height, m_Scroll_2.Height, m_Scroll_3.Height);
            material.SetVector("_ScrollHeights", heightPack);

            material.SetVector("_Scroll_0_ST", m_Scroll_0.ST);
            material.SetVector("_Scroll_1_ST", m_Scroll_1.ST);
            material.SetVector("_Scroll_2_ST", m_Scroll_2.ST);
            material.SetVector("_Scroll_3_ST", m_Scroll_3.ST);
        }

        public void SetParametres(ComputeShader shader)
        {
            var heightPack = new Vector4(m_Scroll_0.Height, m_Scroll_1.Height, m_Scroll_2.Height, m_Scroll_3.Height);
            shader.SetVector("ScrollHeights", heightPack);

            shader.SetVector("Scroll_0_ST", m_Scroll_0.ST);
            shader.SetVector("Scroll_1_ST", m_Scroll_1.ST);
            shader.SetVector("Scroll_2_ST", m_Scroll_2.ST);
            shader.SetVector("Scroll_3_ST", m_Scroll_3.ST);
        }

        [ContextMenu("InitScrolls")]
        public void InitScrolls()
        {
            m_Scroll_0.Init((int)m_Resolution);
            m_Scroll_1.Init((int)m_Resolution);
            m_Scroll_2.Init((int)m_Resolution);
            m_Scroll_3.Init((int)m_Resolution);

            m_Compute.SetInt("Resolution", (int)m_Resolution);
        }

        public void Gnerate(float time)
        {
            RenderScroll(m_Scroll_0, time, 0);
            RenderScroll(m_Scroll_1, time, 0);
            RenderScroll(m_Scroll_2, time, 0);
            RenderScroll(m_Scroll_3, time, 0);
        }

        private void RenderScroll(OceanScroll scroll, float time, int kernel)
        {
            scroll.SetGenerationSource(m_Compute, kernel);
            scroll.SetGenerationInput(m_Compute, time);

            m_Compute.Dispatch(kernel, (int)m_Resolution / COMPUTE_GROUP_X, (int)m_Resolution / COMPUTE_GROUP_Y, COMPUTE_GROUP_Z);

            scroll.Scroll.GenerateMips();
        }

        private void ReleaseScrolls()
        {
            m_Scroll_0.Release();
            m_Scroll_1.Release();
            m_Scroll_2.Release();
            m_Scroll_3.Release();
        }
    }
}