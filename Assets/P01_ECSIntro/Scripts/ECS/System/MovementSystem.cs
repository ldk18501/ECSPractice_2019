using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;

namespace DOTS_2019
{
    // public class MovementSystem : ComponentSystem
    // {
    //     protected override void OnUpdate()
    //     {
    //         float dt = Time.DeltaTime;
    //         Entities.ForEach((ref TransMoveData data, ref Translation trs) =>
    //         {
    //             trs.Value.y += data.speed * dt;
    //             if (trs.Value.y > 5f)
    //             {
    //                 data.speed = -1 * math.abs(data.speed);
    //             }
    //             if (trs.Value.y < -5f)
    //             {
    //                 data.speed = math.abs(data.speed);
    //             }
    //         });
    //     }
    // }

    public class MovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            Entities.ForEach((ref TransMoveData data, ref Translation trs) =>
            {
                trs.Value.y += data.speed * dt;
                if (trs.Value.y > 5f)
                {
                    data.speed = -1 * math.abs(data.speed);
                }
                if (trs.Value.y < -5f)
                {
                    data.speed = math.abs(data.speed);
                }
            }).ScheduleParallel();
        }
    }
}