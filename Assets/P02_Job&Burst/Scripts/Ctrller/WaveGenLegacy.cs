using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveGenLegacy : MonoBehaviour
{
    [Header("Wave Parameters")]
    public float waveScale;
    public float waveOffsetSpeed;
    public float waveHeight;

    [Header("Mesh Parameters")]
    private Vector3[] m_WaterVertices;
    private Vector3[] m_WaterNormals;

    [Header("References and Prefabs")]
    public MeshFilter waterMeshFilter;
    private Mesh m_WaterMesh;


    void Start()
    {
        // 需要频繁更新网格时，调用MarkDynamic，利用底层图形接口的动态缓存器，提高效率
        m_WaterMesh = waterMeshFilter.mesh;
        m_WaterMesh.MarkDynamic();

        // 获取顶点和法线
        m_WaterVertices = m_WaterMesh.vertices;
        m_WaterNormals = m_WaterMesh.normals;
    }

    void Update()
    {
        for (int i = 0; i < m_WaterVertices.Length; i++)
        {
            if (m_WaterNormals[i].z > 0f)
            {
                float noiseValue = Mathf.PerlinNoise(
                    m_WaterVertices[i].x * waveScale + waveOffsetSpeed * Time.time,
                    m_WaterVertices[i].y * waveScale + waveOffsetSpeed * Time.time);
                m_WaterVertices[i] = new Vector3(m_WaterVertices[i].x, m_WaterVertices[i].y, noiseValue * waveHeight + 0.3f);
            }
        }

        m_WaterMesh.SetVertices(m_WaterVertices);
        m_WaterMesh.RecalculateNormals();
    }
}
