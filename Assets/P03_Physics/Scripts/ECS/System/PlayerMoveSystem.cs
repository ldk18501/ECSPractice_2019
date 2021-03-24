using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class PlayerMoveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float2 curInput = new float2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        float dt = Time.DeltaTime;

        Entities.WithAll<PlayerTag>().ForEach((ref PhysicsVelocity vel, ref MoveForwardData speedData) =>
        {
            vel.Linear.xz = curInput * speedData.speed * dt;
        }).Schedule();
    }
}
