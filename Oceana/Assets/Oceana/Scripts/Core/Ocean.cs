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
    [DisallowMultipleComponent]
    public class Ocean : MonoBehaviour
    {
        [SerializeField]
        private OceanGenerator m_Generator;

        private Action m_OnGneratorChanged;
        private Action m_OnUpdated;
        private OceanGenerator m_TempGnerator;

        public event Action OnGneratorChanged
        {
            add => m_OnGneratorChanged += value;
            remove => m_OnGneratorChanged -= value;
        }

        public event Action OnUpdated
        {
            add => m_OnUpdated += value;
            remove => m_OnUpdated -= value;
        }

        public OceanGenerator Generator => m_Generator;

        private void Awake()
        {
            if (!CheckGenerator())
                return;

            m_Generator.InitScrolls();
        }

        private void FixedUpdate()
        {
            if (!CheckGenerator()) 
                return;

            m_Generator.Gnerate(Time.fixedTime);
            m_OnUpdated?.Invoke();
        }

        private void OnValidate()
        {
            _ = CheckGenerator();
        }

        private bool CheckGenerator()
        {
            if (m_Generator != m_TempGnerator)
            {
                m_OnGneratorChanged?.Invoke();
                m_TempGnerator = m_Generator;
            }

            if (m_Generator)
                return true;

            return false;
        }
    }
}