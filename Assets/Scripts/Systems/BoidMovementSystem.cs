using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
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
        Entities.ForEach((
            ref Rotation rotation, 
            ref PhysicsVelocity velocity,
            in BoidTag tag, 
            in BoidAccelerationComponent acceleration
        ) => {
            // velocity.Value *= steeringDataCaptured.drag; // TODO enable drag
            velocity.Linear += acceleration.Value * deltaTime;
            if (math.length(velocity.Linear) < steeringDataCaptured.minSpeed) {
                velocity.Linear = math.normalizesafe(velocity.Linear) * steeringDataCaptured.minSpeed;
            }
            rotation.Value = quaternion.LookRotationSafe(velocity.Linear, up);
        }).ScheduleParallel();
    }
}
