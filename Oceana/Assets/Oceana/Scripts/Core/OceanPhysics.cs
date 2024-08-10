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
using UnityEngine.Rendering;

namespace Oceana
{
    [RequireComponent(typeof(Ocean))]
    [DisallowMultipleComponent]
    public class OceanPhysics : Singleton<OceanPhysics>
    {
        private const int MAX_BODY_COUNT = 1024;
        private const int SIDE_RESOLUTION = 32;

        private const int COMPUTE_GROUP_X = 32;
        private const int COMPUTE_GROUP_Y = 32;
        private const int COMPUTE_GROUP_Z = 1;

        [SerializeField]
        private ComputeShader m_Compute;
        [SerializeField]
        private bool m_IsConstantParametres;

        private Ocean m_Ocean;

        private Transform[] m_Bodies = new Transform[MAX_BODY_COUNT];
        private Vector3[] m_Positions = new Vector3[MAX_BODY_COUNT];
        private ComputeBuffer m_PositionBuffer;
        
        private Vector3[] m_Impulses = new Vector3[MAX_BODY_COUNT];
        private Vector3[] m_UpVectors = new Vector3[MAX_BODY_COUNT];
        private ComputeBuffer m_ImpulseBuffer;
        private ComputeBuffer m_UpVectorBuffer;

        protected override void Awake()
        {
            m_Ocean = GetComponent<Ocean>();

            InitBuffers();
            SetBuffers();
            SetConstantParametres();

            base.Awake();
        }

        private void Start()
        {
            SetScrolls();
        }

        private void OnEnable()
        {
            m_Ocean.OnUpdated += UpdateBodies;
            m_Ocean.OnGneratorChanged += SetScrolls;
            m_Ocean.OnGneratorChanged += SetParametres;
        }

        private void OnDisable()
        {
            m_Ocean.OnUpdated -= UpdateBodies;
            m_Ocean.OnGneratorChanged -= SetScrolls;
            m_Ocean.OnGneratorChanged -= SetParametres;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
        }

        public bool AddBody(Transform body, out int id)
        {
            if(!m_PositionBuffer.IsValid())
            {
                id = -1;
                return false;
            }

            for(int i = 0; i < MAX_BODY_COUNT; i++)
            {
                if (m_Bodies[i] == null)
                {
                    m_Bodies[i] = body;
                    id = i;

                    return true;
                }
            }

            id = -1;
            return false;
        }

        public bool RemoveBody(int id)
        {
            if(id >= 0 && id < MAX_BODY_COUNT)
            {
                m_Bodies[id] = null;
                return true;
            }

            return false;
        }

        public bool UpdateBodyPosition(Transform body, int id)
        {
            if (id >= 0 || id < MAX_BODY_COUNT || m_Bodies[id] == body)
            {
                m_Positions[id] = body.position;
                return true;
            }

            return false;
        }

        public bool GetData(int id, out Vector3 impulse, out Vector3 upVector)
        {
            if(id >= 0 && id < MAX_BODY_COUNT)
            {
                impulse = m_Impulses[id];
                upVector = m_UpVectors[id];

                return true;
            }

            impulse = Vector3.zero;
            upVector = Vector3.zero;
            return false;
        }

        private void UpdateBodies()
        {
            if (!m_IsConstantParametres)
                SetParametres();

            ApplyBufferChanges();
            m_Compute.Dispatch(0, SIDE_RESOLUTION / COMPUTE_GROUP_X, SIDE_RESOLUTION / COMPUTE_GROUP_Y, COMPUTE_GROUP_Z);
            ReadDataAsync();
        }

        private void ReadDataAsync()
        {
            _ = AsyncGPUReadback.Request(m_ImpulseBuffer, OnReadImpulseCallback);
            _ = AsyncGPUReadback.Request(m_UpVectorBuffer, OnReadUpVectorCallback);
        }

        private void OnReadImpulseCallback(AsyncGPUReadbackRequest request)
        {

            if(request.done && !request.hasError)
            {
                var array = request.GetData<Vector3>();
                m_Impulses = array.ToArray();
            }
        }

        private void OnReadUpVectorCallback(AsyncGPUReadbackRequest request)
        {
            if (request.done && !request.hasError)
            {
                var array = request.GetData<Vector3>();
                m_UpVectors = array.ToArray();
            }
        }

        private void InitBuffers()
        {
            if(m_PositionBuffer != null && m_PositionBuffer.IsValid())
                m_PositionBuffer.Release();

            m_PositionBuffer = new ComputeBuffer(MAX_BODY_COUNT, sizeof(float) * 3, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            m_PositionBuffer.SetData(new Vector3[MAX_BODY_COUNT]);

            m_ImpulseBuffer = new ComputeBuffer(MAX_BODY_COUNT, sizeof(float) * 3, ComputeBufferType.Structured);
            m_ImpulseBuffer.SetData(m_Impulses);

            m_UpVectorBuffer = new ComputeBuffer(MAX_BODY_COUNT, sizeof(float) * 3, ComputeBufferType.Structured);
            m_UpVectorBuffer.SetData(m_UpVectors);
        }

        private void SetConstantParametres()
        {
            m_Compute.SetInt("Side", SIDE_RESOLUTION);
        }

        private void SetBuffers()
        {
            m_Compute.SetBuffer(0, "Positions", m_PositionBuffer);
            m_Compute.SetBuffer(0, "Impulses", m_ImpulseBuffer);
            m_Compute.SetBuffer(0, "UpVectors", m_UpVectorBuffer);
        }

        private void SetScrolls()
        {
            m_Ocean.Generator.SetScrolls(m_Compute, 0);
        }

        private void SetParametres()
        {
            m_Ocean.Generator.SetParametres(m_Compute);
        }

        private void Dispose()
        {
            m_PositionBuffer?.Dispose();
            m_ImpulseBuffer?.Dispose();
            m_UpVectorBuffer?.Dispose();
        }

        private void ApplyBufferChanges()
        {
            for (int i = 0; i < MAX_BODY_COUNT; i++)
            {
                if (m_Bodies[i] != null)
                    m_Positions[i] = m_Bodies[i].position;
                else
                    m_Positions[i] = Vector3.zero;
            }

            m_PositionBuffer.SetData(m_Positions);
        }
    }
}