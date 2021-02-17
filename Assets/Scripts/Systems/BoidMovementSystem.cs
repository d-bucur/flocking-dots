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
        Entities.ForEach((ref Translation transform, ref Rotation rotation, ref BoidComponent boidData, in FlockingComponent flocking) => {
            boidData.velocity *= steeringDataCaptured.drag;
            boidData.velocity += flocking.acceleration * deltaTime;
            var newPos = transform.Value + boidData.velocity;
            var worldSize = steeringDataCaptured.worldSize;
            if (newPos.x > worldSize) newPos.x = -worldSize;
            if (newPos.y > worldSize) newPos.y = -worldSize;
            if (newPos.z > worldSize) newPos.z = -worldSize;
            if (newPos.x < -worldSize) newPos.x = worldSize;
            if (newPos.y < -worldSize) newPos.y = worldSize;
            if (newPos.z < -worldSize) newPos.z = worldSize;
            transform.Value = newPos;
            rotation.Value = quaternion.LookRotationSafe(boidData.velocity, up);
        }).ScheduleParallel();
    }
}
