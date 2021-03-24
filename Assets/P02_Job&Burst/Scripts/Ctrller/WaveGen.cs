using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS_2019
{
    public class WaveGen : MonoBehaviour
    {
        [Header("Wave Parameters")]
        public float waveScale;
        public float waveOffsetSpeed;
        public float waveHeight;

        [Header("Mesh Parameters")]
        private NativeArray<Vector3> m_WaterVertices;
        private NativeArray<Vector3> m_WaterNormals;

        [Header("References and Prefabs")]
        public MeshFilter waterMeshFilter;
        private Mesh m_WaterMesh;


        // job handle
        private JobHandle m_MeshModJobHandle;
        // job instance
        private WaveMeshJob m_MeshModJob;

        void Start()
        {
            // 需要频繁更新网格时，调用MarkDynamic，利用底层图形接口的动态缓存器，提高效率
            m_WaterMesh = waterMeshFilter.mesh;
            m_WaterMesh.MarkDynamic();

            // 声明顶点和法线的NativeContainer，因为是全局变量，用Persistent类型
            m_WaterVertices =
                new NativeArray<Vector3>(m_WaterMesh.vertices, Allocator.Persistent);
            m_WaterNormals =
                new NativeArray<Vector3>(m_WaterMesh.normals, Allocator.Persistent);
        }

        private void Update()
        {
            // 创建一个Job结构，传入结构体参数
            m_MeshModJob = new WaveMeshJob()
            {
                vertices = m_WaterVertices,
                normals = m_WaterNormals,
                offsetSpeed = waveOffsetSpeed,
                time = Time.time,
                scale = waveScale,
                height = waveHeight
            };

            // IJobParallel用于处理有数组结构变量的Job，用来将大规模的遍历拆分成多个块同步执行，在Schedule时需要传入遍历的总长度
            // 第二个参数batch count，参考官方文档描述，是根据数组长度以及Job执行的逻辑的负载量来决定，一般数组较长的情况下，innerloopBatchCount可设32~200，对于Job中计算特别复杂的，innerloopBatchCount为1
            m_MeshModJobHandle = m_MeshModJob.Schedule(m_WaterVertices.Length, 64);
        }

        private void LateUpdate()
        {
            // Schedule Early, Complete Late
            // 调用了Complete方法，代表主进程等待Job逻辑执行结束，此时数据是线程安全的，可以从Job中提取数据
            m_MeshModJobHandle.Complete();

            // 从Job线程中获取顶点数据
            m_WaterMesh.SetVertices(m_MeshModJob.vertices);
            // 更新法线信息
            m_WaterMesh.RecalculateNormals();
        }

        private void OnDestroy()
        {
            //手动回收，防止GC
            m_WaterVertices.Dispose();
            m_WaterNormals.Dispose();
        }
    }


    [BurstCompile]
    public struct WaveMeshJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        [ReadOnly] public NativeArray<Vector3> normals;

        public float offsetSpeed;
        public float scale;
        public float height;
        public float time;

        private float Noise(float x, float y)
        {
            float2 pos = math.float2(x, y);
            return noise.snoise(pos);
        }

        public void Execute(int index)
        {
            // 在模型空间，z轴朝上
            if (normals[index].z > 0f)
            {
                // 根据perlin噪音计算顶点的高度偏移
                var vertex = vertices[index];
                float noiseValue = Mathf.PerlinNoise(vertex.x * scale + offsetSpeed * time, vertex.y * scale + offsetSpeed * time);

                vertices[index] = new Vector3(vertex.x, vertex.y, noiseValue * height + 0.3f);
            }
        }
    }
}
