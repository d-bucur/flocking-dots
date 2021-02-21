using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(SpatialHashingSystem))]  // TODO remove dependency
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
            .ForEach((ref BoidAccelerationComponent acceleration, in Translation translation, in Entity entity) => {
                // TODO break into smaller functions
                var position = translation.Value;
                var avoidance = float3.zero;
                var cohesion = position;
                var alignment = float3.zero;
                var target = float3.zero;
                var bounds = float3.zero;

                int neighborCount = 0;
                var positionKey = SpatialMap.GetSpatialHash(position, steeringDataCaptured.senseRange);
                var others = spatialGrid.GetValuesForKey(positionKey);
                foreach (var other in others) {
                    // Debug.Log($"Entity {entity} vs {other}");
                    var vectorToOther = position - other.Translation;
                    var distance = math.length(vectorToOther);
                
                    if (distance > steeringDataCaptured.senseRange || distance == 0f) continue;
                    neighborCount++;
                    cohesion += other.Translation;
                    avoidance += (steeringDataCaptured.senseRange - distance) * vectorToOther;
                    alignment += other.Velocity;
                }
                if (neighborCount > 0) {
                    cohesion /= (float)neighborCount + 1;
                    alignment /= neighborCount;
                }
                cohesion -= position;
                target = steeringDataCaptured.target - position;

                if (math.abs(translation.Value.x) > steeringDataCaptured.worldSize)
                    bounds.x = steeringDataCaptured.worldSize - translation.Value.x;
                if (math.abs(translation.Value.y) > steeringDataCaptured.worldSize)
                    bounds.y = steeringDataCaptured.worldSize - translation.Value.y;
                if (math.abs(translation.Value.z) > steeringDataCaptured.worldSize)
                    bounds.z = steeringDataCaptured.worldSize - translation.Value.z;

                alignment *= steeringDataCaptured.alignmentFactor;
                avoidance *= steeringDataCaptured.avoidanceFactor;
                cohesion *= steeringDataCaptured.cohesionFactor;
                target *= steeringDataCaptured.targetFactor;
                bounds *= steeringDataCaptured.boundsFactor;

                if (steeringDataCaptured.isDebugEnabled) {
                    Debug.DrawRay(position, alignment, Color.green);
                    Debug.DrawRay(position, avoidance, Color.red);
                    Debug.DrawRay(position, cohesion, Color.yellow);
                    Debug.DrawRay(position, target, Color.blue);
                    Debug.DrawRay(position, bounds, Color.magenta);
                }

                acceleration.Value = (alignment + avoidance + cohesion + target + bounds) * steeringDataCaptured.flockingFactor;
                if (math.length(acceleration.Value) > steeringDataCaptured.maxAcceleration) {
                    acceleration.Value = math.normalizesafe(acceleration.Value) * steeringDataCaptured.maxAcceleration;
                }
            })
            .ScheduleParallel(SpatialMap.Instance.gridWriterHandle);
        Dependency = JobHandle.CombineDependencies(Dependency, jobHandle);
    }
}
