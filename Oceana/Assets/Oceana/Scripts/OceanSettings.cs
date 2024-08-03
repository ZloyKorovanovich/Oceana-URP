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
    public class OceanSettings : ScriptableObject
    {
        [Header("Map ST")]
        [SerializeField]
        private Vector4 m_C0_ST = new(0.08f, 0.08f, 0f, 0f);
        [SerializeField]
        private Vector4 m_C1_ST = new(0.06f, 0.06f, 0f, 0f);
        [SerializeField]
        private Vector4 m_C2_ST = new(0.02f, 0.02f, 0f, 0f);

        [Header("Height")]
        [SerializeField]
        private float m_C0_Height = 2f;
        [SerializeField]
        private float m_C1_Height = 2f;
        [SerializeField]
        private float m_C2_Height = 5f;

        private Action m_OnChanged;

        public event Action OnChanged
        {
            add => m_OnChanged += value;
            remove => m_OnChanged -= value;
        }

        private void OnValidate()
        {
            m_OnChanged?.Invoke();
        }

        public void SetParametres(Material material)
        {
            material.SetFloat("_Height_0", m_C0_Height);
            material.SetFloat("_Height_1", m_C1_Height);
            material.SetFloat("_Height_2", m_C2_Height);

            material.SetVector("_Scroll_0_ST", m_C0_ST);
            material.SetVector("_Scroll_1_ST", m_C1_ST);
            material.SetVector("_Scroll_2_ST", m_C2_ST);
        }
    }
}