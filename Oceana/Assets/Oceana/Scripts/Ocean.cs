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

using UnityEngine;

namespace Oceana
{
    [DisallowMultipleComponent]
    public class Ocean : MonoBehaviour
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

        public RenderTexture Scroll_0 => m_Scroll_0.Scroll;
        public RenderTexture Scroll_1 => m_Scroll_1.Scroll;
        public RenderTexture Scroll_2 => m_Scroll_2.Scroll;

        public enum ScrollResolution : int
        {
            res_512x512 = 512,
            res_1024x1024 = 1024,
            res_2048x2048 = 2048
        }

        private void FixedUpdate()
        {
            RenderScroll(m_Scroll_0, Time.time, 0);
            RenderScroll(m_Scroll_1, Time.time, 0);
            RenderScroll(m_Scroll_2, Time.time, 0);
        }

        private void RenderScroll(OceanScroll scroll, float time, int kernel)
        {
            scroll.SetTextures(m_Compute, kernel);
            scroll.SetParametres(m_Compute, time);

            m_Compute.Dispatch(kernel, (int)m_Resolution / COMPUTE_GROUP_X, (int)m_Resolution / COMPUTE_GROUP_Y, COMPUTE_GROUP_Z);

            scroll.Scroll.GenerateMips();
        }

        private void OnEnable()
        {
            m_Scroll_0.Init((int)m_Resolution);
            m_Scroll_1.Init((int)m_Resolution);
            m_Scroll_2.Init((int)m_Resolution);

            m_Compute.SetInt("Resolution", (int)m_Resolution);
        }

        private void OnDisable()
        {
            m_Scroll_0.Release();
            m_Scroll_1.Release();
            m_Scroll_2.Release();
        }
    }
}