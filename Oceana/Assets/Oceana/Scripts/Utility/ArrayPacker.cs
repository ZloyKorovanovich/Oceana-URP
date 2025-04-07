using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ArrayPacker", menuName = "Tools/ArrayPacker")]
public class ArrayPacker : ScriptableObject {
    [SerializeField]
    private int m_Resolution = 1024;
    [SerializeField]
    private List<Texture2D> m_Maps = new List<Texture2D>();
    [SerializeField]
    private TextureFormat m_Format = TextureFormat.RGB24;
    [SerializeField]
    private string m_Path = "MapArray";

    [ContextMenu("Generate")]
    public void Generate() {
        Texture2DArray array = new Texture2DArray(m_Resolution, m_Resolution, m_Maps.Count, m_Format, true);
        for (int i = 0; i < m_Maps.Count; i++) {
            CopyToArrayMip(m_Maps[i], array, i);
        }

        AssetDatabase.CreateAsset(array, "Assets/" + m_Path + ".asset");
        AssetDatabase.SaveAssets();
    }

    private void CopyToArrayMip(Texture2D texture, Texture2DArray array, int index) {
        for(int mip = 0; mip < texture.mipmapCount; mip++) {
            int mipRes = m_Resolution >> mip;
            Graphics.CopyTexture(texture, 0, mip, 0, 0, mipRes, mipRes, array, index, mip, 0, 0);
        }
    }
}