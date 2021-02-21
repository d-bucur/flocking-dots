using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        var jobHandle = Entities
            .WithReadOnly(spatialGrid)
            .WithName("FlockingBehaviours")
            .ForEach((ref BoidAccelerationComponent acceleration, in Translation translation) => {
                acceleration = CalculateSteeringForces(translation, steeringDataCaptured, spatialGrid);
            })
            .ScheduleParallel(SpatialMap.Instance.gridWriterHandle);
        Dependency = JobHandle.CombineDependencies(Dependency, jobHandle);
    }

    private static BoidAccelerationComponent CalculateSteeringForces(in Translation translation,
        in FlockingData steeringConfig, in NativeMultiHashMap<int3, SpatialMapSingleValue> spatialGrid) {
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

        if (steeringConfig.isDebugEnabled) {
            Debug.DrawRay(position, alignment, Color.green);
            Debug.DrawRay(position, avoidance, Color.red);
            Debug.DrawRay(position, cohesion, Color.yellow);
            Debug.DrawRay(position, target, Color.blue);
            Debug.DrawRay(position, bounds, Color.magenta);
        }

        BoidAccelerationComponent acceleration;
        acceleration.Value = (alignment + avoidance + cohesion + target + bounds) * steeringConfig.flockingFactor;
        if (math.length(acceleration.Value) > steeringConfig.maxAcceleration) {
            acceleration.Value = math.normalizesafe(acceleration.Value) * steeringConfig.maxAcceleration;
        }

        return acceleration;
    }
}
