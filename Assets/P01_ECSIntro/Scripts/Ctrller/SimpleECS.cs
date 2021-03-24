using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;

using Random = UnityEngine.Random;

namespace DOTS_2019
{
    public class SimpleECS : MonoBehaviour
    {
        [SerializeField] private Mesh m_CommonMesh;
        [SerializeField] private Material m_CommonMat;


        void Start()
        {
            Debug.Log(12312312);
            // 获取默认World的EntityManager
            var entityMgr = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 通过EntityManager创建Entity
            var e1 = entityMgr.CreateEntity();
            var e2 = entityMgr.CreateEntity(typeof(LifeTimeData));
            entityMgr.SetComponentData(e2, new LifeTimeData { existTime = 10 });


            EntityArchetype aType = entityMgr.CreateArchetype(
                typeof(Translation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(LocalToWorld),

                typeof(LifeTimeData),
                typeof(TransMoveData)
            );


            // var entity = entityMgr.CreateEntity(aType);

            // 实例化一个NativeArray
            NativeArray<Entity> eArray = new NativeArray<Entity>(50000, Allocator.Temp);
            entityMgr.CreateEntity(aType, eArray);

            for (int i = 0; i < eArray.Length; i++)
            {
                var entity = eArray[i];
                entityMgr.SetComponentData(entity, new LifeTimeData { existTime = 100 });
                entityMgr.SetComponentData(entity, new Scale { Value = 0.1f });
                entityMgr.SetComponentData(entity, new Translation { Value = new float3(Random.Range(-10f, 10f), Random.Range(-5f, 5f), 0) });
                entityMgr.SetSharedComponentData(entity, new RenderMesh { mesh = m_CommonMesh, material = m_CommonMat });
                entityMgr.SetComponentData(entity, new TransMoveData { speed = Random.Range(1f, 3f) });
            }

            // 手动回收
            eArray.Dispose();
        }

        void Update()
        {

        }
    }
}
