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
    [RequireComponent(typeof(Ocean))]
    public class OceanRenderer : MonoBehaviour
    {
        [SerializeField]
        private Material m_Material;
        [SerializeField]
        private Material m_FullScreen;
        [SerializeField]
        private OceanMesh m_Mesh = new();
        [SerializeField]
        private bool m_IsConstantParametres;

        private Ocean m_Ocean;

        private MeshFilter m_MeshFilter;
        private MeshRenderer m_MeshRenderer;

        public Transform Spectator { get; private set; }

        private void Awake()
        {
            m_Ocean = GetComponent<Ocean>();

            m_MeshFilter = gameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();

            m_Mesh.GenerateMesh(out var mesh);

            m_MeshFilter.mesh = mesh;
            m_MeshRenderer.material = m_Material;
        }

        private void Start()
        {
            if(Spectator == null)
                Spectator = Camera.main.transform;

            SetScrolls();
        }

        private void OnEnable()
        {
            m_Ocean.OnGneratorChanged += SetScrolls;
            m_Ocean.OnGneratorChanged += SetParametres;
        }

        private void OnDisable()
        {
            m_Ocean.OnGneratorChanged -= SetScrolls;
            m_Ocean.OnGneratorChanged += SetParametres;
        }

        private void FixedUpdate()
        {
            if(Spectator == null)
                return;

            transform.position = new Vector3(Spectator.position.x, 0f, Spectator.position.z);
            if (!m_IsConstantParametres)
                SetParametres();
        }

        private void SetScrolls()
        {
            var generator = m_Ocean.Generator;
            if (generator)
            {
                generator.SetScrolls(m_Material);
                generator.SetScrolls(m_FullScreen);

                return;
            }
        }

        private void SetParametres()
        {
            var generator = m_Ocean.Generator;
            if (generator)
            {
                generator.SetParametres(m_Material);
                generator.SetParametres(m_FullScreen);
            }
        }
    }
}