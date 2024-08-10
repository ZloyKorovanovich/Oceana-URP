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
    [RequireComponent(typeof(Rigidbody))]
    public class FloatingBody : MonoBehaviour
    {
        [SerializeField]
        private bool m_AlignWithNormal;

        private OceanPhysics m_Ocean;

        private Transform m_Transform;
        private Rigidbody m_Rigidbody;

        private bool m_Connected;
        private int m_Index;

        private void Start()
        {
            m_Transform = transform;
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Ocean = OceanPhysics.Instance;

            m_Connected = m_Ocean.AddBody(transform, out m_Index);
        }

        private void FixedUpdate()
        {
            if(!m_Connected)
                return;

            _ = m_Ocean.GetData(m_Index, out var imp, out var up);
            _ = m_Ocean.UpdateBodyPosition(transform, m_Index);
            
            m_Rigidbody.AddForce(imp);
            if(m_AlignWithNormal)
                m_Transform.up = up;
        }
    }
}