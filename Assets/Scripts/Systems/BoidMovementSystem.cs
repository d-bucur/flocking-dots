using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(BoidFlockingSystem))]
public class BoidMovementSystem : SystemBase
{
    private FlockingConfig steeringData;

    protected override void OnCreate() {
        steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
    }
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        var steeringDataCaptured = steeringData.data;
        var up = new float3(0, 1, 0);
        Entities.ForEach((ref Translation transform, ref Rotation rotation, ref BoidVelocityComponent velocity, in BoidAccelerationComponent acceleration) => {
            velocity.Value *= steeringDataCaptured.drag;
            velocity.Value += acceleration.Value * deltaTime;
            
            var newPos = transform.Value + velocity.Value;
            transform.Value = newPos;
            rotation.Value = quaternion.LookRotationSafe(velocity.Value, up);
        }).ScheduleParallel();
    }
}
