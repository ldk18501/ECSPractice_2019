using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class DeleteEntitySystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem m_BufferSystem;
    protected override void OnCreate()
    {
        m_BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        // 在System中涉及到StructChange，需要用ECB等到主线程处理
        var commandBuffer = m_BufferSystem.CreateCommandBuffer();
        Entities.WithAll<DeleteTag>().WithoutBurst().ForEach((Entity entity) =>
        {
            GameMgr.AddScore();
            commandBuffer.DestroyEntity(entity);
        }).Schedule();
        m_BufferSystem.AddJobHandleForProducer(Dependency);

        // 官方写法
        // using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
        // {
        //     Entities.WithAll<DeleteTag>().WithoutBurst().ForEach((Entity entity) =>
        //     {
        //         GameMgr.AddScore();
        //         commandBuffer.DestroyEntity(entity);
        //     }).Run();

        //     commandBuffer.Playback(EntityManager);
        // }
    }
}
