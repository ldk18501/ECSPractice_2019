using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DOTS_2019
{
    // public class LifeTimeSystem : ComponentSystem
    // {
    //     protected override void OnUpdate()
    //     {
    //         float dt = Time.DeltaTime;
    //         Entities.ForEach((ref LifeTimeData data) =>
    //         {
    //             data.existTime += dt;
    //         });
    //     }
    // }

    public class LifeTimeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            // Entities.ForEach((ref LifeTimeData data) =>
            // {
            //     data.existTime += dt;
            // }).Schedule();

            Entities.ForEach((ref LifeTimeData data) =>
            {
                data.existTime += dt;
            }).ScheduleParallel();
        }
    }
}