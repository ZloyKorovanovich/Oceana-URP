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
    public class OceanMesh
    {
        private const int COMPUTE_GROUP_X = 32;
        private const int COMPUTE_GROUP_Y = 32;
        private const int COMPUTE_GROUP_Z = 1;

        [SerializeField]
        private ComputeShader m_Compute;
        [SerializeField]
        private MeshResolution m_Resolution = MeshResolution.res_128x128;
        [SerializeField]
        private float m_MeshScale = 2000f;

        [SerializeField, Range(0f, 1f)]
        private float m_DisplaceStrength = 1f;

        public enum MeshResolution : int
        {
            res_32x32 = 32,
            res_64x64 = 64,
            res_128x128 = 128,
            res_256x256 = 256
        }

        public void GenerateMesh(out Mesh mesh)
        {
            var vertices = new Vector3[(int)m_Resolution * (int)m_Resolution];
            var triangles = new int[((int)m_Resolution - 1) * ((int)m_Resolution - 1) * 6];
            var uvs = new Vector2[(int)m_Resolution * (int)m_Resolution];

            var vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
            var triangleBuffer = new ComputeBuffer(triangles.Length, sizeof(int));
            var uvBuffer = new ComputeBuffer(uvs.Length, sizeof(float) * 2);

            toCompute();
            dispatch();
            finalize();

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();


            void toCompute()
            {
                vertexBuffer.SetData(vertices);
                triangleBuffer.SetData(triangles);
                uvBuffer.SetData(uvs);

                m_Compute.SetBuffer(0, "Vertices", vertexBuffer);
                m_Compute.SetBuffer(0, "Triangles", triangleBuffer);
                m_Compute.SetBuffer(0, "UVs", uvBuffer);

                m_Compute.SetInt("TriResolution", (int)m_Resolution - 1);
                m_Compute.SetFloat("Scale", m_MeshScale);

                m_Compute.SetFloat("DisplaceStrength", m_DisplaceStrength);
            }

            void dispatch()
            {
                m_Compute.Dispatch(0, (int)m_Resolution / COMPUTE_GROUP_X, (int)m_Resolution / COMPUTE_GROUP_Y, COMPUTE_GROUP_Z);
            }

            void finalize()
            {
                vertexBuffer.GetData(vertices);
                triangleBuffer.GetData(triangles);
                uvBuffer.GetData(uvs);

                vertexBuffer.Release();
                triangleBuffer.Release();
                uvBuffer.Release();
            }
        }
    }
}