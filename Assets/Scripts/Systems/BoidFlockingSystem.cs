using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BoidFlockingSystem : SystemBase {
    private NativeMultiHashMap<int3, Entity> spatialGrid;
    private FlockingConfig steeringData;

    protected override void OnCreate() {
        spatialGrid = new NativeMultiHashMap<int3, Entity>(100, Allocator.Persistent);
        steeringData = Resources.Load<FlockingConfig>("SteeringConfig");
    }

    protected override void OnUpdate() {
        // Create spatial hashmap
        var steeringDataCaptured = steeringData.data;
        /*spatialGrid.Clear();
        var boidQuery = GetEntityQuery(typeof(BoidComponent), typeof(Translation));
        if (spatialGrid.Capacity < boidQuery.CalculateEntityCount())
            spatialGrid.Capacity *= 2;
        var spatialGridWriter = spatialGrid.AsParallelWriter();
        Entities.ForEach((in BoidComponent boidData, in Translation translation, in Entity entity) => {
            var key = GetSpatialHash(translation.Value, steeringDataCaptured.senseRange);
            // Debug.Log($"Write {entity} to {key}");
            spatialGridWriter.Add(key, entity);
        }).Run();*/

        // Get acceleration from flocking behaviours
        var otherBoidsQuery = GetEntityQuery(
            ComponentType.ReadOnly<BoidComponent>(),
            ComponentType.ReadOnly<Translation>()
        );
        // TODO is there way to access query entities without allocating an array?
        var otherEntitiesArray = otherBoidsQuery.ToEntityArray(Allocator.TempJob);
        Entities
            .WithReadOnly(otherEntitiesArray)
            .ForEach((ref FlockingComponent flocking, in Translation translation, in Entity entity) => {
            var position = translation.Value;
            var avoidance = float3.zero;
            var cohesion = position;
            var alignment = float3.zero;
            var target = float3.zero;
            int neighborCount = 0;

            var otherBoids = GetComponentDataFromEntity<BoidComponent>(true);
            var otherTranslations = GetComponentDataFromEntity<Translation>(true);

            foreach (var other in otherEntitiesArray) {
                if (entity == other)
                    continue;
                // Debug.Log($"Entity {entity} vs {other}");
                var otherTranslation = otherTranslations[other];
                var vectorToOther = position - otherTranslation.Value;
                var distance = math.length(vectorToOther);
                            
                if (distance > steeringDataCaptured.senseRange) continue;
                neighborCount++;
                cohesion += otherTranslation.Value;
                avoidance += (steeringDataCaptured.senseRange - distance) * vectorToOther;
                var otherVelocity = otherBoids[other].velocity;
                alignment += otherVelocity;
            }
            /*var currentCell = GetSpatialHash(position, steeringDataCaptured.senseRange);
            for (var x = -1; x < 1; x++) {
                for (var y = -1; y < 1; y++)
                    for (var z = -1; z < 1; z++) {
                        var comparedCell = currentCell + new int3(x, y, z);
                        // var others = spatialGridCaptured.GetValuesForKey(comparedCell);
                        // TODO grid reading does not return correct result
                        
                    }
            }*/
            if (neighborCount > 0) {
                cohesion /= (float)neighborCount + 1;
                alignment /= neighborCount;
            }
            cohesion -= position;
            target = steeringDataCaptured.target - position;
            
            alignment *= steeringDataCaptured.alignmentFactor;
            avoidance *= steeringDataCaptured.avoidanceFactor;
            cohesion *= steeringDataCaptured.cohesionFactor;
            target *= steeringDataCaptured.targetFactor;

            if (steeringDataCaptured.isDebugEnabled) {
                Debug.DrawRay(position, alignment, Color.green);
                Debug.DrawRay(position, avoidance, Color.red);
                Debug.DrawRay(position, cohesion, Color.yellow);
                Debug.DrawRay(position, target, Color.blue);
            }

            flocking.acceleration = (alignment + avoidance + cohesion + target) * steeringDataCaptured.flockingFactor;
            })
            .WithDisposeOnCompletion(otherEntitiesArray)
            .ScheduleParallel();
    }

    protected override void OnDestroy() {
        spatialGrid.Dispose();
    }

    private static int3 GetSpatialHash(in float3 pos, float cellSize) {
        return new int3(
            (int)math.floor(pos.x/cellSize),
            (int)math.floor(pos.y/cellSize),
            (int)math.floor(pos.z/cellSize)
        );
    }
    
    
}
