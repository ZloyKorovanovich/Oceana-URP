using UnityEditor;
using UnityEngine;

namespace Oceana {
    [CreateAssetMenu(fileName = "SurfaceGenerator", menuName = "Tools/SurfaceGenerator")]
    public class SurfaceGenerator : ScriptableObject {
        private const int k_KernelID = 0;
        private const int k_GroupSizeXY = 8;

        [SerializeField]
        private ComputeShader m_Shader;
        [SerializeField]
        private int m_ResolutionX = 1024; // vertex resolution
        [SerializeField]
        private int m_ResolutionY = 1024; // vertex resolution
        [SerializeField]
        private string m_SavePath = "WaterMesh";

        private ComputeBuffer m_VertexBuffer;
        private ComputeBuffer m_IndexBuffer;

        [ContextMenu("Generate")]
        public void Generate() {
            if (m_Shader == null) return;

            Mesh mesh = new Mesh() {
                bounds = new Bounds(Vector3.zero, Vector3.one)
            };

            Vector3[] vertices = new Vector3[m_ResolutionX * m_ResolutionY];
            int[] indices = new int[(m_ResolutionX - 1) * (m_ResolutionY - 1) * 6];

            m_VertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);
            m_IndexBuffer = new ComputeBuffer(indices.Length, sizeof(int), ComputeBufferType.Structured);
            m_VertexBuffer.SetData(vertices);
            m_IndexBuffer.SetData(indices);

            m_Shader.SetInt("_ResolutionX", m_ResolutionX);
            m_Shader.SetInt("_ResolutionY", m_ResolutionY);
            m_Shader.SetBuffer(k_KernelID, "_Vertices", m_VertexBuffer);
            m_Shader.SetBuffer(k_KernelID, "_Indices", m_IndexBuffer);

            m_Shader.Dispatch(k_KernelID, m_ResolutionX / k_GroupSizeXY, m_ResolutionY / k_GroupSizeXY, 1);

            m_VertexBuffer.GetData(vertices);
            m_IndexBuffer.GetData(indices);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);


            for (int i = 0; i < vertices.Length; i++) {
                Debug.Log(vertices[i]);
            }

            for (int i = 0; i < indices.Length; i++) {
                Debug.Log(indices[i]);
            }

            m_VertexBuffer.Release();
            m_IndexBuffer.Release();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            SaveMesh(mesh);
        }

        private void SaveMesh(Mesh mesh) {
            AssetDatabase.CreateAsset(mesh, "Assets/" + m_SavePath + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
}