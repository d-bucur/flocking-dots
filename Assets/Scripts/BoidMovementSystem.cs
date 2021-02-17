using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(BoidFlockingSystem))]
public class BoidMovementSystem : SystemBase
{
    const float worldSize = 48; // TODO read from SO
    protected override void OnUpdate()
    {
        // Update positions
        var up = new float3(0, 1, 0);
        Entities.ForEach((ref Translation transform, ref Rotation rotation, ref VelocityTrackerComponent velocityTracker, in BoidComponent boidData) => {
            var newPos = transform.Value + boidData.velocity;
            if (newPos.x > worldSize) newPos.x = -worldSize;
            if (newPos.y > worldSize) newPos.y = -worldSize;
            if (newPos.z > worldSize) newPos.z = -worldSize;
            if (newPos.x < -worldSize) newPos.x = worldSize;
            if (newPos.y < -worldSize) newPos.y = worldSize;
            if (newPos.z < -worldSize) newPos.z = worldSize;
            transform.Value = newPos;
            rotation.Value = quaternion.LookRotationSafe(boidData.velocity, up);
            velocityTracker.velocity = boidData.velocity;
        }).ScheduleParallel();
    }
}
