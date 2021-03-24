using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Jobs;
using UnityEngine;

public class LoopRotateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        // 当一个System中有多个Job的时候，显式的为Dependency赋值来设置依赖关系，决定他们的执行顺序
        // ScheduleParallel不传入依赖时，会自动设置前后依赖关系，如果后续没有其他System控制相关的Entity的情况下没有问题，否则会导致下一个SystemBase出错
        // 深入浅出 SystemBase: https://zhuanlan.zhihu.com/p/252858463
        var jbh1 = Entities.WithAll<LoopRotateTag>().WithNone<Parent>().ForEach((ref Rotation rot, in RotateData rotData) =>
        {
            rot.Value = math.mul(rot.Value, quaternion.RotateY(math.radians(rotData.angularSpeed * dt)));
        }).ScheduleParallel(Dependency);

        var jbh2 = Entities.WithAll<LoopRotateTag, Parent>().ForEach((ref Rotation rot, in RotateData rotData) =>
        {
            rot.Value = math.mul(rot.Value, quaternion.RotateY(math.radians(rotData.angularSpeed * dt)));
        }).ScheduleParallel(jbh1);

        Dependency = JobHandle.CombineDependencies(Dependency, jbh2);
    }
}
