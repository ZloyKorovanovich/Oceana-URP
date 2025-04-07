using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MapPacker", menuName = "Tools/MapPacker")]
public class MapPacker : ScriptableObject {
    private const int k_ComputeKernel = 0;
    private const int k_ComputeGroup = 32;

    [SerializeField]
    private ComputeShader m_Compute;
    [SerializeField]
    private Texture2D m_NormalMap;
    [SerializeField]
    private Texture2D m_HeightMap;
    [SerializeField]
    private int m_Resolution = 2048;
    [SerializeField]
    private string m_Path = "PackMap";
    [SerializeField]
    private bool m_Active = false;

    [ContextMenu("Generate")]
    public void Generate() {
        if(!m_Active || m_Compute == null || m_NormalMap == null || m_HeightMap == null) return;

        RenderTexture packMapRT = new RenderTexture(m_Resolution, m_Resolution, 16) {
            format = RenderTextureFormat.ARGB32,
            enableRandomWrite = true,
            useMipMap = false,
            autoGenerateMips = false
        };
        packMapRT.Create();

        m_Compute.SetTexture(k_ComputeKernel, "_NormalMap", m_NormalMap);
        m_Compute.SetTexture(k_ComputeKernel, "_HeightMap", m_HeightMap);
        m_Compute.SetTexture(k_ComputeKernel, "_PackMap", packMapRT);

        m_Compute.Dispatch(k_ComputeKernel, m_Resolution / k_ComputeGroup, m_Resolution / k_ComputeGroup, 1);

        SaveRenderTexturePNG(packMapRT);
        packMapRT.Release();
    }

    private void SaveRenderTexturePNG(RenderTexture renderTexture) {
        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();

        string path = m_Path + ".png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
    }
}