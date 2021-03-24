using Unity.Entities;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;

[UpdateAfter(typeof(PlayerMoveSystem))]
public class PickupSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_BufferSystem;
    private BuildPhysicsWorld m_BuildPhysicsWorld;
    private StepPhysicsWorld m_StepPhysicsWorld;

    protected override void OnCreate()
    {
        m_BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        TriggerJob triggerJob = new TriggerJob
        {
            playerEntity = GetComponentDataFromEntity<PlayerTag>(),
            pickableEntity = GetComponentDataFromEntity<CollectableTag>(),
            waitDeleteEntity = GetComponentDataFromEntity<DeleteTag>(),
            commandBuffer = m_BufferSystem.CreateCommandBuffer()
        };

        // 由于TriggerJob涉及到Unity.Physics内的方法，需要传入物理系统的依赖，然TriggerJob执行在物理系统的基本逻辑之后
        Dependency = triggerJob.Schedule(m_StepPhysicsWorld.Simulation, ref m_BuildPhysicsWorld.PhysicsWorld, Dependency);
        // 当在Job的CommandBuffer执行了写的操作（比如AddComponent），就需要调用AddJobHandleForProducer(JobHandle)把当前System的JobHandle依赖加入缓存系统的依赖队列
        // 目前我们的Job集成自IJob，只是一个线程上运行，如果有集成自IJobParaller或者IJobChunk的，还要调用ToConcurrent()，具体调用方式可查阅官方文档
        // https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Entities.EntityCommandBufferSystem.html
        m_BufferSystem.AddJobHandleForProducer(Dependency);
    }

    private struct TriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<PlayerTag> playerEntity;
        [ReadOnly] public ComponentDataFromEntity<CollectableTag> pickableEntity;
        [ReadOnly] public ComponentDataFromEntity<DeleteTag> waitDeleteEntity;

        // SyncPoint（同步点）:当我们创建实体、销毁实体、对实体添加移除Component和改变SharedComponents的值几种操作时产生,因为它们都是StructualChanges（结构改变）
        // 同步点会导致你的程序先等待所有正在调度的Job执行完毕，以确保后续获得正确的结果
        // 此时我们的程序在这个等待期间就丧失了对Worker的调度能力(无法继续多线程操作了)
        // Buffer故名思意缓冲区，ECBSystem会将这类操作储存起来，各个Job在线程上继续走自己的安全逻辑，
        // 最终回到主线程的System中，按缓存顺序统一进行调用，原本每个Job会产生一次的同步点变成了一个（PS：即使是在主线程总，使用ECB也比使用EntityManager更好，可控时机）
        public EntityCommandBuffer commandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            TestEntityTrigger(triggerEvent.EntityA, triggerEvent.EntityB);
            TestEntityTrigger(triggerEvent.EntityB, triggerEvent.EntityA);
        }

        private void TestEntityTrigger(Entity entity1, Entity entity2)
        {
            if (playerEntity.HasComponent(entity1) && pickableEntity.HasComponent(entity2) && !waitDeleteEntity.HasComponent(entity2))
            {
                commandBuffer.AddComponent(entity2, new DeleteTag());
            }
        }
    }
}
