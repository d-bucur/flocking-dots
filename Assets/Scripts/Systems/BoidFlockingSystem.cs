using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateAfter(typeof(SpatialHashingSystem))]  // TODO horrible dependencies
public class BoidFlockingSystem : SystemBase {
    private FlockingConfig steeringData;

    protected override void OnCreate() {
        steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
    }

    protected override void OnUpdate() {
        // Get spatial hashmap
        var spatialGrid = SpatialMap.Instance.spatialGrid;

        // Get acceleration from flocking behaviours
        var steeringDataCaptured = steeringData.data;
        var inputDependencies = JobHandle.CombineDependencies(
            SpatialMap.Instance.gridWriterHandle, ForwardRaycastSystem.writeHandle
        );
        var jobHandle = Entities
            .WithReadOnly(spatialGrid)
            .WithName("FlockingBehaviours")
            .ForEach((ref BoidAccelerationComponent acceleration, in Translation translation, in BoidForwardRaycastComponent rayResult) => {
                acceleration = CalculateSteeringForces(
                    translation, steeringDataCaptured, spatialGrid, rayResult
                    );
            })
            .ScheduleParallel(inputDependencies);
        Dependency = JobHandle.CombineDependencies(Dependency, jobHandle);
    }

    private static BoidAccelerationComponent CalculateSteeringForces(in Translation translation,
        in FlockingData steeringConfig, in NativeMultiHashMap<int3, SpatialMapSingleValue> spatialGrid,
        BoidForwardRaycastComponent rayResult) {
        var position = translation.Value;
        var avoidance = float3.zero;
        var cohesion = position;
        var alignment = float3.zero;

        int neighborCount = 0;
        var positionKey = SpatialMap.GetSpatialHash(position, steeringConfig.senseRange);
        var others = spatialGrid.GetValuesForKey(positionKey);
        foreach (var other in others) {
            // Debug.Log($"Entity {entity} vs {other}");
            var vectorToOther = position - other.Translation;
            var distance = math.length(vectorToOther);

            if (distance > steeringConfig.senseRange || distance == 0f) continue;
            neighborCount++;
            cohesion += other.Translation;
            avoidance += (steeringConfig.senseRange - distance) * vectorToOther;
            alignment += other.Velocity;
        }
        if (neighborCount > 0) {
            cohesion /= (float) neighborCount + 1;
            alignment /= neighborCount;
        }
        cohesion = (cohesion - position) * steeringConfig.cohesionFactor;
        alignment *= steeringConfig.alignmentFactor;
        avoidance *= steeringConfig.avoidanceFactor;
        
        var target = (steeringConfig.target - position) * steeringConfig.targetFactor;

        var bounds = float3.zero;
        if (math.abs(translation.Value.x) > steeringConfig.worldSize)
            bounds.x = steeringConfig.worldSize - translation.Value.x;
        if (math.abs(translation.Value.y) > steeringConfig.worldSize)
            bounds.y = steeringConfig.worldSize - translation.Value.y;
        if (math.abs(translation.Value.z) > steeringConfig.worldSize)
            bounds.z = steeringConfig.worldSize - translation.Value.z;
        bounds *= steeringConfig.boundsFactor;

        var obstacleAvoidance = float3.zero;
        if (math.all(rayResult.surfaceNormal != float3.zero)) {
            var dirToHit = rayResult.hitPosition - position;
            var reflection = math.reflect(dirToHit, rayResult.surfaceNormal);
            obstacleAvoidance = reflection + steeringConfig.obstacleAvoidanceFactor;
        }

        if (steeringConfig.isDebugEnabled) {
            Debug.DrawRay(position, alignment, Color.green);
            Debug.DrawRay(position, avoidance, Color.red);
            Debug.DrawRay(position, cohesion, Color.yellow);
            Debug.DrawRay(position, target, Color.blue);
            Debug.DrawRay(position, bounds, Color.magenta);
        }

        BoidAccelerationComponent acceleration;
        acceleration.Value = (alignment + avoidance + cohesion + target + bounds + obstacleAvoidance) * steeringConfig.flockingFactor;
        float accelLength = math.length(acceleration.Value);
        if (accelLength > steeringConfig.maxAcceleration) {
            acceleration.Value = math.normalizesafe(acceleration.Value) * steeringConfig.maxAcceleration;
        }
        else if (accelLength < steeringConfig.minAcceleration) {
            acceleration.Value = math.normalizesafe(acceleration.Value, new float3(1,1,1)) * steeringConfig.minAcceleration;
        }

        return acceleration;
    }
}
